using System;

namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Leistung
    {
        public readonly Guid Id;
        public readonly string Bezeichnung;

        public Leistung(Guid id, string bezeichnung)
        {
            Id = id;
            Bezeichnung = bezeichnung;
        }
    }
}