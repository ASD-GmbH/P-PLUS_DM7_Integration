using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    /// <summary>
    /// Implementiert die API version 3 und übersetzt die Anfragen in API-Version-unabhängige Nachrichten
    /// </summary>
    internal class API_Version_3_Proxy : DisposeGroupMember, DM7_PPLUS_API
    {
        private const int API_VERSION = 3;

        private readonly string _credentials;
        private readonly Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung _schicht_2_Proxy;
        private readonly Log _log;
        private Guid _session;

        /// <summary>
        /// Implementiert die API version 2 und übersetzt die Anfragen in API-Version-unabhängige Nachrichten
        /// </summary>
        public API_Version_3_Proxy(string credentials, Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung schicht2Proxy, int auswahllistenversion, Log log, DisposeGroup disposegroup) : base(disposegroup)
        {
            _credentials = credentials;
            _schicht_2_Proxy = schicht2Proxy;
            disposegroup.With(() =>
            {
                _log.Info("DM7/P-PLUS Schnittstelle wird beendent...");
            });

            _log = log;
            Auswahllisten_Version = auswahllistenversion;

            var standMitarbeiterdaten = new Subject<Stand>();
            var subscription = schicht2Proxy.Notifications.Subscribe(new Observer<Notification>(
                no =>
                {
                    if (no is NotificationData data)
                    {
                        if (data.Session != _session)
                        {
                            _log.Info("P-PLUS Server wurde neu verbunden.");
                            _session = data.Session;
                            standMitarbeiterdaten.Next(new VersionsStand(data.Session, data.Version));
                        }
                        else
                        {
                            if (data.Datenquelle == Datenquellen.Mitarbeiter)
                            {
                                _log.Debug($"Mitarbeiterdaten aktualisiert (@{data.Version}))");
                                standMitarbeiterdaten.Next(new VersionsStand(data.Session, data.Version));
                            }
                        }
                    }
                    else if (no is NotificationsClosed) { standMitarbeiterdaten.Completed(); }
                    else standMitarbeiterdaten.Error(new ConnectionErrorException($"Interner Fehler im Notificationstream. Unbekannte Nachricht: {no.GetType().Name}"));
                },
                ex => throw new ConnectionErrorException($"Interner Fehler im Notificationstream: {ex.Message}", ex)));
            Stand_Mitarbeiterdaten = standMitarbeiterdaten;
            disposegroup.With(() =>
            {
                log.Debug("Subscription wird geschlossen...");
                subscription.Dispose();
            });
        }

        public int Auswahllisten_Version { get; }
        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen()
        {
            throw new NotImplementedException();
        }

        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen_ab(Datenstand stand)
        {
            throw new NotImplementedException();
        }

        public Task<Stammdaten<Mitarbeiterfoto>> Mitarbeiterfotos_abrufen()
        {
            throw new NotImplementedException();
        }

        public Task<Stammdaten<Mitarbeiterfoto>> Mitarbeiterfotos_abrufen_ab(Datenstand stand)
        {
            throw new NotImplementedException();
        }

        public Task<Stammdaten<Dienst>> Dienste_abrufen_ab(Datenstand stand)
        {
            throw new NotImplementedException();
        }

        Task<Stammdaten<Dienst>> DM7_PPLUS_API.Dienste_abrufen()
        {
            throw new NotImplementedException();
        }

        public IObservable<Stand> Stand_Mitarbeiterdaten { get; }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen(Stand von, Stand bis)
        {
            _log.Debug($"Mitarbeiterdaten werden abgerufen ({von}..{bis})...");
            var vvon = ((VersionsStand)von);
            var vbis = ((VersionsStand)bis);

            return
                _schicht_2_Proxy
                    .Query_Mitarbeiterdatensätze(_credentials, API_VERSION, vvon.Session, Datenquellen.Mitarbeiter, vvon.Version, vbis.Version)
                    .ContinueWith(
                        task =>
                        {
                            if (task.Result is QueryResult result)
                            {
                                var mitarbeiterdatensaetze = Deserialize.Deserialisiere_Mitarbeiterdatensaetze(result.Data);
                                _log.Debug($"Mitarbeiterdaten empfangen ({mitarbeiterdatensaetze.Mitarbeiter.Count} Mitarbeiter, {mitarbeiterdatensaetze.Fotos.Count} Bilder, @{mitarbeiterdatensaetze.Stand}, {(mitarbeiterdatensaetze.Teilmenge?"teildaten":"vollständig")})");
                                return mitarbeiterdatensaetze;
                            }
                            if (task.Result is QueryFailed failed)
                            {
                                if (failed.Reason == QueryFailure.Internal_Server_Error)
                                {
                                    throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterdaten_abrufen' ist auf dem Server fehlgeschlagen: {failed.Info}.");
                                }
                                if (failed.Reason == QueryFailure.Unauthorized)
                                {
                                    throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterdaten_abrufen' ist fehlgeschlagen: {failed.Info}.");
                                }
                                if (failed.Reason == QueryFailure.Unknown_reason)
                                {
                                    throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterdaten_abrufen' ist fehlgeschlagen: {failed.Info}.");
                                }
                            }

                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterdaten_abrufen' ist fehlgeschlagen: Unbekanntes Protokoll ({task.Result.GetType().Name}).");
                        }
                    );
        }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen()
        {
            var stand = VersionsStand.AbInitio();
            return Mitarbeiterdaten_abrufen(stand, stand);
        }
    }

    internal static class Deserialize
    {
        internal static Mitarbeiterdatensaetze Deserialisiere_Mitarbeiterdatensaetze(byte[] data)
        {
            var position = 0;

            var session = Deserialize_Guid(data, ref position);
            var version = Deserialize_Longint(data, ref position);
            var teildaten = Deserialize_Bool(data, ref position);

            var mitarbeiter = new List<Mitarbeiter>();

            return
                new Mitarbeiterdatensaetze(
                    teildaten,
                    new VersionsStand(
                        session,
                        version),
                    new ReadOnlyCollection<Mitarbeiter>(mitarbeiter),
                    new ReadOnlyCollection<Mitarbeiterfoto>(new List<Mitarbeiterfoto>()));
        }

        internal static Stammdaten<Dienst> Deserialisiere_Dienste(byte[] data)
        {
            var position = 0;
            var anzahl_datensätze = Deserialize_Int(data, ref position);
            return new Stammdaten<Dienst>(
                Enumerable.Range(0, anzahl_datensätze)
                    .Select(_ => new Dienst(
                        Deserialize_Int(data, ref position),
                        Deserialize_Guid(data, ref position),
                        Deserialize_String(data, ref position),
                        Deserialize_String(data, ref position),
                        Deserialize_Datum(data, ref position),
                        Deserialize_Nullable_Datum(data, ref position),
                        Deserialize_Uhrzeit(data, ref position),
                        Deserialize_Dienst_Gültigkeit(data, ref position),
                        Deserialize_Bool(data, ref position)
                    ))
                    .ToList(),
                new Datenstand(Deserialize_ULong(data, ref position))
            );
        }

        private static ulong Deserialize_ULong(byte[] data, ref int position)
        {
            var value = BitConverter.ToUInt64(data, position);
            position += 8;
            return value;
        }

        private static ReadOnlyCollection<Kontakt> Deserialize_Kontakte(byte[] data, ref int position)
        {
            var anzahl = Deserialize_Int(data, ref position);

            var result = new List<Kontakt>();

            for (int i = 0; i < anzahl; i++)
            {
                var art = Deserialize_Guid(data, ref position);
                var form = Deserialize_Guid(data, ref position);
                var eintrag = Deserialize_String(data, ref position);
                result.Add(new Kontakt(art, form, eintrag));
            }

            return new ReadOnlyCollection<Kontakt>(result);
        }

        private static Dienst_Gültigkeit Deserialize_Dienst_Gültigkeit(byte[] data, ref int position)
        {
            return new Dienst_Gültigkeit(
                Deserialize_Bool(data, ref position),
                Deserialize_Bool(data, ref position),
                Deserialize_Bool(data, ref position),
                Deserialize_Bool(data, ref position),
                Deserialize_Bool(data, ref position),
                Deserialize_Bool(data, ref position),
                Deserialize_Bool(data, ref position),
                Deserialize_Bool(data, ref position)
            );
        }

        private static ReadOnlyCollection<Qualifikation> Deserialize_Qualifikationen(byte[] data, ref int position)
        {
            throw new NotImplementedException();
        }

        private static Datum? Deserialize_Nullable_Datum(byte[] data, ref int position)
        {
            var laenge = BitConverter.ToInt32(data, position);
            position += 4;
            switch (laenge)
            {
                case 8:
                    var d = System.Text.Encoding.UTF8.GetString(data, position, 8);
                    position += 8;
                    return new Datum(int.Parse(d.Substring(6, 2)), int.Parse(d.Substring(4, 2)), int.Parse(d.Substring(0, 4)));
                case 0:
                    return null;
                default:
                    throw new ConnectionErrorException($"Fehler beim Deserialisieren eines Mitarbeiters. Datumsformat defekt. Datagram: {data}");
            }
        }

        private static Datum Deserialize_Datum(byte[] data, ref int position)
        {
            var laenge = BitConverter.ToInt32(data, position);
            position += 4;
            switch (laenge)
            {
                case 8:
                    var d = System.Text.Encoding.UTF8.GetString(data, position, 8);
                    position += 8;
                    return new Datum(int.Parse(d.Substring(6, 2)), int.Parse(d.Substring(4,2)), int.Parse(d.Substring(0, 4)));
                default:
                    throw new ConnectionErrorException($"Fehler beim Deserialisieren eines Mitarbeiters. Datumsformat defekt. Datagram: {data}");
            }
        }

        private static Uhrzeit Deserialize_Uhrzeit(byte[] data, ref int position)
        {
            return Uhrzeit.HHMM(Deserialize_Int(data, ref position), Deserialize_Int(data, ref position));
        }

        private static Postanschrift? Deserialize_Postanschrift(byte[] data, ref int position)
        {
            var laenge = BitConverter.ToInt32(data, position);
            position += 4;
            if (laenge != 0)
            {
                position += laenge;
                using (var stream = new MemoryStream(data, position-laenge, laenge, false))
                {
                    var id = stream.ReadGuid();
                    var adresszusatz = stream.ReadStringWithPrefixedLength();
                    var strasse = stream.ReadStringWithPrefixedLength();
                    var plz = stream.ReadStringWithPrefixedLength();
                    var ort = stream.ReadStringWithPrefixedLength();
                    var land = stream.ReadStringWithPrefixedLength();
                    return new Postanschrift(id, strasse, plz, ort, land, adresszusatz);
                }
            }
            else
            {
                return null;
            }

        }

        private static string Deserialize_String(byte[] data, ref int position)
        {
            var laenge = BitConverter.ToInt32(data, position);
            position += 4;
            var result = System.Text.Encoding.UTF8.GetString(data, position, laenge);
            position += laenge;
            return result;
        }

        private static bool Deserialize_Bool(byte[] data, ref int position)
        {
            var result = data[position] == 1;
            position += 1;
            return result;
        }

        private static long Deserialize_Longint(byte[] data, ref int position)
        {
            var result = BitConverter.ToInt64(data, position);
            position += 8;
            return result;
        }

        private static int Deserialize_Int(byte[] data, ref int position)
        {
            var result = BitConverter.ToInt32(data, position);
            position += 4;
            return result;
        }

        private static Guid Deserialize_Guid(byte[] data, ref int position)
        {
            var guidbuffer = new byte[16];
            Array.Copy(data.Skip(position).ToArray(), guidbuffer, 16);
            position += 16;
            return new Guid(guidbuffer);
        }

        public static Stammdaten<Mitarbeiterfoto> Deserialisiere_Mitarbeiterfotos(byte[] data)
        {
            var position = 0;
            var anzahl_datensätze = Deserialize_Int(data, ref position);
            return new Stammdaten<Mitarbeiterfoto>(
                Enumerable.Range(0, anzahl_datensätze)
                    .Select(_ => new Mitarbeiterfoto(
                        Deserialize_Guid(data, ref position),
                        Deserialize_Guid(data, ref position),
                        Deserialize_Data(data, ref position)
                    ))
                    .ToList(),
                new Datenstand(Deserialize_ULong(data, ref position))
            );
        }

        private static byte[] Deserialize_Data(byte[] data, ref int position)
        {
            var länge = Deserialize_Int(data, ref position);
            var result = data.Skip(position).Take(länge).ToArray();
            position += länge;
            return result;
        }

        private static ReadOnlyCollection<DM7_Mandantenzugehörigkeiten> Deserialize_Mandantenzugehörigkeit(byte[] data, ref int position)
        {
            var anzahl_datensätze = Deserialize_Int(data, ref position);
            var tmpPosition = position;
            var res = new ReadOnlyCollection<DM7_Mandantenzugehörigkeiten>(
                Enumerable.Range(0, anzahl_datensätze)
                    .Select(_ => new DM7_Mandantenzugehörigkeiten(
                        Deserialize_Int(data, ref tmpPosition),
                        Deserialize_Datum(data, ref tmpPosition),
                        Deserialize_Nullable_Datum(data, ref tmpPosition)
                    ))
                    .ToList()
            );
            position = tmpPosition;
            return res;
        }

        public static Stammdaten<Mitarbeiter> Deserialisiere_Mitarbeiter(byte[] data)
        {
            var position = 0;
            var anzahl_datensätze = Deserialize_Int(data, ref position);
            return new Stammdaten<Mitarbeiter>(
                Enumerable.Range(0, anzahl_datensätze)
                    .Select(_ => new Mitarbeiter(
                        Deserialize_Guid(data, ref position),
                        Deserialize_Mandantenzugehörigkeit(data, ref position),
                        Deserialize_String(data, ref position),
                        Deserialize_Guid(data, ref position),
                        Deserialize_String(data, ref position),
                        Deserialize_String(data, ref position),
                        Deserialize_Postanschrift(data, ref position),
                        Deserialize_String(data, ref position),
                        Deserialize_Nullable_Datum(data, ref position),
                        Deserialize_Guid(data, ref position),
                        Deserialize_Guid(data, ref position),
                        Deserialize_Guid(data, ref position),
                        Deserialize_Qualifikationen(data, ref position),
                        Deserialize_Kontakte(data, ref position)
                    ))
                    .ToList(),
                new Datenstand(Deserialize_ULong(data, ref position))
            );
        }
    }

}