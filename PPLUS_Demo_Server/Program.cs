using System;
using System.Diagnostics;
using System.Threading;
using DM7_PPLUS_Integration;
using DM7_PPLUS_Integration.Implementierung.Server;
using DM7_PPLUS_Integration.Implementierung.Testing;

namespace PPLUS_Demo_Server
{

    class Program
    {

        static void Main(string[] args)
        {
            var log = new Logger("");

            log.Info("Demoserver wird gestartet...");

            if (args.Length == 0) Show_info_and_terminate(log);

            var url = args[0];

            var privatekey = CryptoService.GenerateRSAKeyPair();

            if (url.Contains("|"))
            {
                var parts0 = url.Split('|');
                url = parts0[0];
                privatekey = parts0[1];
            }

            var parts = url.Split(':');
            if (parts.Length!=3) Show_info_and_terminate(log);

            var hostaddress = parts[0] + ":" + parts[1];
            int port;
            if (!Int32.TryParse(parts[2], out port)) Show_info_and_terminate(log);


            int intervall;
            if (args.Length < 2 || !Int32.TryParse(args[1], out intervall)) intervall = 60;        
            
            var publickey = CryptoService.GetPublicKey(privatekey);

            if (!args[0].Contains("|"))
            {
                log.Info($"PRIVATE KEY INFO ({hostaddress}:{port}|{privatekey}).");
            }

            var backend = new Demo_Datenserver(log, TimeSpan.FromSeconds(intervall));
            var host = DM7_PPLUS_Host.Starten(backend, new StaticAuthentication("user"), hostaddress, port, privatekey, log, log.OnError);

            log.Info($"Demoserver wurde gestartet ({hostaddress}:{port}|{publickey}).");

            var sub =
                host.Schicht_1_API_Version_1.Stand_Mitarbeiterdaten.Subscribe(new Observer<Stand>(stand =>
                {
                    log.Debug(
                        // ReSharper disable once AccessToDisposedClosure
                        $" - Mitarbeiterliste jetzt auf Stand {stand}, {host.Schicht_1_API_Version_1.Mitarbeiterdaten_abrufen().Result.Mitarbeiter.Count} Mitarbeiter.");
                }, log.OnError));

            Console.ReadKey();
            sub.Dispose();
            log.Info("Demoserver wird beendet...");
            host.Dispose();
            backend.Dispose();
            log.Info("Demoserver wurde beendet.");
            Thread.Sleep(2000);
        }

        private static void Show_info_and_terminate(Logger log)
        {
            log.Info("PPLUS_Demo_Server <url> [<intervall>]");
            log.Info("  <url>: bspw 'tcp://127.0.0.1:16000'");
            log.Info("  <intervall>: in Sekunden, bspw '60' (Standardwert)");
            Thread.Sleep(2000);
            Process.GetCurrentProcess().Kill();
        }
    }

    public class Logger : Log
    {
        private readonly string _prefix;

        public Logger(string prefix)
        {
            _prefix = prefix;
        }

        private string Meta => $"[{DateTime.Now.ToString("dd-HH:mm:ss.fff")}|{Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(3)}] ";

        public void Info(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Meta + _prefix + text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void OnError(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Out.WriteLine(ex.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadLine();
        }

        public void Debug(string text)
        {
            Console.WriteLine(Meta + _prefix + text);
        }
    }
}
