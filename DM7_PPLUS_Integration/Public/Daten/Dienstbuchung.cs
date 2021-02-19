using System;

namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Dienstbuchung
    {
        public readonly Guid MitarbeiterId;
        public readonly int DienstId;
        public readonly Uhrzeit Beginnt_um;

        public Dienstbuchung(Guid mitarbeiterId, int dienstId, Uhrzeit beginntUm)
        {
            MitarbeiterId = mitarbeiterId;
            DienstId = dienstId;
            Beginnt_um = beginntUm;
        }
    }
}