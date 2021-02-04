using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Client
{
    /// <summary>
    /// Implementiert die API version 4 und übersetzt die Anfragen in API-Version-unabhängige Nachrichten
    /// </summary>
    internal class API_Version_4_Proxy : DisposeGroupMember, DM7_PPLUS_API
    {
        private const int API_VERSION = 4;

        private readonly string _credentials;
        private readonly Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung _schicht_2_Proxy;
        private readonly Log _log;
        private Guid _session;

        /// <summary>
        /// Implementiert die API version 2 und übersetzt die Anfragen in API-Version-unabhängige Nachrichten
        /// </summary>
        public API_Version_4_Proxy(string credentials, Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung schicht2Proxy, int auswahllistenversion, Log log, DisposeGroup disposegroup) : base(disposegroup)
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
            _log.Debug("Mitarbeiter werden abgerufen ...");
            return _schicht_2_Proxy
                .Query_Mitarbeiter(_credentials, API_VERSION, _session, null)
                .ContinueWith(task =>
                {
                    if (task.Result is QueryResult result)
                    {
                        var mitarbeiter = Deserialize.Deserialisiere_Mitarbeiter(result.Data);
                        _log.Debug($"{mitarbeiter.Count} Mitarbeiter empfangen");
                        return mitarbeiter;
                    }
                    if (task.Result is QueryFailed failed)
                    {
                        if (failed.Reason == QueryFailure.Internal_Server_Error)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiter_abrufen' ist auf dem Server fehlgeschlagen: {failed.Info}.");
                        }
                        if (failed.Reason == QueryFailure.Unauthorized)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiter_abrufen' ist fehlgeschlagen: {failed.Info}.");
                        }
                        if (failed.Reason == QueryFailure.Unknown_reason)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiter_abrufen' ist fehlgeschlagen: {failed.Info}.");
                        }
                    }

                    throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiter_abrufen' ist fehlgeschlagen: Unbekanntes Protokoll ({task.Result.GetType().Name}).");
                });
        }

        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen_ab(Datenstand stand)
        {
            _log.Debug("Mitarbeiter für Datenstand werden abgerufen ...");
            return _schicht_2_Proxy
                .Query_Mitarbeiter(_credentials, API_VERSION, _session, stand)
                .ContinueWith(task =>
                {
                    if (task.Result is QueryResult result)
                    {
                        var mitarbeiter = Deserialize.Deserialisiere_Mitarbeiter(result.Data);
                        _log.Debug($"{mitarbeiter.Count} Mitarbeiter empfangen");
                        return mitarbeiter;
                    }
                    if (task.Result is QueryFailed failed)
                    {
                        if (failed.Reason == QueryFailure.Internal_Server_Error)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiter_abrufen_ab' ist auf dem Server fehlgeschlagen: {failed.Info}.");
                        }
                        if (failed.Reason == QueryFailure.Unauthorized)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiter_abrufen_ab' ist fehlgeschlagen: {failed.Info}.");
                        }
                        if (failed.Reason == QueryFailure.Unknown_reason)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiter_abrufen_ab' ist fehlgeschlagen: {failed.Info}.");
                        }
                    }

                    throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiter_abrufen_ab' ist fehlgeschlagen: Unbekanntes Protokoll ({task.Result.GetType().Name}).");
                });
        }

        public Task<Stammdaten<Mitarbeiterfoto>> Mitarbeiterfotos_abrufen()
        {
            _log.Debug("Mitarbeiterfotos werden abgerufen ...");
            return _schicht_2_Proxy
                .Query_Mitarbeiterfotos(_credentials, API_VERSION, _session, null)
                .ContinueWith(task =>
                {
                    if (task.Result is QueryResult result)
                    {
                        var fotos = Deserialize.Deserialisiere_Mitarbeiterfotos(result.Data);
                        _log.Debug($"{fotos.Count} Mitarbeiterfotos empfangen");
                        return fotos;
                    }
                    if (task.Result is QueryFailed failed)
                    {
                        if (failed.Reason == QueryFailure.Internal_Server_Error)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterfotos_abrufen' ist auf dem Server fehlgeschlagen: {failed.Info}.");
                        }
                        if (failed.Reason == QueryFailure.Unauthorized)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterfotos_abrufen' ist fehlgeschlagen: {failed.Info}.");
                        }
                        if (failed.Reason == QueryFailure.Unknown_reason)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterfotos_abrufen' ist fehlgeschlagen: {failed.Info}.");
                        }
                    }

                    throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterfotos_abrufen' ist fehlgeschlagen: Unbekanntes Protokoll ({task.Result.GetType().Name}).");
                });
        }

        public Task<Stammdaten<Mitarbeiterfoto>> Mitarbeiterfotos_abrufen_ab(Datenstand stand)
        {
            _log.Debug("Mitarbeiterfotos für Datenstand werden abgerufen ...");
            return _schicht_2_Proxy
                .Query_Mitarbeiterfotos(_credentials, API_VERSION, _session, stand)
                .ContinueWith(task =>
                {
                    if (task.Result is QueryResult result)
                    {
                        var fotos = Deserialize.Deserialisiere_Mitarbeiterfotos(result.Data);
                        _log.Debug($"{fotos.Count} Mitarbeiterfotos empfangen");
                        return fotos;
                    }
                    if (task.Result is QueryFailed failed)
                    {
                        if (failed.Reason == QueryFailure.Internal_Server_Error)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterfotos_abrufen_ab' ist auf dem Server fehlgeschlagen: {failed.Info}.");
                        }
                        if (failed.Reason == QueryFailure.Unauthorized)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterfotos_abrufen_ab' ist fehlgeschlagen: {failed.Info}.");
                        }
                        if (failed.Reason == QueryFailure.Unknown_reason)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterfotos_abrufen_ab' ist fehlgeschlagen: {failed.Info}.");
                        }
                    }

                    throw new ConnectionErrorException($"Die Datenabfrage 'Mitarbeiterfotos_abrufen_ab' ist fehlgeschlagen: Unbekanntes Protokoll ({task.Result.GetType().Name}).");
                });
        }

        public Task<Stammdaten<Dienst>> Dienste_abrufen_ab(Datenstand stand)
        {
            _log.Debug("Dienste für Datenstand werden abgerufen ...");
            return _schicht_2_Proxy
                .Query_Dienste(_credentials, API_VERSION, _session, stand)
                .ContinueWith(task =>
                {
                    if (task.Result is QueryResult result)
                    {
                        var dienste = Deserialize.Deserialisiere_Dienste(result.Data);
                        _log.Debug($"{dienste.Count} Dienste empfangen");
                        return dienste;
                    }
                    if (task.Result is QueryFailed failed)
                    {
                        if (failed.Reason == QueryFailure.Internal_Server_Error)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Dienste_abrufen' ist auf dem Server fehlgeschlagen: {failed.Info}.");
                        }
                        if (failed.Reason == QueryFailure.Unauthorized)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Dienste_abrufen' ist fehlgeschlagen: {failed.Info}.");
                        }
                        if (failed.Reason == QueryFailure.Unknown_reason)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Dienste_abrufen' ist fehlgeschlagen: {failed.Info}.");
                        }
                    }

                    throw new ConnectionErrorException($"Die Datenabfrage 'Dienste_abrufen' ist fehlgeschlagen: Unbekanntes Protokoll ({task.Result.GetType().Name}).");
                });
        }

        Task<Stammdaten<Dienst>> DM7_PPLUS_API.Dienste_abrufen()
        {
            _log.Debug("Dienste werden abgerufen ...");
            return _schicht_2_Proxy
                .Query_Dienste(_credentials, API_VERSION, _session, null)
                .ContinueWith(task =>
                {
                    if (task.Result is QueryResult result)
                    {
                        var dienste = Deserialize.Deserialisiere_Dienste(result.Data);
                        _log.Debug($"{dienste.Count} Dienste empfangen");
                        return dienste;
                    }
                    if (task.Result is QueryFailed failed)
                    {
                        if (failed.Reason == QueryFailure.Internal_Server_Error)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Dienste_abrufen' ist auf dem Server fehlgeschlagen: {failed.Info}.");
                        }
                        if (failed.Reason == QueryFailure.Unauthorized)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Dienste_abrufen' ist fehlgeschlagen: {failed.Info}.");
                        }
                        if (failed.Reason == QueryFailure.Unknown_reason)
                        {
                            throw new ConnectionErrorException($"Die Datenabfrage 'Dienste_abrufen' ist fehlgeschlagen: {failed.Info}.");
                        }
                    }

                    throw new ConnectionErrorException($"Die Datenabfrage 'Dienste_abrufen' ist fehlgeschlagen: Unbekanntes Protokoll ({task.Result.GetType().Name}).");
                });
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
}