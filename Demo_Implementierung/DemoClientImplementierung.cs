﻿using System;
using System.Collections.Generic;
using System.Threading;
using DM7_PPLUS_Integration;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Shared;

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
        private static readonly Dictionary<Guid, Mitarbeiterdatensatz> _mitarbeiter = new Dictionary<Guid, Mitarbeiterdatensatz>();

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

            // Rückgabe von Connect muss Disposed werden, um alle Verbindungen zu schließen
            using (var api = PPLUS.Connect(url, log).Result)
            {
                if (api==null) throw new ApplicationException("P-PLUS API war unterwarteterweise <null>.");

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

            Thread.Sleep(2000);
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
                if (!daten.Teilmenge) _mitarbeiter.Clear();

                foreach (var datensatz in daten.Mitarbeiter)
                {
                    if (!_mitarbeiter.ContainsKey(datensatz.Id))
                    {
                        _mitarbeiter.Add(datensatz.Id, datensatz);
                    }
                    else
                    {
                        _mitarbeiter[datensatz.Id] = datensatz;
                    }
                }

                _stand_im_Cache = daten.Stand;
                _query_in_Progress = false;
            }

            Console.Out.WriteLine($"- {(daten.Teilmenge ? "Neu oder geändert" : "Vollständige Liste")} in Stand {daten.Stand}:");
            foreach (var mitarbeiter in daten.Mitarbeiter)
            {
                Console.Out.WriteLine($"- Mitarbeiter {mitarbeiter.Personalnummer}: {mitarbeiter.Vorname}, {mitarbeiter.Nachname}");
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