using System;
using System.Threading.Tasks;

namespace DM7_PPLUS_Integration.Implementierung
{
    public interface ProxyFactory
    {
        Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>> Connect(string networkAddress, Log log);
    }

    public static class Connector
    {
        public static Task<DM7_PPLUS_API> Instance(string networkAddress, Log log, ProxyFactory factory = null)
        {
            var verbindung = (factory ?? new NetMqfactory()).Connect(networkAddress, log);
            return verbindung.ContinueWith(task =>
            {
                var parts = networkAddress.Split('|');
                if (parts.Length > 2 && parts[0] == "test://")
                {
                    var client_min_api_level_request = int.Parse(parts[1]);
                    var client_max_api_level_request = int.Parse(parts[2]);
                    var result = task.Result.Item1.Connect("test", client_max_api_level_request, client_min_api_level_request).Result;
                    if (result is ConnectionSucceeded)
                    {
                        var success = (ConnectionSucceeded) result;
                        var possible_client_api_level = success.Api_Level;
                        Guard_Api_Level(possible_client_api_level, client_min_api_level_request, client_max_api_level_request);

                        return
                            possible_client_api_level == 1
                                ? (DM7_PPLUS_API) new API_Level_1_Proxy(task.Result.Item2, success.Session, success.Auswahllistenversion)
                                : (DM7_PPLUS_API) new API_Level_2_Proxy(task.Result.Item2, success.Session, success.Auswahllistenversion);
                    }
                    else if (result is ConnectionFailed)
                    {
                        var failure = (ConnectionFailed) result;
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
                }
                throw new UnsupportedVersionException(
                    $"Das Protokoll der Netzwerkadresse {networkAddress} wird nicht unterstützt.");
            });
        }

        private static void Guard_Api_Level(int possible_client_api_level, int client_min_api_level_request, int client_max_api_level_request)
        {
            if (!(possible_client_api_level >= client_min_api_level_request && possible_client_api_level <= client_max_api_level_request))
                throw new UnsupportedVersionException($"P-PLUS API Level {possible_client_api_level} wird nicht unterstützt.");
        }
    }

    public class NetMqfactory : ProxyFactory
    {
        public Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>> Connect(string networkAddress, Log log)
        {
            throw new NotImplementedException();
        }
    }
}