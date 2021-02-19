namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Qualifikation
    {
        public Qualifikation(int stufe, string bezeichnung, Datum gültigAb, Datum? gültigBis)
        {
            Stufe = stufe;
            Bezeichnung = bezeichnung;
            Gültig_ab = gültigAb;
            Gültig_bis = gültigBis;
        }

        public readonly int Stufe;
        public readonly string Bezeichnung;
        public readonly Datum Gültig_ab;
        public readonly Datum? Gültig_bis;
    }
}