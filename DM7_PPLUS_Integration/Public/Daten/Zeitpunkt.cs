namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Zeitpunkt
    {
        public readonly Datum Datum;
        public readonly Uhrzeit Uhrzeit;

        public static Zeitpunkt DD_MM_YYYY_HH_MM(int tag, int monat, int jahr, int stunden, int minuten) =>
            new Zeitpunkt(Datum.DD_MM_YYYY(tag, monat, jahr), Uhrzeit.HH_MM(stunden, minuten));

        public Zeitpunkt(Datum datum, Uhrzeit uhrzeit)
        {
            Datum = datum;
            Uhrzeit = uhrzeit;
        }
    }
}