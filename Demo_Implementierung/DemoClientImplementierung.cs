﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using DM7_PPLUS_Integration;
using DM7_PPLUS_Integration.Auswahllisten;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Server;

namespace Demo_Implementierung
{
    /// <summary>
    /// Beispiel Client für die DM7-PPLUS API.
    /// Diese Programm ruft Daten von einem unter der angegebenen Adresse laufenden P-PLUS (oder eines Demodaten Server) ab.
    /// </summary>
    class Program
    {
        private const string DEMO_URL = "demo://60";

        /// <summary>
        /// Repräsentiert die lokale Persistenz, z.B. die HVP Datenbank
        /// </summary>
        private static readonly Dictionary<Guid, Mitarbeiter> _mitarbeiter_datensaetze = new Dictionary<Guid, Mitarbeiter>();

        private static readonly object _lock = new object();

        /// <summary>
        /// Aktuell läuft eine Abfrage
        /// </summary>
        private static bool _query_in_Progress;

        /// <summary>
        /// Datenversionsstand, der z.B. in HVP bereits vorliegt
        /// </summary>
        private static Stand _stand_im_Cache;

        /// <summary>
        /// Datenversionsstand, der von P-PLUS zur Verfügung gestellt wird
        /// </summary>
        private static Stand _verfuegbarer_Stand;


        /// <summary>
        /// Datenversionstand, der aktuell abgerufen ist oder wird
        /// </summary>
        private static Stand _angeforderter_Stand;


        static void Main(string [] args)
        {
            var log = new ConsoleLogger();
            var url = (args.Length > 0) ? args[0] : DEMO_URL;
            var credentials = (args.Length > 1) ? args[1] : "anonymous";

            demo_Dienste_abfragen(url, credentials, log);
            // demo_Mitarbeiter_abfragen(url, credentials, log)
            Thread.Sleep(2000);
        }

        static void demo_Dienste_abfragen(string url, string credentials, ConsoleLogger log)
        {
            // Rückgabe von Connect muss Disposed werden, um alle Verbindungen zu schließen
            using (var api = PPLUS.Connect(url, credentials, log, CancellationToken.None).Result)
            {
                Console.Out.WriteLine($"- Auswahlliste: {api.Auswahllisten_Version}");
                var mitarbeiters = api.Mitarbeiter_abrufen().Result.ToList();
                var mitarbeitersfotos = api.Mitarbeiterfotos_abrufen().Result.ToList();
                var dienste = api.Dienste_abrufen().Result;
            
                Dienste_ausgeben(dienste);

                Console.Out.WriteLine("- Press any key to quit.");
                Console.ReadKey();
            }
        }
        
        static void demo_Mitarbeiter_abfragen(string url, string credentials, ConsoleLogger log)
        {
            var api = Connect_mit_Abbruchmoeglichkeit(url, credentials, log);

            if (api != null)
            {
                // Rückgabe von Connect muss Disposed werden, um alle Verbindungen zu schließen
                using (api)
                {
                    Console.Out.WriteLine($"- Auswahlliste: {api.Auswahllisten_Version}");

                    // Beim Neustart wird einmal der Stammdatenbestand abgerufen
                    UpdateCache(api.Mitarbeiterdaten_abrufen().Result); // <-- im Echteinsatz nicht blockierend...
                    _verfuegbarer_Stand = _stand_im_Cache;
                    _angeforderter_Stand = _stand_im_Cache;

                    // Subscription auf Änderungen, muss Disposed werden, um die Subscription aufzuheben
                    using (api.Stand_Mitarbeiterdaten.Subscribe(
                        new
                            Observer<Stand>(
                                // ReSharper disable once AccessToDisposedClosure
                                s => Neuen_Stand_vermerken(api, s),
                                ex => log.Debug(ex.Message))))
                    {
                        Console.Out.WriteLine("- Press any key to quit.");
                        Console.ReadKey();
                    }
                }
            }
            else
            {
                Console.Out.WriteLine("Verbindungsaufbau wurde abgebrochen.");
            }
        }

        private static void Dienste_ausgeben(IEnumerable<Dienst> dienste)
        {
            string Uhrzeit_als_Text(Uhrzeit uhrzeit) => $"{uhrzeit.Stunden:00}:{uhrzeit.Minuten:00}";
            string Datum_als_Text(Datum datum) => $"{datum.Tag:00}.{datum.Monat:00}.{datum.Jahr:0000}";
            string Checkbox(bool check) => check ? "[x]" : "[ ]";

            string Gültigkeit_als_Text(Dienst_Gültigkeit gültigkeit) => $"Mo: {Checkbox(gültigkeit.Montag)}, Di: {Checkbox(gültigkeit.Dienstag)}, Mi: {Checkbox(gültigkeit.Mittwoch)}, Do: {Checkbox(gültigkeit.Donnerstag)}, Fr: {Checkbox(gültigkeit.Freitag)}, Sa: {Checkbox(gültigkeit.Samstag)}, So: {Checkbox(gültigkeit.Sonntag)}, Fei: {Checkbox(gültigkeit.Feiertags)}";
            string Dienst_als_Text(Dienst dienst) => $"- {dienst.Bezeichnung} ({dienst.Kurzbezeichnung})\n\t- ID: {dienst.Id}\n\t- Mandant: {dienst.Mandant}\n\t- Gültig ab: {Datum_als_Text(dienst.Gültig_ab)}\n\t- Gültig bis: {(dienst.Gültig_bis.HasValue ? Datum_als_Text(dienst.Gültig_bis.Value) : "Ende offen")}\n\t- Dienstbeginn: {Uhrzeit_als_Text(dienst.Beginn)}\n\t- Gültig an {Gültigkeit_als_Text(dienst.Gültig_an)}\n\t- Gelöscht: {Checkbox(dienst.Gelöscht)}";
            Console.WriteLine($"Definierte Dienste\n---\n{string.Join("\n", dienste.Select(Dienst_als_Text))}");
        }

        // ReSharper disable once UnusedMember.Local
        private static DM7_PPLUS_API Connect(string url, string credentials, Log log)
        {
            return PPLUS.Connect(url, credentials, log, CancellationToken.None).Result;
        }

        private static DM7_PPLUS_API Connect_mit_Abbruchmoeglichkeit(string url, string credentials, Log log)
        {
            Console.TreatControlCAsInput = true;
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken_Verbindung = cancellationTokenSource.Token;

            try
            {
                DM7_PPLUS_API result = null;

                var task = PPLUS.Connect(url, credentials, log, cancellationToken_Verbindung);

                var cancel = false;
                var info = "Verbindung konnte noch nicht hergestellt werden. Bitte warten - oder eine Taste drücken, um abzubrechen.";
                while (result == null && !cancel)
                {
                    try
                    {
                        if (task.Wait(TimeSpan.FromMilliseconds(2000)))
                        {
                            result = task.Result;
                        }
                        else
                        {
                            if (Console.KeyAvailable)
                            {
                                while (Console.KeyAvailable) Console.ReadKey(true);
                                cancel = true;
                            }
                            else
                            {
                                Console.Out.WriteLine(info);
                                info = ".";
                            }
                        }
                    }
                    catch (AggregateException ex)
                    {
                        ex.Handle(inner =>
                        {
                            {
                                if (inner is ConnectionErrorException ex2)
                                {
                                    Console.Out.WriteLine($"Fehler beim Verbindungsaufbau: {ex2.Message}");
                                    return true;
                                }
                            }
                            {
                                if (inner is UnsupportedVersionException ex2)
                                {
                                    Console.Out.WriteLine($"API Version nicht unterstützt: {ex2.Message}");
                                    return true;
                                }
                            }
                            return false;
                        });
                    }
                }

                if (cancel) cancellationTokenSource.Cancel();
                return result;

            }
            finally
            {
                Console.TreatControlCAsInput = false;
                cancellationTokenSource.Dispose();
            }
        }

        /// <summary>
        /// Vermerkt einen neuen Datenversionsstand, um die entsprechenden Änderungen entweder sofort (falls aktuell keine Abfrage läuft) oder später abzurufen.
        /// </summary>
        private static void Neuen_Stand_vermerken(DM7_PPLUS_API api, Stand neuer_stand)
        {
            lock (_lock) _verfuegbarer_Stand = neuer_stand;
            Neue_Daten_abrufen_falls_erforderlich_und_moeglich(api);
        }

        /// <summary>
        /// Prüft, ob neue Daten bereit stehen; falls ja und aktuell keine Abfrage läuft, wird eine Abfrage gestartet.
        /// </summary>
        private static void Neue_Daten_abrufen_falls_erforderlich_und_moeglich(DM7_PPLUS_API api)
        {
            Action query = null;

            lock (_lock)
            {
                if (_angeforderter_Stand != _verfuegbarer_Stand && !_query_in_Progress)
                {
                    var von = _angeforderter_Stand;
                    var bis = _verfuegbarer_Stand;

                    query = () =>
                    {
                        Aktualisierung_anfordern(api, von, bis);
                    };

                    _query_in_Progress = true;
                    _angeforderter_Stand = bis;
                }
            }

            if (query != null) query();
        }

        /// <summary>
        /// Ruft aktualisierte Daten vom Server ab, speichert diese im lokalen Datenbestand und prüft danach, ob zwischenzeitlich eine weitere Aktualisierung notwendig wurde.
        /// Dadurch wird sichergestellt, dass Aktualisierungen in der korrekten Reihenfolge verarbeitet werden.
        /// </summary>
        private static void Aktualisierung_anfordern(DM7_PPLUS_API api, Stand von, Stand bis)
        {
            api.Mitarbeiterdaten_abrufen(von, bis)
                .ContinueWith(task =>
                {
                    UpdateCache(task.Result);
                    Neue_Daten_abrufen_falls_erforderlich_und_moeglich(api);
                });
        }

        /// <summary>
        /// Aktualisiert den lokalen Datenbestand
        /// </summary>
        private static void UpdateCache(Mitarbeiterdatensaetze daten)
        {
            lock (_lock)
            {
                if (!daten.Teilmenge) _mitarbeiter_datensaetze.Clear();

                foreach (var datensatz in daten.Mitarbeiter)
                {
                    if (!_mitarbeiter_datensaetze.ContainsKey(datensatz.PPLUS_Id))
                    {
                        _mitarbeiter_datensaetze.Add(datensatz.PPLUS_Id, datensatz);
                    }
                    else
                    {
                        _mitarbeiter_datensaetze[datensatz.PPLUS_Id] = datensatz;
                    }
                }

                _stand_im_Cache = daten.Stand;
                _query_in_Progress = false;
            }

            Console.Out.WriteLine($"- {(daten.Teilmenge ? "Neu oder geändert" : "Vollständige Liste")} in Stand {daten.Stand}:");
            foreach (var mitarbeiter in daten.Mitarbeiter)
            {
                Console.Out.WriteLine($"- Mitarbeiter {mitarbeiter.Personalnummer}: {mitarbeiter.Vorname}, {mitarbeiter.Nachname}. Mandant ({mitarbeiter.PPLUS_Id})");
            }
        }
    }




    /// <summary>
    /// Simple IObserver Implementierung für diese Demo. Im Echteinsatz bitte z.B. Reactive Extensions verwenden.
    /// </summary>
    internal class Observer<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;

        public Observer(Action<T> onNext, Action<Exception> onError)
        {
            _onNext = onNext;
            _onError = onError;
        }

        public void OnNext(T value)
        {
            _onNext(value);
        }

        public void OnError(Exception error)
        {
            _onError(error);
        }

        public void OnCompleted()
        {
            throw new NotSupportedException("OnComplete ist für " + typeof(T).Name + " nicht vorgesehen.");
        }
    }




    /// <summary>
    /// Beispiel des durch DM zu implementierenden Log, das von P-PLUS genutzt wird, um betriebliche Meldungen auszugeben (z.B. Verbindungsstatus etc.).
    /// </summary>
    internal class ConsoleLogger : Log
    {
        private readonly object _console = new object();

        public void Info(string text)
        {
            lock (_console)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Out.Write(Prefix);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine(text);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public void Debug(string text)
        {
            //return;
            lock (_console)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Out.Write(Prefix);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Out.WriteLine(text);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private string Prefix => $"[{DateTime.Now:d-HH:mm:ss.fff}|{Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(3)}] ";
    }
}
