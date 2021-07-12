using System;

namespace DM7_PPLUS_Integration.Daten
{
    public enum Abwesenheitsart
    {
        Fehlzeit,
        Anderweitig_verplant
    }

    public readonly struct Abwesenheit
    {
        public readonly Guid MitarbeiterId;
        public readonly Zeitpunkt Abwesend_ab;
        public readonly Zeitpunkt? Vorraussichtlich_wieder_verfügbar_ab;
        public readonly string Grund;
        public readonly Abwesenheitsart Art;

        public Abwesenheit(Guid mitarbeiterId, Zeitpunkt abwesendAb, Zeitpunkt? vorraussichtlichWiederVerfügbarAb, string grund, Abwesenheitsart art)
        {
            MitarbeiterId = mitarbeiterId;
            Abwesend_ab = abwesendAb;
            Vorraussichtlich_wieder_verfügbar_ab = vorraussichtlichWiederVerfügbarAb;
            Grund = grund;
            Art = art;
        }
    }
}