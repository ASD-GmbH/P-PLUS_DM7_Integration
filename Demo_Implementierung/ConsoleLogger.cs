using System;
using DM7_PPLUS_Integration;

namespace Demo_Implementierung
{
    /// <summary>
    /// Beispiel des durch DM zu implementierenden Log, das von P-PLUS genutzt wird, um betriebliche Meldungen auszugeben (z.B. Verbindungsstatus etc.).
    /// </summary>
    internal class ConsoleLogger : Log
    {
        public void Info(string text)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Out.WriteLine(text);
        }

        public void Debug(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Out.WriteLine(text);
        }
    }
}