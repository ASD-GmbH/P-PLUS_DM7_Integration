using System;

namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct DM7_Mandant
    {
        public readonly Guid Id;
        public readonly string Bezeichnung;

        public DM7_Mandant(Guid id, string bezeichnung)
        {
            Id = id;
            Bezeichnung = bezeichnung;
        }
    }
}