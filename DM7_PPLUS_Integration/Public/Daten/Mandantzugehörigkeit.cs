using System;

namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct DM7_Mandantenzugehörigkeiten
    {
        public DM7_Mandantenzugehörigkeiten(Guid mandantId, Datum gueltigAb, Datum? gueltigBis)
        {
            MandantId = mandantId;
            GueltigAb = gueltigAb;
            GueltigBis = gueltigBis;
        }

        public readonly Guid MandantId;
        public readonly Datum GueltigAb;
        public readonly Datum? GueltigBis;
    }

}