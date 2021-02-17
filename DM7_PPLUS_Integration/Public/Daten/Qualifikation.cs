namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Qualifikation
    {
        public Qualifikation(int stufe, string bezeichnung)
        {
            Stufe = stufe;
            Bezeichnung = bezeichnung;
        }

        public readonly int Stufe;
        public readonly string Bezeichnung;
    }
}