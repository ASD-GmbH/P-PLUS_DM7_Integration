using System;
using System.Collections.ObjectModel;

namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Dienst
    {
        public readonly int Id;
        public readonly ReadOnlyCollection<DM7_Mandantenzugehörigkeit> Mandantenzugehörigkeiten;
        public readonly string Kurzbezeichnung;
        public readonly string Bezeichnung;
        public readonly Datum Gültig_ab;
        public readonly Datum? Gültig_bis;
        public readonly Dienst_Gültigkeit Gültig_an;
        public readonly bool Gelöscht;

        public Dienst(int id, ReadOnlyCollection<DM7_Mandantenzugehörigkeit> mandantenzugehörigkeiten, string kurzbezeichnung, string bezeichnung, Datum gültigAb, Datum? gültigBis, Dienst_Gültigkeit gültigAn, bool gelöscht)
        {
            Id = id;
            Mandantenzugehörigkeiten = mandantenzugehörigkeiten;
            Kurzbezeichnung = kurzbezeichnung;
            Bezeichnung = bezeichnung;
            Gültig_ab = gültigAb;
            Gültig_bis = gültigBis;
            Gültig_an = gültigAn;
            Gelöscht = gelöscht;
        }
    }
}