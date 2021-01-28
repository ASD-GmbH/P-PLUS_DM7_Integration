namespace DM7_PPLUS_Integration.Daten
{
    public struct DM7_Mandantenzugehörigkeiten
    {
        public DM7_Mandantenzugehörigkeiten(int mandantId, Datum gueltigAb, Datum? gueltigBis)
        {
            MandantId = mandantId;
            GueltigAb = gueltigAb;
            GueltigBis = gueltigBis;
        }

        public readonly int MandantId;
        public readonly Datum GueltigAb;
        public readonly Datum? GueltigBis;
    }

}