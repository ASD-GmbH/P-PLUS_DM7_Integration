using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bare.Msg;
using DM7_PPLUS_Integration.Daten;
using Datum = DM7_PPLUS_Integration.Daten.Datum;
using Uhrzeit = DM7_PPLUS_Integration.Daten.Uhrzeit;

namespace DM7_PPLUS_Integration.Implementierung.V2
{
    public static class Message_mapper
    {
        public static Mitarbeiterliste_V1 Mitarbeiterstammdaten_als_Message(Stammdaten<Mitarbeiter> mitarbeiter)
        {
            return new Mitarbeiterliste_V1(
                Liste_als_Message(mitarbeiter, Mitarbeiter_als_Message),
                Stand_als_Message(mitarbeiter.Stand));
        }

        public static Stammdaten<Mitarbeiter> Mitarbeiterlist_als_Stammdaten(Mitarbeiterliste_V1 mitarbeiter)
        {
            return new Stammdaten<Mitarbeiter>(
                Liste_aus(mitarbeiter.Mitarbeiter, Mitarbeiter_aus),
                Stand_aus(mitarbeiter.Stand));
        }

        public static Mitarbeiterfotos_V1 Mitarbeiterfotos_als_Message(Stammdaten<Mitarbeiterfoto> fotos)
        {
            return new Mitarbeiterfotos_V1(
                Liste_als_Message(fotos, Mitarbeiterfoto_als_Message),
                Stand_als_Message(fotos.Stand));
        }

        public static Stammdaten<Mitarbeiterfoto> Mitarbeiterfotos_als_Stammdaten(Mitarbeiterfotos_V1 fotos)
        {
            return new Stammdaten<Mitarbeiterfoto>(
                Liste_aus(fotos.Fotos, Mitarbeiterfoto_aus),
                Stand_aus(fotos.Stand));
        }

        public static Dienste_V1 Dienste_als_Message(Stammdaten<Dienst> dienste)
        {
            return new Dienste_V1(
                Liste_als_Message(dienste, Dienst_als_Message),
                Stand_als_Message(dienste.Stand));
        }

        public static Stammdaten<Dienst> Dienste_als_Stammdaten(Dienste_V1 dienste)
        {
            return new Stammdaten<Dienst>(
                Liste_aus(dienste.Dienste, Dienst_aus),
                Stand_aus(dienste.Stand));
        }

        public static Bare.Msg.Datenstand Stand_als_Message(Datenstand stand) => new Bare.Msg.Datenstand(stand.Value);
        public static Datenstand Stand_aus(Bare.Msg.Datenstand stand) => new Datenstand(stand.Value);

        private static Mitarbeiter_V1 Mitarbeiter_als_Message(Mitarbeiter mitarbeiter)
        {
            return new Mitarbeiter_V1(
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
        }

        private static Mitarbeiter Mitarbeiter_aus(Mitarbeiter_V1 mitarbeiter)
        {
            return new Mitarbeiter(
                Guid_aus(mitarbeiter.Id),
                Liste_aus(mitarbeiter.Mandantenzugehörigkeiten, DM7_Mandantenzugehörigkeit_aus),
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
        }

        private static Kontakt_V1 Kontakt_als_Message(Kontakt kontakt) => new Kontakt_V1(UUID_aus(kontakt.Kontaktart), UUID_aus(kontakt.Kontaktform), kontakt.Eintrag);
        private static Kontakt Kontakt_aus(Kontakt_V1 kontakt) => new Kontakt(Guid_aus(kontakt.Art), Guid_aus(kontakt.Form), kontakt.Eintrag);

        private static Qualifikation_V1 Qualifikation_als_Message(Qualifikation qualifikation) => new Qualifikation_V1((byte) qualifikation.Stufe, qualifikation.Bezeichnung);
        private static Qualifikation Qualifikation_aus(Qualifikation_V1 qualifikation) => new Qualifikation(qualifikation.Stufe, qualifikation.Bezeichnung);

        private static Postanschrift_V1 Postanschrift_als_Message(Postanschrift postanschrift)
        {
            return new Postanschrift_V1(
                UUID_aus(postanschrift.Id),
                postanschrift.Strasse,
                postanschrift.Postleitzahl,
                postanschrift.Ort,
                postanschrift.Land,
                postanschrift.Adresszusatz);
        }

        private static Postanschrift Postanschrift_aus(Postanschrift_V1 postanschrift)
        {
            return new Postanschrift(
                Guid_aus(postanschrift.Id),
                postanschrift.Strasse,
                postanschrift.Postleitzahl,
                postanschrift.Ort,
                postanschrift.Land,
                postanschrift.Adresszusatz);
        }

        private static DM7_Mandantenzugehörigkeit_V1 DM7_Mandantenzugehörigkeit_als_Message(DM7_Mandantenzugehörigkeit mandantenzugehörigkeit)
        {
            return new DM7_Mandantenzugehörigkeit_V1(
                UUID_aus(mandantenzugehörigkeit.MandantId),
                Datum_als_Message(mandantenzugehörigkeit.GueltigAb),
                Option_Map(mandantenzugehörigkeit.GueltigBis, Datum_als_Message));
        }

        private static DM7_Mandantenzugehörigkeit DM7_Mandantenzugehörigkeit_aus(DM7_Mandantenzugehörigkeit_V1 mandantenzugehörigkeit)
        {
            return new DM7_Mandantenzugehörigkeit(
                Guid_aus(mandantenzugehörigkeit.Mandantid),
                Datum_aus(mandantenzugehörigkeit.Gültigab),
                mandantenzugehörigkeit.Gültigbis.HasValue ? Datum_aus(mandantenzugehörigkeit.Gültigbis.Value) : (Datum?) null);
        }

        private static Mitarbeiterfoto_V1 Mitarbeiterfoto_als_Message(Mitarbeiterfoto foto)
        {
            return new Mitarbeiterfoto_V1(UUID_aus(foto.Mitarbeiter), foto.Foto);
        }

        private static Mitarbeiterfoto Mitarbeiterfoto_aus(Mitarbeiterfoto_V1 foto)
        {
            return new Mitarbeiterfoto(Guid_aus(foto.Mitarbeiter), Guid.Empty, foto.Foto);
        }

        private static Dienst_V1 Dienst_als_Message(Dienst dienst)
        {
            return new Dienst_V1(
                (ulong) dienst.Id,
                UUID_aus(dienst.Mandant),
                dienst.Kurzbezeichnung,
                dienst.Bezeichnung,
                Datum_als_Message(dienst.Gültig_ab),
                Option_Map(dienst.Gültig_bis, Datum_als_Message),
                Uhrzeit_als_Message(dienst.Beginn),
                Dienst_Gültigkeit_als_Message(dienst.Gültig_an),
                dienst.Gelöscht);
        }

        private static Dienst Dienst_aus(Dienst_V1 dienst)
        {
            return new Dienst(
                (int) dienst.Id,
                Guid_aus(dienst.Mandant),
                dienst.Kurzbezeichnung,
                dienst.Bezeichnung,
                Datum_aus(dienst.Gültigab),
                Option_Map(dienst.Gültibbis, Datum_aus),
                Uhrzeit_aus(dienst.Beginn),
                Dienst_Gültigkeit_aus(dienst.Gültigan),
                dienst.Gelöscht);
        }

        private static Dienst_Gültigkeit_V1 Dienst_Gültigkeit_als_Message(Dienst_Gültigkeit gültigkeit)
        {
            return new Dienst_Gültigkeit_V1(
                gültigkeit.Montag,
                gültigkeit.Dienstag,
                gültigkeit.Mittwoch,
                gültigkeit.Donnerstag,
                gültigkeit.Freitag,
                gültigkeit.Samstag,
                gültigkeit.Sonntag,
                gültigkeit.Feiertags);
        }
        private static Dienst_Gültigkeit Dienst_Gültigkeit_aus(Dienst_Gültigkeit_V1 gültigkeit)
        {
            return new Dienst_Gültigkeit(
                gültigkeit.Montag,
                gültigkeit.Dienstag,
                gültigkeit.Mittwoch,
                gültigkeit.Donnerstag,
                gültigkeit.Freitag,
                gültigkeit.Samstag,
                gültigkeit.Sonntag,
                gültigkeit.Feiertags);
        }

        private static Guid Guid_aus(UUID uuid) => new Guid(uuid.Value);
        private static UUID UUID_aus(Guid guid) => new UUID(guid.ToByteArray());
        private static Datum Datum_aus(Bare.Msg.Datum datum) => new Datum(datum.Tag, datum.Monat, datum.Jahr);
        private static Bare.Msg.Datum Datum_als_Message(Datum datum) => new Bare.Msg.Datum((byte) datum.Tag, (byte) datum.Monat, (ushort) datum.Jahr);

        private static Bare.Msg.Uhrzeit Uhrzeit_als_Message(Uhrzeit uhrzeit) => new Bare.Msg.Uhrzeit((byte) uhrzeit.Stunden, (byte) uhrzeit.Minuten);
        private static Uhrzeit Uhrzeit_aus(Bare.Msg.Uhrzeit uhrzeit) => Uhrzeit.HHMM(uhrzeit.Stunden, uhrzeit.Minuten);
        private static ReadOnlyCollection<TOut> Liste_aus<TIn, TOut>(IEnumerable<TIn> daten, Func<TIn, TOut> mapper) => new ReadOnlyCollection<TOut>(daten.Select(mapper).ToList());
        private static TOut[] Liste_als_Message<TIn, TOut>(IEnumerable<TIn> daten, Func<TIn, TOut> mapper) => daten.Select(mapper).ToArray();

        private static TOut? Option_Map<TIn, TOut>(TIn? option, Func<TIn, TOut> mapper) where TIn : struct where TOut : struct => option.HasValue ? mapper(option.Value) : (TOut?) null;
    }
}