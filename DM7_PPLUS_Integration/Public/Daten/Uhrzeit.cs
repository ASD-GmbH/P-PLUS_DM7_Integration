using System;

namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Uhrzeit
    {
        public readonly int Stunden;
        public readonly int Minuten;

        public static Uhrzeit HH_MM(int stunden, int minuten)
        {
            if (stunden < 0 || stunden > 23 || minuten < 0 || minuten > 59) throw new FormatException($"Ungültige Uhrzeit {stunden}:{minuten}");
            return new Uhrzeit(stunden, minuten);
        }

        private Uhrzeit(int stunden, int minuten)
        {
            Stunden = stunden;
            Minuten = minuten;
        }
    }
}