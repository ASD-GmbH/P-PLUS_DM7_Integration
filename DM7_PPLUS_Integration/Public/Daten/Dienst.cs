using System;

namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Dienst
    {
        public readonly int Id;
        public readonly Guid Mandant;
        public readonly string Kurzbezeichnung;
        public readonly string Bezeichnung;
        public readonly Datum Gültig_ab;
        public readonly Datum? Gültig_bis;
        public readonly Uhrzeit Beginn;
        public readonly Dienst_Gültigkeit Gültig_an;
        public readonly bool Gelöscht;

        public Dienst(int id, Guid mandant, string kurzbezeichnung, string bezeichnung, Datum gültigAb, Datum? gültigBis, Uhrzeit beginn, Dienst_Gültigkeit gültigAn, bool gelöscht)
        {
            Id = id;
            Mandant = mandant;
            Kurzbezeichnung = kurzbezeichnung;
            Bezeichnung = bezeichnung;
            Gültig_ab = gültigAb;
            Gültig_bis = gültigBis;
            Beginn = beginn;
            Gültig_an = gültigAn;
            Gelöscht = gelöscht;
        }
    }
}