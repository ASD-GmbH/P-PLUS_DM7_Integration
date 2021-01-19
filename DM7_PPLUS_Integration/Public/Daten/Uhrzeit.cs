namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Uhrzeit
    {
        public readonly int Stunden;
        public readonly int Minuten;

        public static Uhrzeit HHMM(int stunden, int minuten)
        {
            // TODO: Validierungscode
            return new Uhrzeit(stunden, minuten);
        }

        private Uhrzeit(int stunden, int minuten)
        {
            Stunden = stunden;
            Minuten = minuten;
        }
    }
}