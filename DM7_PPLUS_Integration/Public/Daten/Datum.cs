namespace DM7_PPLUS_Integration.Daten
{
    public struct Datum
    {
        public Datum(int tag, int monat, int jahr)
        {
            Tag = tag;
            Monat = monat;
            Jahr = jahr;
        }

        public readonly int Tag;
        public readonly int Monat;
        public readonly int Jahr;
    }
}