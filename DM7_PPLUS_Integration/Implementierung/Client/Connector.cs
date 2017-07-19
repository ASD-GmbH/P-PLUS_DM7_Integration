using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;
using DM7_PPLUS_Integration.Implementierung.Testing;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    public interface ProxyFactory
    {
        Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>> Connect(string networkAddress, Log log);
    }

    public static class Connector
    {
        public static Task<Level_0_Test_API> Instance_API_level_0_nur_fuer_Testzwecke(string networkAddress, Log log, ProxyFactory factory = null)
        {

            var client_min_api_level_request = 0;
            var client_max_api_level_request = 0;

            return
                Verbindungsaufbau(networkAddress, client_min_api_level_request, client_max_api_level_request, factory, log)
                    .ContinueWith(task => (Level_0_Test_API)new Level_0_Test_Proxy(task.Result.Item1));
        }

        public static Task<DM7_PPLUS_API> Instance_API_Level_1(string networkAddress, Log log, ProxyFactory factory = null)
        {

            var client_min_api_level_request = networkAddress == "test://0" ? 0 : 1;
            var client_max_api_level_request = 1;

            return
                Verbindungsaufbau(networkAddress, client_min_api_level_request, client_max_api_level_request, factory, log)
                    .ContinueWith(task =>
                        task.Result.Item2 != 1
                        ? (DM7_PPLUS_API)new Level_1_upgrade_Test_Proxy(task.Result.Item1)
                        : (DM7_PPLUS_API)new API_Level_1_Proxy(task.Result.Item1, task.Result.Item3, task.Result.Item4));
        }


        private static Task<Tuple<Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung, int, Guid, int>> Verbindungsaufbau(string networkaddress, int client_min_api_level_request, int client_max_api_level_request, ProxyFactory factory, Log log)
        {
            var login = "test";

            if (networkaddress.StartsWith("tcp://"))
            {
                factory = new NetMqfactory();
            }

            var verbindung_tuple = factory.Connect(networkaddress, log);

            return verbindung_tuple.ContinueWith(task =>
            {
                var result = task.Result.Item1
                    .Connect(login, client_max_api_level_request, client_min_api_level_request).Result;
                if (result is ConnectionSucceeded)
                {
                    var success = (ConnectionSucceeded)result;
                    var agreed_API_level = success.Api_Level;

                    Guard_Api_Level(agreed_API_level, client_min_api_level_request, client_max_api_level_request);

                    return new Tuple<Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung, int, Guid, int>(
                        verbindung_tuple.Result.Item2,
                        agreed_API_level,
                        success.Session,
                        success.Auswahllistenversion);
                }
                else if (result is ConnectionFailed)
                {
                    var failure = (ConnectionFailed)result;
                    switch (failure.Reason)
                    {
                        case ConnectionFailure.Internal_Server_Error:
                            throw new ConnectionErrorException(
                                $"Fehler beim Verbindungsaufbau mit P-PLUS ({failure.Info})");
                        case ConnectionFailure.Unable_to_provide_API_level:
                            throw new UnsupportedVersionException(
                                $"Der P-PLUS Server unterstützt diese Version der DM7 Software noch nicht oder nicht mehr ({failure.Info}).");
                        case ConnectionFailure.Unsupported_Connection_Protocol:
                            throw new UnsupportedVersionException(
                                $"Der P-PLUS Server kann zu dieser Version der DM7 Software keine Verbindung aufbauen ({failure.Info}).");
                    }
                }
                throw new ConnectionErrorException(
                    $"Fehler beim Verbindungsaufbau mit P-PLUS (unbekannte Antwort: {result.GetType().Name})");

            });
        }


        private static void Guard_Api_Level(int api_level, int client_min_api_level_request, int client_max_api_level_request)
        {
            if (!(client_min_api_level_request <= api_level && api_level <= client_max_api_level_request))
            {
                throw new UnsupportedVersionException($"P-PLUS API Level {api_level} wird nicht unterstützt.");
            }
        }
    }
}