using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Messages.PPLUS;
using Datenstand = DM7_PPLUS_Integration.Daten.Datenstand;
using Datum = DM7_PPLUS_Integration.Daten.Datum;
using Uhrzeit = DM7_PPLUS_Integration.Daten.Uhrzeit;
using Zeitpunkt = DM7_PPLUS_Integration.Daten.Zeitpunkt;

namespace DM7_PPLUS_Integration.Implementierung.PPLUS
{
    public static class Message_mapper
    {
        public static MitarbeiterlisteV1 Mitarbeiterstammdaten_als_Message(Stammdaten<Mitarbeiter> mitarbeiter) =>
            new MitarbeiterlisteV1(
                Liste_als_Message(mitarbeiter, Mitarbeiter_als_Message),
                Stand_als_Message(mitarbeiter.Stand));

        public static Stammdaten<Mitarbeiter> Mitarbeiterlist_als_Stammdaten(MitarbeiterlisteV1 mitarbeiter) =>
            new Stammdaten<Mitarbeiter>(
                Liste_aus(mitarbeiter.Mitarbeiter, Mitarbeiter_aus),
                Stand_aus(mitarbeiter.Stand));

        public static DiensteV1 Dienste_als_Message(Stammdaten<Dienst> dienste) =>
            new DiensteV1(
                Liste_als_Message(dienste, Dienst_als_Message),
                Stand_als_Message(dienste.Stand));

        public static Stammdaten<Dienst> Dienste_als_Stammdaten(DiensteV1 dienste) =>
            new Stammdaten<Dienst>(
                Liste_aus(dienste.Dienste, Dienst_aus),
                Stand_aus(dienste.Stand));

        public static DienstbeginnV1 Dienstbeginn_als_Message(Uhrzeit? beginn) =>
            new DienstbeginnV1(Option_Map(beginn, Uhrzeit_als_Message));

        public static Uhrzeit? Dienstbeginn_aus_Message(DienstbeginnV1 beginn) =>
            Option_Map(beginn.Value, Uhrzeit_aus);

        public static Messages.PPLUS.Datenstand Stand_als_Message(Datenstand stand) => new Messages.PPLUS.Datenstand(stand.Value);
        public static Datenstand Stand_aus(Messages.PPLUS.Datenstand stand) => new Datenstand(stand.Value);

        public static Dictionary<Datum, ReadOnlyCollection<Dienstbuchung>> Dienstbuchungen(DienstbuchungenV1 dienstbuchungen) =>
            Liste_aus(dienstbuchungen.Value, kv => new KeyValuePair<Datum, ReadOnlyCollection<Dienstbuchung>>(Datum_aus(kv.Datum), Liste_aus(kv.Dienstbuchungen, Dienstbuchung_aus)))
                .ToDictionary(_ => _.Key, _ => _.Value);
        public static DienstbuchungenV1 Dienstbuchungen_als_Message(Dictionary<Datum, ReadOnlyCollection<Dienstbuchung>> dienstbuchungen) =>
            new DienstbuchungenV1(Liste_als_Message(dienstbuchungen, kv => new DienstbuchungenV1ValueStruct(Datum_als_Message(kv.Key), Liste_als_Message(kv.Value, Dienstbuchung_als_Message))));

        public static ReadOnlyCollection<Abwesenheit> Abwesenheiten(AbwesenheitenV1 abwesenheiten) => Liste_aus(abwesenheiten.Value, Abwesenheit_aus);
        public static AbwesenheitenV1 Abwesenheiten_als_Message(ReadOnlyCollection<Abwesenheit> abwesenheiten) => new AbwesenheitenV1(Liste_als_Message(abwesenheiten, Abwesenheit_als_Message));

        public static SollIstAbgleichV1 Soll_Ist_Abgleich_als_Message(Soll_Ist_Abgleich abgleich) =>
            new SollIstAbgleichV1(
                Datum_als_Message(abgleich.Datum),
                Liste_als_Message(abgleich.Ungeplante_Touren_ohne_Tourenstamm, Ungeplante_Tour_als_Message),
                Liste_als_Message(abgleich.Geplante_Touren, Geplante_Tour_als_Message),
                Liste_als_Message(abgleich.Nicht_gefahrene_Touren, Nicht_gefahrene_Tour_als_Message));

        public static Soll_Ist_Abgleich Soll_Ist_Abgleich_aus(SollIstAbgleichV1 abgleich) =>
            new Soll_Ist_Abgleich(
                Datum_aus(abgleich.Datum),
                Liste_aus(abgleich.UngeplanteTourenOhneTourenstamm, Ungeplante_Tour_aus),
                Liste_aus(abgleich.GeplanteTouren, Geplante_Tour_aus),
                Liste_aus(abgleich.NichtGefahreneTouren, Nicht_gefahrene_Tour_aus));

        public static SollIstAbgleichVerarbeitungsergebnisV1 Soll_Ist_Abgleich_Verarbeitungsergebnis_als_Message(Soll_Ist_Abgleich_Verarbeitungsergebnis ergebnis)
        {
            switch (ergebnis)
            {
                case Verarbeitet _: return new VerarbeitetV1();
                case Dienstplanabschluss_verhindert_Verarbeitung e: return new DienstplanabschlussVerhindertVerarbeitungV1(Liste_als_Message(e.Dienstplanabschlüsse, Dienstplanabschluss_als_Message));
                default: throw new ArgumentOutOfRangeException(nameof(ergebnis), ergebnis, null);
            }
        }

        public static Soll_Ist_Abgleich_Verarbeitungsergebnis Soll_Ist_Abgleich_Verarbeitungsergebnis(SollIstAbgleichVerarbeitungsergebnisV1 ergebnis)
        {
            switch (ergebnis)
            {
                case VerarbeitetV1 _: return new Verarbeitet();
                case DienstplanabschlussVerhindertVerarbeitungV1 e: return new Dienstplanabschluss_verhindert_Verarbeitung(Liste_aus(e.Value, Dienstplanabschluss_aus));
                default: throw new ArgumentOutOfRangeException(nameof(ergebnis), ergebnis, null);
            }
        }

        private static MitarbeiterV1 Mitarbeiter_als_Message(Mitarbeiter mitarbeiter) =>
            new MitarbeiterV1(
                UUID_aus(mitarbeiter.Id),
                Liste_als_Message(mitarbeiter.DM7_Mandantenzugehörigkeiten, DM7_Mandantenzugehörigkeit_als_Message),
                UUID_aus(mitarbeiter.Titel),
                mitarbeiter.Vorname,
                mitarbeiter.Nachname,
                Option_Map(mitarbeiter.Postanschrift, Postanschrift_als_Message),
                Option_Map(mitarbeiter.Geburtstag, Datum_als_Message),
                UUID_aus(mitarbeiter.Familienstand),
                UUID_aus(mitarbeiter.Konfession),
                Liste_als_Message(mitarbeiter.Qualifikationen, Qualifikation_als_Message),
                mitarbeiter.Handzeichen,
                mitarbeiter.Personalnummer,
                UUID_aus(mitarbeiter.Geschlecht),
                Liste_als_Message(mitarbeiter.Kontakte, Kontakt_als_Message));

        private static Mitarbeiter Mitarbeiter_aus(MitarbeiterV1 mitarbeiter) =>
            new Mitarbeiter(
                Guid_aus(mitarbeiter.Id),
                Liste_aus(mitarbeiter.Mandantenzugehoerigkeiten, DM7_Mandantenzugehörigkeit_aus),
                mitarbeiter.Personalnummer,
                Guid_aus(mitarbeiter.Titel),
                mitarbeiter.Vorname,
                mitarbeiter.Nachname,
                Option_Map(mitarbeiter.Postanschrift, Postanschrift_aus),
                mitarbeiter.Handzeichen,
                Option_Map(mitarbeiter.Geburtstag, Datum_aus),
                Guid_aus(mitarbeiter.Geschlecht),
                Guid_aus(mitarbeiter.Konfession),
                Guid_aus(mitarbeiter.Familienstand),
                Liste_aus(mitarbeiter.Qualifikationen, Qualifikation_aus),
                Liste_aus(mitarbeiter.Kontakte, Kontakt_aus));

        private static KontaktV1 Kontakt_als_Message(Kontakt kontakt) => new KontaktV1(UUID_aus(kontakt.Kontaktart), UUID_aus(kontakt.Kontaktform), kontakt.Eintrag, kontakt.Hauptkontakt);
        private static Kontakt Kontakt_aus(KontaktV1 kontakt) => new Kontakt(Guid_aus(kontakt.Art), Guid_aus(kontakt.Form), kontakt.Eintrag, kontakt.Hauptkontakt);

        private static QualifikationV1 Qualifikation_als_Message(Qualifikation qualifikation) =>
            new QualifikationV1(
                (byte) qualifikation.Stufe,
                qualifikation.Bezeichnung,
                Datum_als_Message(qualifikation.Gültig_ab),
                Option_Map(qualifikation.Gültig_bis, Datum_als_Message));
        private static Qualifikation Qualifikation_aus(QualifikationV1 qualifikation) =>
            new Qualifikation(
                qualifikation.Stufe,
                qualifikation.Bezeichnung,
                Datum_aus(qualifikation.GueltigAb),
                Option_Map(qualifikation.GueltigBis, Datum_aus));

        private static PostanschriftV1 Postanschrift_als_Message(Postanschrift postanschrift) =>
            new PostanschriftV1(
                UUID_aus(postanschrift.Id),
                postanschrift.Strasse,
                postanschrift.Postleitzahl,
                postanschrift.Ort,
                postanschrift.Land);

        private static Postanschrift Postanschrift_aus(PostanschriftV1 postanschrift) =>
            new Postanschrift(
                Guid_aus(postanschrift.Id),
                postanschrift.Strasse,
                postanschrift.Postleitzahl,
                postanschrift.Ort,
                postanschrift.Land);

        private static DM7MandantenzugehoerigkeitV1 DM7_Mandantenzugehörigkeit_als_Message(DM7_Mandantenzugehörigkeit mandantenzugehörigkeit) =>
            new DM7MandantenzugehoerigkeitV1(
                UUID_aus(mandantenzugehörigkeit.MandantId),
                Datum_als_Message(mandantenzugehörigkeit.GueltigAb),
                Option_Map(mandantenzugehörigkeit.GueltigBis, Datum_als_Message));

        private static DM7_Mandantenzugehörigkeit DM7_Mandantenzugehörigkeit_aus(DM7MandantenzugehoerigkeitV1 mandantenzugehörigkeit) =>
            new DM7_Mandantenzugehörigkeit(
                Guid_aus(mandantenzugehörigkeit.MandantId),
                Datum_aus(mandantenzugehörigkeit.GueltigAb),
                Option_Map(mandantenzugehörigkeit.GueltigBis, Datum_aus));

        private static DienstV1 Dienst_als_Message(Dienst dienst) =>
            new DienstV1(
                (ulong) dienst.Id,
                Liste_als_Message(dienst.Mandantenzugehörigkeiten, DM7_Mandantenzugehörigkeit_als_Message),
                dienst.Kurzbezeichnung,
                dienst.Bezeichnung,
                Dienst_Gültigkeit_als_Message(dienst.Gültig_an),
                dienst.Gelöscht);

        private static Dienst Dienst_aus(DienstV1 dienst) =>
            new Dienst(
                (int) dienst.Id,
                Liste_aus(dienst.Mandantenzugehoerigkeiten, DM7_Mandantenzugehörigkeit_aus),
                dienst.Kurzbezeichnung,
                dienst.Bezeichnung,
                Dienst_Gültigkeit_aus(dienst.GueltigAn),
                dienst.Geloescht);

        private static DienstGueltigkeitV1 Dienst_Gültigkeit_als_Message(Dienst_Gültigkeit gültigkeit) =>
            new DienstGueltigkeitV1(
                gültigkeit.Montag,
                gültigkeit.Dienstag,
                gültigkeit.Mittwoch,
                gültigkeit.Donnerstag,
                gültigkeit.Freitag,
                gültigkeit.Samstag,
                gültigkeit.Sonntag,
                gültigkeit.Feiertags);

        private static Dienst_Gültigkeit Dienst_Gültigkeit_aus(DienstGueltigkeitV1 gültigkeit) =>
            new Dienst_Gültigkeit(
                gültigkeit.Montag,
                gültigkeit.Dienstag,
                gültigkeit.Mittwoch,
                gültigkeit.Donnerstag,
                gültigkeit.Freitag,
                gültigkeit.Samstag,
                gültigkeit.Sonntag,
                gültigkeit.Feiertags);

        private static Dienstbuchung Dienstbuchung_aus(DienstbuchungV1 dienstbuchung) =>
            new Dienstbuchung(
                Guid_aus(dienstbuchung.Mitarbeiter),
                (int)dienstbuchung.Dienst,
                Uhrzeit_aus(dienstbuchung.BeginntUm));

        private static DienstbuchungV1 Dienstbuchung_als_Message(Dienstbuchung dienstbuchung) =>
            new DienstbuchungV1(
                UUID_aus(dienstbuchung.MitarbeiterId),
                dienstbuchung.DienstId,
                Uhrzeit_als_Message(dienstbuchung.Beginnt_um));

        private static Abwesenheit Abwesenheit_aus(AbwesenheitV1 abwesenheit) =>
            new Abwesenheit(
                Guid_aus(abwesenheit.Mitarbeiter),
                Zeitpunkt_aus(abwesenheit.AbwesendAb),
                Option_Map(abwesenheit.VorraussichtlichWiederVerfuegbarAb, Zeitpunkt_aus),
                abwesenheit.Grund,
                Abwesenheitsart_aus(abwesenheit.Art));

        private static AbwesenheitV1 Abwesenheit_als_Message(Abwesenheit abwesenheit) =>
            new AbwesenheitV1(
                UUID_aus(abwesenheit.MitarbeiterId),
                Zeitpunkt_als_Message(abwesenheit.Abwesend_ab),
                Option_Map(abwesenheit.Vorraussichtlich_wieder_verfügbar_ab, Zeitpunkt_als_Message),
                abwesenheit.Grund,
                Abwesenheitsart_als_Message(abwesenheit.Art));

        private static Abwesenheitsart Abwesenheitsart_aus(AbwesenheitsartV1 art)
        {
            switch (art)
            {
                case AbwesenheitsartV1.FEHLZEIT:
                    return Abwesenheitsart.Fehlzeit;

                case AbwesenheitsartV1.ANDERWEITIG_VERPLANT:
                    return Abwesenheitsart.Anderweitig_verplant;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(art), art, null);
            }
        }

        private static AbwesenheitsartV1 Abwesenheitsart_als_Message(Abwesenheitsart art)
        {
            switch (art)
            {
                case Abwesenheitsart.Fehlzeit:
                    return AbwesenheitsartV1.FEHLZEIT;

                case Abwesenheitsart.Anderweitig_verplant:
                    return AbwesenheitsartV1.ANDERWEITIG_VERPLANT;

                default:
                    throw new ArgumentOutOfRangeException(nameof(art), art, null);
            }
        }

        private static UngeplanteTourV1 Ungeplante_Tour_als_Message(Ungeplante_Tour tour) =>
            new UngeplanteTourV1(
                UUID_aus(tour.MitarbeiterId),
                UUID_aus(tour.MandantId),
                Liste_als_Message(tour.Einsätze, Einsatz_als_Message));
        
        private static Ungeplante_Tour Ungeplante_Tour_aus(UngeplanteTourV1 tour) =>
            new Ungeplante_Tour(
                Guid_aus(tour.Mitarbeiter),
                Guid_aus(tour.Mandant),
                Liste_aus(tour.Einsaetze, Einsatz_aus));

        private static GeplanteTourV1 Geplante_Tour_als_Message(Geplante_Tour tour) =>
            new GeplanteTourV1(
                UUID_aus(tour.MitarbeiterId),
                UUID_aus(tour.MandantId),
                tour.Dienst,
                Liste_als_Message(tour.Einsätze, Einsatz_als_Message));

        private static Geplante_Tour Geplante_Tour_aus(GeplanteTourV1 tour) =>
            new Geplante_Tour(
                Guid_aus(tour.Mitarbeiter),
                Guid_aus(tour.Mandant),
                (int) tour.Dienst,
                Liste_aus(tour.Einsaetze, Einsatz_aus));

        private static NichtGefahreneTourV1 Nicht_gefahrene_Tour_als_Message(Nicht_gefahrene_Tour tour) =>
            new NichtGefahreneTourV1(
                UUID_aus(tour.MitarbeiterId),
                UUID_aus(tour.MandantId),
                tour.Dienst);

        private static Nicht_gefahrene_Tour Nicht_gefahrene_Tour_aus(NichtGefahreneTourV1 tour) =>
            new Nicht_gefahrene_Tour(
                Guid_aus(tour.Mitarbeiter),
                Guid_aus(tour.Mandant),
                (int) tour.Dienst);

        private static EinsatzV1 Einsatz_als_Message(Einsatz einsatz) =>
            new EinsatzV1(
                Relative_Zeit_als_Message(einsatz.Beginn),
                einsatz.Dauer_in_Minuten,
                einsatz.Anfahrtsdauer_in_Minuten,
                einsatz.Abfahrtsdauer_in_Minuten,
                Einsatzart_als_Message(einsatz.Art));

        private static Einsatz Einsatz_aus(EinsatzV1 einsatz) =>
            new Einsatz(
                Relative_Zeit_aus(einsatz.Beginn),
                (uint) einsatz.DauerInMinuten,
                (uint) einsatz.AnfahrtsdauerInMinuten,
                (uint) einsatz.AbfahrtsdauerInMinuten,
                Einsatzart_aus(einsatz.Art));

        private static EinsatzartV1 Einsatzart_als_Message(Einsatzart einsatzart)
        {
            switch (einsatzart)
            {
                case Klient_Einsatz _: return new KlientEinsatzV1();
                case Sonstige_Zeit sonstige: return new SonstigeZeitV1(UUID_aus(sonstige.Leistung));
                case Pause _: return new PauseV1();
                case Unterbrechung _: return new UnterbrechungV1();
                default: throw new ArgumentOutOfRangeException(nameof(einsatzart), einsatzart, null);
            }
        }

        private static Einsatzart Einsatzart_aus(EinsatzartV1 einsatzart)
        {
            switch (einsatzart)
            {
                case KlientEinsatzV1 _: return new Klient_Einsatz();
                case SonstigeZeitV1 sonstige: return new Sonstige_Zeit(Guid_aus(sonstige.Leistung));
                case PauseV1 _: return new Pause();
                case UnterbrechungV1 _: return new Unterbrechung();
                default: throw new ArgumentOutOfRangeException(nameof(einsatzart), einsatzart, null);
            }
        }

        private static Dienstplanabschluss Dienstplanabschluss_aus(DienstplanabschlussV1 abschluss) =>
            new Dienstplanabschluss(
                Guid_aus(abschluss.Mitarbeiter),
                Guid_aus(abschluss.Mandant),
                Datum_aus(abschluss.Datum));

        private static DienstplanabschlussV1 Dienstplanabschluss_als_Message(Dienstplanabschluss abschluss) =>
            new DienstplanabschlussV1(
                UUID_aus(abschluss.MitarbeiterId),
                UUID_aus(abschluss.MandantId),
                Datum_als_Message(abschluss.Datum));

        public static Guid Guid_aus(UUID uuid) => new Guid(uuid.Value);
        public static UUID UUID_aus(Guid guid) => new UUID(guid.ToByteArray());
        public static Datum Datum_aus(Messages.PPLUS.Datum datum) => Datum.DD_MM_YYYY(datum.Tag, datum.Monat, datum.Jahr);
        public static Messages.PPLUS.Datum Datum_als_Message(Datum datum) => new Messages.PPLUS.Datum((byte) datum.Tag, (byte) datum.Monat, (ushort) datum.Jahr);

        private static Messages.PPLUS.Zeitpunkt Zeitpunkt_als_Message(Zeitpunkt zeitpunkt) => new Messages.PPLUS.Zeitpunkt(Datum_als_Message(zeitpunkt.Datum), Uhrzeit_als_Message(zeitpunkt.Uhrzeit));
        private static Zeitpunkt Zeitpunkt_aus(Messages.PPLUS.Zeitpunkt zeitpunkt) => new Zeitpunkt(Datum_aus(zeitpunkt.Datum), Uhrzeit_aus(zeitpunkt.Uhrzeit));
        private static Messages.PPLUS.Uhrzeit Uhrzeit_als_Message(Uhrzeit uhrzeit) => new Messages.PPLUS.Uhrzeit((byte) uhrzeit.Stunden, (byte) uhrzeit.Minuten);
        private static Uhrzeit Uhrzeit_aus(Messages.PPLUS.Uhrzeit uhrzeit) => Uhrzeit.HH_MM(uhrzeit.Stunden, uhrzeit.Minuten);
        private static RelativeZeit Relative_Zeit_als_Message(Relative_Zeit zeit) => new RelativeZeit(Uhrzeit_als_Message(zeit.Zeit), zeit.Am_Folgetag);
        private static Relative_Zeit Relative_Zeit_aus(RelativeZeit zeit) => new Relative_Zeit(Uhrzeit_aus(zeit.Zeit), zeit.AmFolgetag);
        private static ReadOnlyCollection<TOut> Liste_aus<TIn, TOut>(IEnumerable<TIn> daten, Func<TIn, TOut> mapper) => new ReadOnlyCollection<TOut>(daten.Select(mapper).ToList());
        private static List<TOut> Liste_als_Message<TIn, TOut>(IEnumerable<TIn> daten, Func<TIn, TOut> mapper) => daten.Select(mapper).ToList();

        public static Token Token_aus(QueryMessage message) => new Token(message.Token);
        public static Token Token_aus(CommandMessage message) => new Token(message.Token);

        private static TOut? Option_Map<TIn, TOut>(TIn? option, Func<TIn, TOut> mapper) where TIn : struct where TOut : struct => option.HasValue ? mapper(option.Value) : (TOut?) null;
    }
}