using System;
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

            var hostaddress = args.Length < 2 ? "tcp://127.0.0.1" : args[0];
            var port = args.Length < 2 ? 20000 : Int32.Parse(args[1]);

            var backend = new Demo_Datenserver(log);
            var host = DM7_PPLUS_Host.Starten(backend, hostaddress, port, log, log.OnError);

            log.Info($"Demoserver wurde gestartet ({hostaddress}).");

            var sub =
                host.Ebene_1_API_Level_1.Stand_Mitarbeiterdaten.Subscribe(new Observer<Stand>(stand =>
                {
                    log.Debug(
                        // ReSharper disable once AccessToDisposedClosure
                        $" - Mitarbeiterliste jetzt auf Stand {stand}, {host.Ebene_1_API_Level_1.Mitarbeiterdaten_abrufen().Result.Mitarbeiter.Count} Mitarbeiter.");
                }, log.OnError));

            Console.ReadKey();
            sub.Dispose();
            log.Info("Demoserver wird beendet...");
            host.Dispose();
            backend.Dispose();
            log.Info("Demoserver wurde beendet.");
            Thread.Sleep(2000);
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
