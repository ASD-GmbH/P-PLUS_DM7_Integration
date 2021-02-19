namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Datum
    {
        public readonly int Tag;
        public readonly int Monat;
        public readonly int Jahr;

        public static Datum DD_MM_YYYY(int tag, int monat, int jahr) => new Datum(tag, monat, jahr);

        public Datum(int tag, int monat, int jahr)
        {
            Tag = tag;
            Monat = monat;
            Jahr = jahr;
        }
    }
}