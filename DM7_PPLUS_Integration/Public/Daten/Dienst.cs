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

    public readonly struct Dienst_Gültigkeit
    {
        public readonly bool Montag;
        public readonly bool Dienstag;
        public readonly bool Mittwoch;
        public readonly bool Donnerstag;
        public readonly bool Freitag;
        public readonly bool Samstag;
        public readonly bool Sonntag;
        public readonly bool Feiertags;

        public Dienst_Gültigkeit(bool montag, bool dienstag, bool mittwoch, bool donnerstag, bool freitag, bool samstag, bool sonntag, bool feiertags)
        {
            Montag = montag;
            Dienstag = dienstag;
            Mittwoch = mittwoch;
            Donnerstag = donnerstag;
            Freitag = freitag;
            Samstag = samstag;
            Sonntag = sonntag;
            Feiertags = feiertags;
        }
    }
}