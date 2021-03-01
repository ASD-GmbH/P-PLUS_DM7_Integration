using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bare.Msg;
using DM7_PPLUS_Integration.Daten;
using Datum = DM7_PPLUS_Integration.Daten.Datum;
using Uhrzeit = DM7_PPLUS_Integration.Daten.Uhrzeit;
using Zeitpunkt = DM7_PPLUS_Integration.Daten.Zeitpunkt;

namespace DM7_PPLUS_Integration.Implementierung
{
    public static class Message_mapper
    {
        public static Mitarbeiterliste_V1 Mitarbeiterstammdaten_als_Message(Stammdaten<Mitarbeiter> mitarbeiter) =>
            new Mitarbeiterliste_V1(
                Liste_als_Message(mitarbeiter, Mitarbeiter_als_Message),
                Stand_als_Message(mitarbeiter.Stand));

        public static Stammdaten<Mitarbeiter> Mitarbeiterlist_als_Stammdaten(Mitarbeiterliste_V1 mitarbeiter) =>
            new Stammdaten<Mitarbeiter>(
                Liste_aus(mitarbeiter.Mitarbeiter, Mitarbeiter_aus),
                Stand_aus(mitarbeiter.Stand));

        public static Dienste_V1 Dienste_als_Message(Stammdaten<Dienst> dienste) =>
            new Dienste_V1(
                Liste_als_Message(dienste, Dienst_als_Message),
                Stand_als_Message(dienste.Stand));

        public static Stammdaten<Dienst> Dienste_als_Stammdaten(Dienste_V1 dienste) =>
            new Stammdaten<Dienst>(
                Liste_aus(dienste.Dienste, Dienst_aus),
                Stand_aus(dienste.Stand));

        public static Bare.Msg.Datenstand Stand_als_Message(Datenstand stand) => new Bare.Msg.Datenstand(stand.Value);
        public static Datenstand Stand_aus(Bare.Msg.Datenstand stand) => new Datenstand(stand.Value);

        public static ReadOnlyCollection<Dienstbuchung> Dienstbuchungen(Dienstbuchungen_V1 dienstbuchungen) => Liste_aus(dienstbuchungen.Value, Dienstbuchung_aus);
        public static Dienstbuchungen_V1 Dienstbuchungen_als_Message(ReadOnlyCollection<Dienstbuchung> dienstbuchungen) => new Dienstbuchungen_V1(Liste_als_Message(dienstbuchungen, Dienstbuchung_als_Message));

        public static ReadOnlyCollection<Abwesenheit> Abwesenheiten(Abwesenheiten_V1 abwesenheiten) => Liste_aus(abwesenheiten.Value, Abwesenheit_aus);
        public static Abwesenheiten_V1 Abwesenheiten_als_Message(ReadOnlyCollection<Abwesenheit> abwesenheiten) => new Abwesenheiten_V1(Liste_als_Message(abwesenheiten, Abwesenheit_als_Message));

        private static Mitarbeiter_V1 Mitarbeiter_als_Message(Mitarbeiter mitarbeiter) =>
            new Mitarbeiter_V1(
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

        private static Mitarbeiter Mitarbeiter_aus(Mitarbeiter_V1 mitarbeiter) =>
            new Mitarbeiter(
                Guid_aus(mitarbeiter.Id),
                Liste_aus(mitarbeiter.Mandantenzugehoerigkeiten, DM7_Mandantenzugehörigkeit_aus),
                mitarbeiter.Personalnummer,
                Guid_aus(mitarbeiter.Titel),
                mitarbeiter.Vorname,
                mitarbeiter.Nachname,
                mitarbeiter.Postanschrift.HasValue ? Postanschrift_aus(mitarbeiter.Postanschrift.Value) : (Postanschrift?) null,
                mitarbeiter.Handzeichen,
                mitarbeiter.Geburtstag.HasValue ? Datum_aus(mitarbeiter.Geburtstag.Value) : (Datum?) null,
                Guid_aus(mitarbeiter.Geschlecht),
                Guid_aus(mitarbeiter.Konfession),
                Guid_aus(mitarbeiter.Familienstand),
                Liste_aus(mitarbeiter.Qualifikationen, Qualifikation_aus),
                Liste_aus(mitarbeiter.Kontakte, Kontakt_aus));

        private static Kontakt_V1 Kontakt_als_Message(Kontakt kontakt) => new Kontakt_V1(UUID_aus(kontakt.Kontaktart), UUID_aus(kontakt.Kontaktform), kontakt.Eintrag);
        private static Kontakt Kontakt_aus(Kontakt_V1 kontakt) => new Kontakt(Guid_aus(kontakt.Art), Guid_aus(kontakt.Form), kontakt.Eintrag);

        private static Qualifikation_V1 Qualifikation_als_Message(Qualifikation qualifikation) =>
            new Qualifikation_V1(
                (byte) qualifikation.Stufe,
                qualifikation.Bezeichnung,
                Datum_als_Message(qualifikation.Gültig_ab),
                Option_Map(qualifikation.Gültig_bis, Datum_als_Message));
        private static Qualifikation Qualifikation_aus(Qualifikation_V1 qualifikation) =>
            new Qualifikation(
                qualifikation.Stufe,
                qualifikation.Bezeichnung,
                Datum_aus(qualifikation.Gueltigab),
                Option_Map(qualifikation.Gueltigbis, Datum_aus));

        private static Postanschrift_V1 Postanschrift_als_Message(Postanschrift postanschrift) =>
            new Postanschrift_V1(
                UUID_aus(postanschrift.Id),
                postanschrift.Strasse,
                postanschrift.Postleitzahl,
                postanschrift.Ort,
                postanschrift.Land);

        private static Postanschrift Postanschrift_aus(Postanschrift_V1 postanschrift) =>
            new Postanschrift(
                Guid_aus(postanschrift.Id),
                postanschrift.Strasse,
                postanschrift.Postleitzahl,
                postanschrift.Ort,
                postanschrift.Land);

        private static DM7_Mandantenzugehoerigkeit_V1 DM7_Mandantenzugehörigkeit_als_Message(DM7_Mandantenzugehörigkeit mandantenzugehörigkeit) =>
            new DM7_Mandantenzugehoerigkeit_V1(
                UUID_aus(mandantenzugehörigkeit.MandantId),
                Datum_als_Message(mandantenzugehörigkeit.GueltigAb),
                Option_Map(mandantenzugehörigkeit.GueltigBis, Datum_als_Message));

        private static DM7_Mandantenzugehörigkeit DM7_Mandantenzugehörigkeit_aus(DM7_Mandantenzugehoerigkeit_V1 mandantenzugehörigkeit) =>
            new DM7_Mandantenzugehörigkeit(
                Guid_aus(mandantenzugehörigkeit.Mandantid),
                Datum_aus(mandantenzugehörigkeit.Gueltigab),
                mandantenzugehörigkeit.Gueltigbis.HasValue ? Datum_aus(mandantenzugehörigkeit.Gueltigbis.Value) : (Datum?) null);

        private static Dienst_V1 Dienst_als_Message(Dienst dienst) =>
            new Dienst_V1(
                (ulong) dienst.Id,
                Liste_als_Message(dienst.Mandantenzugehörigkeiten, DM7_Mandantenzugehörigkeit_als_Message),
                dienst.Kurzbezeichnung,
                dienst.Bezeichnung,
                Dienst_Gültigkeit_als_Message(dienst.Gültig_an),
                dienst.Gelöscht);

        private static Dienst Dienst_aus(Dienst_V1 dienst) =>
            new Dienst(
                (int) dienst.Id,
                Liste_aus(dienst.Mandantenzugehoerigkeiten, DM7_Mandantenzugehörigkeit_aus),
                dienst.Kurzbezeichnung,
                dienst.Bezeichnung,
                Dienst_Gültigkeit_aus(dienst.Gueltigan),
                dienst.Geloescht);

        private static Dienst_Gueltigkeit_V1 Dienst_Gültigkeit_als_Message(Dienst_Gültigkeit gültigkeit) =>
            new Dienst_Gueltigkeit_V1(
                gültigkeit.Montag,
                gültigkeit.Dienstag,
                gültigkeit.Mittwoch,
                gültigkeit.Donnerstag,
                gültigkeit.Freitag,
                gültigkeit.Samstag,
                gültigkeit.Sonntag,
                gültigkeit.Feiertags);

        private static Dienst_Gültigkeit Dienst_Gültigkeit_aus(Dienst_Gueltigkeit_V1 gültigkeit) =>
            new Dienst_Gültigkeit(
                gültigkeit.Montag,
                gültigkeit.Dienstag,
                gültigkeit.Mittwoch,
                gültigkeit.Donnerstag,
                gültigkeit.Freitag,
                gültigkeit.Samstag,
                gültigkeit.Sonntag,
                gültigkeit.Feiertags);

        private static Dienstbuchung Dienstbuchung_aus(Dienstbuchung_V1 dienstbuchung) =>
            new Dienstbuchung(
                Guid_aus(dienstbuchung.Mitarbeiter),
                (int)dienstbuchung.Dienst,
                Uhrzeit_aus(dienstbuchung.Beginntum));

        private static Dienstbuchung_V1 Dienstbuchung_als_Message(Dienstbuchung dienstbuchung) =>
            new Dienstbuchung_V1(
                UUID_aus(dienstbuchung.MitarbeiterId),
                dienstbuchung.DienstId,
                Uhrzeit_als_Message(dienstbuchung.Beginnt_um));

        private static Abwesenheit Abwesenheit_aus(Abwesenheit_V1 abwesenheit) =>
            new Abwesenheit(
                Guid_aus(abwesenheit.Mitarbeiter),
                Zeitpunkt_aus(abwesenheit.Abwesendab),
                Zeitpunkt_aus(abwesenheit.Vorraussichtlichwiederverfuegbarab),
                abwesenheit.Grund,
                Abwesenheitsart_aus(abwesenheit.Art));

        private static Abwesenheit_V1 Abwesenheit_als_Message(Abwesenheit abwesenheit) =>
            new Abwesenheit_V1(
                UUID_aus(abwesenheit.MitarbeiterId),
                Zeitpunkt_als_Message(abwesenheit.Abwesend_ab),
                Zeitpunkt_als_Message(abwesenheit.Vorraussichtlich_wieder_verfügbar_ab),
                abwesenheit.Grund,
                Abwesenheitsart_als_Message(abwesenheit.Art));

        private static Abwesenheitsart Abwesenheitsart_aus(Abwesenheitsart_V1 art)
        {
            switch (art)
            {
                case Abwesenheitsart_V1.FEHLZEIT:
                    return Abwesenheitsart.Fehlzeit;

                case Abwesenheitsart_V1.ANDERSWEITIG_VERPLANT:
                    return Abwesenheitsart.Andersweitig_verplant;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(art), art, null);
            }
        }

        private static Abwesenheitsart_V1 Abwesenheitsart_als_Message(Abwesenheitsart art)
        {
            switch (art)
            {
                case Abwesenheitsart.Fehlzeit:
                    return Abwesenheitsart_V1.FEHLZEIT;

                case Abwesenheitsart.Andersweitig_verplant:
                    return Abwesenheitsart_V1.ANDERSWEITIG_VERPLANT;

                default:
                    throw new ArgumentOutOfRangeException(nameof(art), art, null);
            }
        }

        public static Guid Guid_aus(UUID uuid) => new Guid(uuid.Value);
        public static UUID UUID_aus(Guid guid) => new UUID(guid.ToByteArray());
        public static Datum Datum_aus(Bare.Msg.Datum datum) => new Datum(datum.Tag, datum.Monat, datum.Jahr);
        public static Bare.Msg.Datum Datum_als_Message(Datum datum) => new Bare.Msg.Datum((byte) datum.Tag, (byte) datum.Monat, (ushort) datum.Jahr);

        private static Bare.Msg.Zeitpunkt Zeitpunkt_als_Message(Zeitpunkt zeitpunkt) => new Bare.Msg.Zeitpunkt(Datum_als_Message(zeitpunkt.Datum), Uhrzeit_als_Message(zeitpunkt.Uhrzeit));
        private static Zeitpunkt Zeitpunkt_aus(Bare.Msg.Zeitpunkt zeitpunkt) => new Zeitpunkt(Datum_aus(zeitpunkt.Datum), Uhrzeit_aus(zeitpunkt.Uhrzeit));
        private static Bare.Msg.Uhrzeit Uhrzeit_als_Message(Uhrzeit uhrzeit) => new Bare.Msg.Uhrzeit((byte) uhrzeit.Stunden, (byte) uhrzeit.Minuten);
        private static Uhrzeit Uhrzeit_aus(Bare.Msg.Uhrzeit uhrzeit) => Uhrzeit.HH_MM(uhrzeit.Stunden, uhrzeit.Minuten);
        private static ReadOnlyCollection<TOut> Liste_aus<TIn, TOut>(IEnumerable<TIn> daten, Func<TIn, TOut> mapper) => new ReadOnlyCollection<TOut>(daten.Select(mapper).ToList());
        private static TOut[] Liste_als_Message<TIn, TOut>(IEnumerable<TIn> daten, Func<TIn, TOut> mapper) => daten.Select(mapper).ToArray();

        public static Token Token_aus(Query_Message message) => new Token(message.Token);

        private static TOut? Option_Map<TIn, TOut>(TIn? option, Func<TIn, TOut> mapper) where TIn : struct where TOut : struct => option.HasValue ? mapper(option.Value) : (TOut?) null;
    }
}