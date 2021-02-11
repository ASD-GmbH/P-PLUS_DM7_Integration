using System;
using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Server;
using DM7_PPLUS_Integration.Implementierung.Shared;
using DM7_PPLUS_Integration.Implementierung.Testing;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    internal static class Connector
    {
        public static Task<Version_0_Test_API> Instance_API_version_0_nur_fuer_Testzwecke(string networkAddress, string login, Log log, CancellationToken cancellationToken_Verbindung, Schicht_2_Proxy_Factory factory = null)
        {

            var client_min_api_version_request = 0;
            var client_max_api_version_request = 0;

            var disposegroup = new DisposeGroup();
            disposegroup.With(() => { log.Info("DM7/P-PLUS Schnittstelle geschlossen."); });
            return
                Verbindungsaufbau(networkAddress, login, client_min_api_version_request, client_max_api_version_request, factory, log, disposegroup, cancellationToken_Verbindung)
                    .ContinueWith(task => (Version_0_Test_API)new Version_0_Test_Proxy(task.Result.Item1), cancellationToken_Verbindung);
        }

        public static Task<Legacy_DM7_PPLUS_API> Instance_API_Version_1(string networkAddress, string credentials, Log log, CancellationToken cancellationToken_Verbindung, Schicht_2_Proxy_Factory factory = null)
        {

            var client_min_api_version_request = networkAddress == "test://0" ? 0 : 1;
            var client_max_api_version_request = 1;

            var disposegroup = new DisposeGroup();
            disposegroup.With(() => { log.Info("DM7/P-PLUS Schnittstelle geschlossen."); });
            return
                Verbindungsaufbau(networkAddress, credentials, client_min_api_version_request, client_max_api_version_request, factory, log, disposegroup, cancellationToken_Verbindung)
                    .ContinueWith(task =>
                    {
                        Legacy_DM7_PPLUS_API api = null;
                        if (task.Result.Item2 == 0) api = new Version_1_upgrade_Test_Proxy(task.Result.Item1);
                        if (task.Result.Item2 == 1) api = new API_Version_1_Proxy(credentials, task.Result.Item1, task.Result.Item3, log, disposegroup);
                        if (api == null) throw new UnsupportedVersionException($"Vereinbartes API Version entspricht nicht den Rahmenbedingungen: {task.Result.Item2}");
                        return api;
                    }, cancellationToken_Verbindung);
        }


        public static Task<Legacy_DM7_PPLUS_API> Instance_API_Version_3(string networkAddress, string credentials, Log log, CancellationToken cancellationToken_Verbindung, Schicht_2_Proxy_Factory factory = null)
        {

            var client_min_api_version_request = networkAddress == "test://0" ? 0 : 1;
            var client_max_api_version_request = 3;

            var disposegroup = new DisposeGroup();
            disposegroup.With(() => { log.Info("DM7/P-PLUS Schnittstelle geschlossen."); });
            return
                Verbindungsaufbau(networkAddress, credentials, client_min_api_version_request, client_max_api_version_request, factory, log, disposegroup, cancellationToken_Verbindung)
                    .ContinueWith(task =>
                    {
                        Legacy_DM7_PPLUS_API api = null;
                        if (task.Result.Item2 == 0) api = new Version_1_upgrade_Test_Proxy(task.Result.Item1);
                        if (task.Result.Item2 == 1) api = new API_Version_1_Proxy(credentials, task.Result.Item1, task.Result.Item3, log, disposegroup);
                        if (task.Result.Item2 == 3) api = new API_Version_3_Proxy(credentials, task.Result.Item1, task.Result.Item3, log, disposegroup);
                        if (api==null) throw new UnsupportedVersionException($"Vereinbartes API Version entspricht nicht den Rahmenbedingungen: {task.Result.Item2}");
                        return api;
                    }, cancellationToken_Verbindung);
        }


        private static Task<Tuple<Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung, int, int>> Verbindungsaufbau(string networkaddress, string credentials, int client_min_api_version_request, int client_max_api_version_request, Schicht_2_Proxy_Factory factory, Log log, DisposeGroup disposegroup, CancellationToken cancellationToken_Verbindung)
        {
            log.Info("DM7/P-PLUS Integrationsschnittstelle - " + Version.VersionString);

            if (networkaddress.StartsWith("tcp://"))
            {
                factory = new NetMQ_Factory(disposegroup);
            }

            if (networkaddress.StartsWith("demo://"))
            {
                int intervall;
                if (!int.TryParse(networkaddress.Substring(7), out intervall)) intervall = 60;
                var host = DM7_PPLUS_Host.Starten(new Demo_Datenserver(log, TimeSpan.FromSeconds(intervall)).WithDisposeGroup(disposegroup), new StaticAuthentication("anonymous"), log, ex => { throw new Exception("Unerwarteter Fehler", ex); }).WithDisposeGroup(disposegroup);
                factory = new LoopbackFactory(host, networkaddress.EndsWith("2") ? 2 : 3, disposegroup);
            }

            if (factory==null) throw new ConnectionErrorException("Unbekanntes Protokoll im Connection string gefunden. Erwartet wird tcp://...");

            var info = networkaddress.StartsWith("tcp://") ? "DM7/P-PLUS Verbindung über NetMQ" : "Test/Demo Verbindung";
            log.Info(info + " wird aufgebaut...");
            var verbindung_tuple = factory.Connect_Schicht_2(networkaddress, credentials, log, cancellationToken_Verbindung);

            return verbindung_tuple.ContinueWith(task =>
            {
                log.Info("DM7 wird angemeldet...");
                var result = task.Result.Item1.Connect_Schicht_1(credentials, client_max_api_version_request, client_min_api_version_request).Result;

                var succeeded = result as ConnectionSucceeded;
                if (succeeded != null)
                {
                    var success = succeeded;
                    var agreed_API_version = success.Api_Version;

                    log.Info($"DM7/P-PLUS Verbindung (API {agreed_API_version}, IDs {success.Auswahllistenversion}).");

                    Guard_Api_Version(agreed_API_version, client_min_api_version_request, client_max_api_version_request);

                    return new Tuple<Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung, int, int>(
                        verbindung_tuple.Result.Item2,
                        agreed_API_version,
                        success.Auswahllistenversion);
                }

                var failed = result as ConnectionFailed;
                if (failed != null)
                {
                    var failure = failed;
                    switch (failure.Reason)
                    {
                        case ConnectionFailure.Internal_Server_Error:
                            throw new ConnectionErrorException(
                                $"Fehler beim Verbindungsaufbau mit P-PLUS ({failure.Info})");
                        case ConnectionFailure.Invalid_Server_Key:
                            throw new ConnectionErrorException(
                                $"Fehler beim Verbindungsaufbau mit P-PLUS (Schlüssel-Fehler, {failure.Info})");
                        case ConnectionFailure.Unauthorized:
                            throw new ConnectionErrorException(
                                $"Fehler beim Verbindungsaufbau mit P-PLUS (nicht autorisiert, {failure.Info})");
                        case ConnectionFailure.Unable_to_provide_API_version:
                            throw new UnsupportedVersionException(
                                $"Der P-PLUS Server unterstützt diese Version der DM7 Software noch nicht oder nicht mehr ({failure.Info}).");
                        case ConnectionFailure.Unsupported_Connection_Protocol:
                            throw new UnsupportedVersionException(
                                $"Der P-PLUS Server kann zu dieser Version der DM7 Software keine Verbindung aufbauen ({failure.Info}).");
                    }
                }

                throw new ConnectionErrorException(
                    $"Fehler beim Verbindungsaufbau mit P-PLUS (unbekannte Antwort: {result.GetType().Name})");

            }, cancellationToken_Verbindung);
        }


        // ReSharper disable UnusedParameter.Local
        private static void Guard_Api_Version(int api_version, int client_min_api_version_request, int client_max_api_version_request)
        // ReSharper restore UnusedParameter.Local
        {
            if (!(client_min_api_version_request <= api_version && api_version <= client_max_api_version_request))
            {
                throw new UnsupportedVersionException($"P-PLUS API Version {api_version} wird nicht unterstützt.");
            }
        }
    }
}