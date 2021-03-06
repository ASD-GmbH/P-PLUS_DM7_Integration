﻿using System;

namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct DM7_Mandantenzugehörigkeit
    {
        public DM7_Mandantenzugehörigkeit(Guid mandantId, Datum gueltigAb, Datum? gueltigBis)
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