using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration;
using DM7_PPLUS_Integration.Daten;

namespace DumpData
{
    class Program
    {
        private const string DEMO_URL = "demo://60";

        private static readonly Dictionary<string, Mitarbeiterdatensatz> _mitarbeiter_datensaetze =
            new Dictionary<string, Mitarbeiterdatensatz>();

        private static readonly object _lock = new object();

        private static bool _query_in_Progress;

        private static Stand _stand_im_Cache;

        private static Stand _verfuegbarer_Stand;


        private static Stand _angeforderter_Stand;


        static void Main(string[] args)
        {
            var log = new Observer<Stand>.ConsoleLogger();

            var url = (args.Length > 0) ? args[0] : DEMO_URL;

            var api = Connect_mit_Abbruchmoeglichkeit(url, log);

            if (api != null)
            {
                using (api)
                {
                    Console.Out.WriteLine($"- Auswahlliste: {api.Auswahllisten_Version}");

                    UpdateCache(api.Mitarbeiterdaten_abrufen().Result); // <-- im Echteinsatz nicht blockierend...
                    _verfuegbarer_Stand = _stand_im_Cache;
                    _angeforderter_Stand = _stand_im_Cache;

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

            Thread.Sleep(2000);
        }

        private static DM7_PPLUS_API Connect_mit_Abbruchmoeglichkeit(string url, Log log)
        {
            Console.TreatControlCAsInput = true;
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken_Verbindung = cancellationTokenSource.Token;

            try
            {
                DM7_PPLUS_API result = null;

                var task = PPLUS.Connect(url, log, cancellationToken_Verbindung);

                var cancel = false;
                var info =
                    "Verbindung konnte noch nicht hergestellt werden. Bitte warten - oder eine Taste drücken, um abzubrechen.";
                while (result == null && !cancel)
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

                if (cancel) cancellationTokenSource.Cancel();
                return result;

            }
            finally
            {
                Console.TreatControlCAsInput = false;
                cancellationTokenSource.Dispose();
            }
        }

        private static void Neuen_Stand_vermerken(DM7_PPLUS_API api, Stand neuer_stand)
        {
            lock (_lock) _verfuegbarer_Stand = neuer_stand;
            Neue_Daten_abrufen_falls_erforderlich_und_moeglich(api);
        }

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

        private static void Aktualisierung_anfordern(DM7_PPLUS_API api, Stand von, Stand bis)
        {
            api.Mitarbeiterdaten_abrufen(von, bis)
                .ContinueWith(task =>
                {
                    UpdateCache(task.Result);
                    Neue_Daten_abrufen_falls_erforderlich_und_moeglich(api);
                });
        }

        private static void UpdateCache(Mitarbeiterdatensaetze daten)
        {
            lock (_lock)
            {
                if (!daten.Teilmenge) _mitarbeiter_datensaetze.Clear();

                foreach (var datensatz in daten.Mitarbeiter)
                {
                    if (!_mitarbeiter_datensaetze.ContainsKey(datensatz.DatensatzId))
                    {
                        _mitarbeiter_datensaetze.Add(datensatz.DatensatzId, datensatz);
                    }
                    else
                    {
                        _mitarbeiter_datensaetze[datensatz.DatensatzId] = datensatz;
                    }
                }

                _stand_im_Cache = daten.Stand;
                _query_in_Progress = false;
            }

            Console.Out.WriteLine(
                $"- {(daten.Teilmenge ? "Neu oder geändert" : "Vollständige Liste")} in Stand {daten.Stand}:");
            foreach (var mitarbeiter in daten.Mitarbeiter)
            {
                Console.Out.WriteLine($"\n- Mitarbeiter {mitarbeiter.Personalnummer}\n{mitarbeiter.Vorname}, {mitarbeiter.Nachname} {mitarbeiter.DatensatzId}");
                Console.Out.WriteLine($"{mitarbeiter.ArbeitsverhaeltnisId} {mitarbeiter.PersonId} {mitarbeiter.GueltigAb} {mitarbeiter.GueltigBis}");
                Console.Out.WriteLine($"{mitarbeiter.Struktur}");
                foreach (var kontakt in mitarbeiter.Kontakte) {
                    Console.Out.WriteLine($"{kontakt.Kontaktart} {kontakt.Kontaktform} {kontakt.Eintrag}");
                }
                foreach (var quali in mitarbeiter.Qualifikation) {
                    Console.Out.WriteLine($"{quali.Bezeichnung} {quali.Stufe}");
                }
            }
        }
    }




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

            private string Prefix =>
                $"[{DateTime.Now:d-HH:mm:ss.fff}|{Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(3)}] ";
        }
    }
}
