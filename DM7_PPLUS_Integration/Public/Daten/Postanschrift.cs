using System;

namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Postanschrift
    {
        public Postanschrift(Guid id, string strasse, string postleitzahl, string ort, string land)
        {
            Id = id;
            Strasse = strasse;
            Postleitzahl = postleitzahl;
            Ort = ort;
            Land = land;
        }

        public readonly Guid Id;
        public readonly string Strasse;
        public readonly string Postleitzahl;
        public readonly string Ort;
        public readonly string Land;
    }
}