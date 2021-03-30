using System;
using System.Collections.ObjectModel;

namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Soll_Ist_Abgleich
    {
        public readonly Datum Datum;
        public readonly ReadOnlyCollection<Ungeplante_Tour> Ungeplante_Touren_ohne_Tourenstamm;
        public readonly ReadOnlyCollection<Geplante_Tour> Geplante_Touren;
        public readonly ReadOnlyCollection<Nicht_gefahrene_Tour> Nicht_gefahrene_Touren;

        public Soll_Ist_Abgleich(Datum datum, ReadOnlyCollection<Ungeplante_Tour> ungeplante_Touren_ohne_Tourenstamm, ReadOnlyCollection<Geplante_Tour> geplante_Touren, ReadOnlyCollection<Nicht_gefahrene_Tour> nicht_gefahrene_Touren)
        {
            Datum = datum;
            Ungeplante_Touren_ohne_Tourenstamm = ungeplante_Touren_ohne_Tourenstamm;
            Geplante_Touren = geplante_Touren;
            Nicht_gefahrene_Touren = nicht_gefahrene_Touren;
        }
    }

    public readonly struct Ungeplante_Tour
    {
        public readonly Guid MitarbeiterId;
        public readonly Guid MandantId;
        public readonly ReadOnlyCollection<Einsatz> Einsätze;

        public Ungeplante_Tour(Guid mitarbeiterId, Guid mandantId, ReadOnlyCollection<Einsatz> einsätze)
        {
            MitarbeiterId = mitarbeiterId;
            MandantId = mandantId;
            Einsätze = einsätze;
        }
    }

    public readonly struct Geplante_Tour
    {
        public readonly Guid MitarbeiterId;
        public readonly Guid MandantId;
        public readonly int Dienst;
        public readonly ReadOnlyCollection<Einsatz> Einsätze;

        public Geplante_Tour(Guid mitarbeiterId, Guid mandantId, int dienst, ReadOnlyCollection<Einsatz> einsätze)
        {
            MitarbeiterId = mitarbeiterId;
            MandantId = mandantId;
            Dienst = dienst;
            Einsätze = einsätze;
        }
    }

    public readonly struct Nicht_gefahrene_Tour
    {
        public readonly Guid MitarbeiterId;
        public readonly Guid MandantId;
        public readonly int Dienst;

        public Nicht_gefahrene_Tour(Guid mitarbeiterId, Guid mandantId, int dienst)
        {
            MitarbeiterId = mitarbeiterId;
            MandantId = mandantId;
            Dienst = dienst;
        }
    }

    public readonly struct Einsatz
    {
        public readonly Relative_Zeit Beginn;
        public readonly uint Dauer_in_Minuten;
        public readonly uint Anfahrtsdauer_in_Minuten;
        public readonly uint Abfahrtsdauer_in_Minuten;
        public readonly Einsatzart Art;

        public Einsatz(Relative_Zeit beginn, uint dauerInMinuten, uint anfahrtsdauerInMinuten, uint abfahrtsdauerInMinuten, Einsatzart art)
        {
            Beginn = beginn;
            Dauer_in_Minuten = dauerInMinuten;
            Anfahrtsdauer_in_Minuten = anfahrtsdauerInMinuten;
            Abfahrtsdauer_in_Minuten = abfahrtsdauerInMinuten;
            Art = art;
        }
    }

    public interface Einsatzart { }

    public readonly struct Klient_Einsatz : Einsatzart { }

    public readonly struct Sonstige_Zeit : Einsatzart
    {
        public readonly Guid Leistung;
        
        public Sonstige_Zeit(Guid leistung)
        {
            Leistung = leistung;
        }
    }

    public readonly struct Pause : Einsatzart { }

    public readonly struct Unterbrechung : Einsatzart { }
}