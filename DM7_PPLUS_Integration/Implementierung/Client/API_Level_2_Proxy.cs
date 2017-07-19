using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    /// <summary>
    /// Implementiert die API level 2 und übersetzt die Anfragen in API-Level-unabhängige Nachrichten
    /// (Derzeit nur zum Test der Versionsverhandlung. Reuse für echte Anforderungen Level 2)
    /// </summary>
    public class API_Level_2_Proxy : DM7_PPLUS_API
    {
        private const int API_LEVEL = 1;

        private readonly Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung _ebene_2_Proxy;
        private readonly Guid _session;
        private readonly IDisposable _subscription;

        public API_Level_2_Proxy(Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung ebene2Proxy, Guid session, int auswahllistenversion)
        {
            _ebene_2_Proxy = ebene2Proxy;
            _session = session;
            Auswahllisten_Version = auswahllistenversion;
            var subject = new Subject<Stand>();
            _subscription = ebene2Proxy.Notifications.Subscribe(new Observer<Notification>(
                no =>
                {
                    var data = no as NotificationData;
                    if (data != null) { subject.Next(new VersionsStand(data.Session, data.Version)); }
                    else if (no is NotificationsClosed) { subject.Completed(); }
                    else subject.Error(new ConnectionErrorException($"Interner Fehler im Notificationstream. Unbekannte Nachricht: {no.GetType().Name}"));
                },
                ex => { throw new ConnectionErrorException($"Interner Fehler im Notificationstream: {ex.Message}", ex); } ));
            Stand_Mitarbeiterdaten = subject;
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }

        public int Auswahllisten_Version { get; }

        public IObservable<Stand> Stand_Mitarbeiterdaten { get; }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen(Stand von, Stand bis)
        {
            return
                _ebene_2_Proxy
                    .Query(API_LEVEL, _session, Datenquellen.Mitarbeiter, ((VersionsStand)von).Version, ((VersionsStand)bis).Version)
                    .ContinueWith(
                        task =>
                        {
                            var result = task.Result as QueryResult;
                            if (result != null)
                            {
                                return Deserialisiere_Mitarbeiterdatensaetze(result.Data);
                            }
                            else
                            {
                                var failed = task.Result as QueryFailed;
                                if (failed != null)
                                {
                                    if (failed.Reason == QueryFailure.Internal_Server_Error)
                                    {
                                        throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterdaten_abrufen' ist auf dem Server fehlgeschlagen: {failed.Info}.");
                                    }
                                    if (failed.Reason == QueryFailure.Unknown_reason)
                                    {
                                        throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterdaten_abrufen' ist fehlgeschlagen: {failed.Info}.");
                                    }
                                }
                            }

                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterdaten_abrufen' ist fehlgeschlagen: Unbekanntes Protokoll ({task.Result.GetType().Name}).");
                        }
                    );
        }

        private Mitarbeiterdatensaetze Deserialisiere_Mitarbeiterdatensaetze(byte[] resultData)
        {
            var guidbuffer = new byte[16];

            var position = 0;

            Array.Copy(resultData, guidbuffer, 16);
            var session = new Guid(guidbuffer);
            position += 16;
            var version = BitConverter.ToInt64(resultData, position);
            position += 8;
            var anzahl_Mitarbeiterdatensaetze = BitConverter.ToInt32(resultData, position);
            position += 4;

            var mitarbeiter = new List<Mitarbeiterdatensatz>();

            for (var i = 0; i < anzahl_Mitarbeiterdatensaetze; i++)
            {
                var laenge_nachname = BitConverter.ToInt32(resultData, position);
                position += 4;
                var laenge_vorname = BitConverter.ToInt32(resultData, position);
                position += 4;

                var nachname = System.Text.Encoding.UTF8.GetString(resultData, position, laenge_nachname);
                position += laenge_nachname;

                var vorname = System.Text.Encoding.UTF8.GetString(resultData, position, laenge_vorname);
                position += laenge_vorname;

                var ma =
                    new Mitarbeiterdatensatz(
                        Guid.NewGuid(),
                        Guid.Empty,
                        vorname,
                        nachname,
                        null,
                        null,
                        Guid.Empty,
                        Guid.Empty,
                        new Datum(1, 1, 2017),
                        null,
                        new ReadOnlyCollection<Qualifikation>(new List<Qualifikation>()),
                        "",
                        "1",
                        Guid.Empty,
                        new ReadOnlyCollection<Kontakt>(new List<Kontakt>()));

                mitarbeiter.Add(ma);
            }

            return
                new Mitarbeiterdatensaetze(
                    false,
                    new VersionsStand(
                        session,
                        version),
                    new ReadOnlyCollection<Mitarbeiterdatensatz>(mitarbeiter),
                    new ReadOnlyCollection<Mitarbeiterfoto>(new List<Mitarbeiterfoto>()));
        }

        public Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen()
        {
            var stand = VersionsStand.AbInitio(_session);
            return Mitarbeiterdaten_abrufen(stand, stand);
        }
    }
}