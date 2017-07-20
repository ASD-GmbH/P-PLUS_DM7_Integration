using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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


            var hostaddress = "tcp://127.0.0.1";
            var port = 20000;

            var backend = new Demo_Datenserver();
            var host = DM7_PPLUS_Host.Starten(backend, hostaddress, port, log, log.OnError);

            log.Info($"Demoserver wurde gestartet ({host.Url}).");
            Console.ReadKey();
            log.Info("Demoserver wird beendet...");
            host.Dispose();
            log.Info("Demoserver wurde beendet.");
            Thread.Sleep(500);
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
