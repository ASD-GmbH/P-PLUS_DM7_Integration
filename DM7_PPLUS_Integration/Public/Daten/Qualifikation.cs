namespace DM7_PPLUS_Integration.Daten
{
    public struct Qualifikation
    {
        // TODO: ID einpflegen, wenn die Datenhoheit geklärt ist.
        public Qualifikation(int stufe, string bezeichnung, Datum gueltigAb, Datum? gueltigBis)
        {
            Stufe = stufe;
            Bezeichnung = bezeichnung;
            GueltigAb = gueltigAb;
            GueltigBis = gueltigBis;
        }

        public readonly int Stufe;
        public readonly string Bezeichnung;
        public readonly Datum GueltigAb;
        public readonly Datum? GueltigBis;        
    }
}