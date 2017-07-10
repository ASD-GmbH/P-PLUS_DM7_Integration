using System;

namespace DM7_PPLUS_Integration.Daten
{
    public struct Mitarbeiterfoto
    {
        public Mitarbeiterfoto(Guid mitarbeiter, Guid format, byte[] foto)
        {
            Mitarbeiter = mitarbeiter;
            Format = format;
            Foto = foto;
        }

        public readonly Guid Mitarbeiter;
        public readonly Guid Format;
        public readonly byte[] Foto;
    }
}