using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DM7_PPLUS_Integration;
using DM7_PPLUS_Integration.Auswahllisten;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung;

namespace Demo_Implementierung
{
    /// <summary>
    /// Beispiel Client für die DM7-PPLUS API.
    /// Diese Programm ruft Daten von einem unter der angegebenen Adresse laufenden P-PLUS (oder eines Demodaten Server) ab.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            var test_Mitarbeiter = new[]
            {
                Heimeshoff(),
                Helmig()
            };

            //var logger = new ConsoleLogger();
            var logger = new NoLogger();

            var testServer = Test_Server.Instanz()
                .Mit_Authentification("user", "password")
                .Mit_Mitarbeitern(test_Mitarbeiter)
                .Mit_Diensten(Testdienst())
                .Mit_Dienstbuchungen_für(Mandant_1, Heute(), 
                    Ist_im_Testdienst(Helmig()),
                    Ist_im_Testdienst(Heimeshoff()))
                .Mit_Abwesenheiten_für(Mandant_1,
                    Krank(Heimeshoff()),
                    Verplant(Helmig()))
                .Start("127.0.0.1", 2000, out var key);

            using (testServer)
            {
                //var api = PPLUS.Connect(testServer.ConnectionString, "user", "password", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef")), logger).Result;
                var api = PPLUS.Connect(testServer.ConnectionString, "user", "password", key, logger).Result;
                Console.WriteLine($"Daten arbeiten mit Auswahllisten Version {api.Auswahllisten_Version}");

                Console.WriteLine("### Initiale Mitarbeiter");
                var mitarbeiter = api.Mitarbeiter_abrufen().Result;
                Mitarbeiter_anzeigen(mitarbeiter);

                testServer.Mitarbeiter_hinzufügen(Willenborg());
                Console.WriteLine("\n### Mitarbeiter nach Hinzufügen");
                Mitarbeiter_anzeigen(api);

                Console.WriteLine("\n### Nur Mitarbeiter seit neuen Stand");
                Mitarbeiter_anzeigen(api.Mitarbeiter_abrufen_ab(mitarbeiter.Stand).Result);

                Console.WriteLine("\n### Initiale Dienste");
                Dienste_anzeigen(api);

                testServer.Dienste_anlegen(Frühtour());
                Console.WriteLine("\n### Dienste nach Neuanlage");
                Dienste_anzeigen(api);

                Console.WriteLine("\n### Initiale Dienstbuchungen");
                Dienstbuchungen_anzeigen(Mandant_1, Heute(), api);

                testServer.Dienste_buchen(Mandant_1, Heute(), Fährt_Frühtour(Willenborg()));
                Console.WriteLine("\n### Dienstbuchungen nach Buchung");
                Dienstbuchungen_anzeigen(Mandant_1, Heute(), api);

                testServer.Dienst_streichen(Mandant_1, Heute(), Testdienst().Id);
                Console.WriteLine("\n### Dienstbuchungen nach Streichung");
                Dienstbuchungen_anzeigen(Mandant_1, Heute(), api);

                Console.WriteLine("\n### Initiale Abwesenheiten zum 04.02.2021");
                Abwesenheiten_anzeigen(Mandant_1, Datum.DD_MM_YYYY(04, 02, 2021), api);

                Console.WriteLine("\n### Initiale Abwesenheiten zum 10.02.2021");
                Abwesenheiten_anzeigen(Mandant_1, Datum.DD_MM_YYYY(10, 02, 2021), api);

                testServer.Abwesenheiten_eintragen(Mandant_1, Urlaub(Willenborg()));
                Console.WriteLine("\n### Abwesenheiten zum 20.02.2021 nach Eintragung");
                Abwesenheiten_anzeigen(Mandant_1, Datum.DD_MM_YYYY(20, 02, 2021), api);

                Console.WriteLine("\n### Abwesenheiten zum 04.02.2021 nach Eintragung");
                Abwesenheiten_anzeigen(Mandant_1, Datum.DD_MM_YYYY(04, 02, 2021), api);

                testServer.Dienste_buchen(Mandant_1, Heute(), Fährt_Frühtour(Heimeshoff()));
                Console.WriteLine("\n### Dienstbuchungen vor Soll/Ist Abgleich");
                Dienstbuchungen_anzeigen(Mandant_1, Heute(), api);

                Console.WriteLine("\n### Soll/Ist Abgleich freigeben");
                Soll_Ist_Abgleich_freigeben(api);

                Console.WriteLine("\n### Dienstbuchungen nach Soll/Ist Abgleich");
                Dienstbuchungen_anzeigen(Mandant_1, Heute(), api);

                testServer.Revoke_Token();
                Console.WriteLine("\n### Token revoked");
                try { Mitarbeiter_anzeigen(api); }
                catch (AggregateException e) { Console.WriteLine(e.InnerExceptions.First().Message); }
            }

            Console.WriteLine("Beliebige Taste drücken zum Beenden...");
            Console.ReadKey();
        }

        private static void Mitarbeiter_anzeigen(DM7_PPLUS_API api)
        {
            var alle_Mitarbeiter = api.Mitarbeiter_abrufen().Result;
            Mitarbeiter_anzeigen(alle_Mitarbeiter);
        }

        private static void Mitarbeiter_anzeigen(IEnumerable<Mitarbeiter> alle_Mitarbeiter)
        {
            foreach (var mitarbeiter in alle_Mitarbeiter)
            {
                var mandantenzugehörigkeit = mitarbeiter.DM7_Mandantenzugehörigkeiten.First();
                Console.WriteLine($" - {mitarbeiter.Nachname}, {mitarbeiter.Vorname} - Zugehörig ab {Datum_als_Text(mandantenzugehörigkeit.GueltigAb)} bis {Datum_als_Text(mandantenzugehörigkeit.GueltigBis)}");
            }
        }

        private static void Dienste_anzeigen(DM7_PPLUS_API api)
        {
            var dienste = api.Dienste_abrufen().Result;
            foreach (var dienst in dienste)
            {
                Console.WriteLine($" - {dienst.Bezeichnung} ({dienst.Kurzbezeichnung}){(dienst.Gelöscht ? " [Gelöscht]" : "")}");
            }
        }

        private static void Dienstbuchungen_anzeigen(Guid mandantId, Datum stichtag, DM7_PPLUS_API api)
        {
            var mitarbeiter_lookup = api.Mitarbeiter_abrufen().Result.ToDictionary(_ => _.Id, _ => $"{_.Nachname}, {_.Vorname}");
            var dienst_lookup = api.Dienste_abrufen().Result.ToDictionary(_ => _.Id, _ => _.Bezeichnung);
            var dienstbuchungen = api.Dienstbuchungen_zum_Stichtag(stichtag, mandantId).Result;
            foreach (var dienstbuchung in dienstbuchungen)
            {
                var mitarbeiter =
                    mitarbeiter_lookup.ContainsKey(dienstbuchung.MitarbeiterId)
                        ? mitarbeiter_lookup[dienstbuchung.MitarbeiterId]
                        : "[Unbekannter Mitarbeiter]";
                var dienst =
                    dienst_lookup.ContainsKey(dienstbuchung.DienstId)
                        ? dienst_lookup[dienstbuchung.DienstId]
                        : "[Unbekannter Dienst]";
                Console.WriteLine($" - {mitarbeiter}: {dienst} - {Datum_als_Text(stichtag)} {Uhrzeit_als_Text(dienstbuchung.Beginnt_um)}");
            }
        }

        private static void Abwesenheiten_anzeigen(Guid mandantId, Datum stichtag, DM7_PPLUS_API api)
        {
            var mitarbeiter_lookup = api.Mitarbeiter_abrufen().Result.ToDictionary(_ => _.Id, _ => $"{_.Nachname}, {_.Vorname}");
            var abwesenheiten = api.Abwesenheiten_zum_Stichtag(stichtag, mandantId).Result;
            foreach (var abwesenheit in abwesenheiten)
            {
                var mitarbeiter =
                    mitarbeiter_lookup.ContainsKey(abwesenheit.MitarbeiterId)
                        ? mitarbeiter_lookup[abwesenheit.MitarbeiterId]
                        : "[Unbekannter Mitarbeiter]";
                Console.WriteLine($" - {mitarbeiter}: {abwesenheit.Grund} für Zeitraum {Zeitpunkt_als_Text(abwesenheit.Abwesend_ab)} - {Zeitpunkt_als_Text(abwesenheit.Vorraussichtlich_wieder_verfügbar_ab)}");
            }
        }

        private static void Soll_Ist_Abgleich_freigeben(DM7_PPLUS_API api)
        {
            var nicht_gefahrene_Touren =
                new[]
                {
                    new Nicht_gefahrene_Tour(Heimeshoff().Id, Mandant_1, Frühtour().Id, Heute())
                };
            var abgleich = new Soll_Ist_Abgleich(
                new ReadOnlyCollection<Ungeplante_Tour>(new List<Ungeplante_Tour>()),
                new ReadOnlyCollection<Geplante_Tour>(new List<Geplante_Tour>()),
                new ReadOnlyCollection<Nicht_gefahrene_Tour>(nicht_gefahrene_Touren));
            var ergebnis = api.Soll_Ist_Abgleich_freigeben(abgleich).Result;

            Console.WriteLine($"Ergebnis {ergebnis.GetType().Name}");
        }
        
        private static string Datum_als_Text(Datum? datum)
        {
            return datum.HasValue
                ? $"{datum.Value.Tag:00}.{datum.Value.Monat:00}.{datum.Value.Jahr:0000}"
                : "[Ende offen]";
        }

        private static string Uhrzeit_als_Text(Uhrzeit uhrzeit) => $"{uhrzeit.Stunden:00}:{uhrzeit.Minuten:00}";
        private static string Zeitpunkt_als_Text(Zeitpunkt zeitpunkt) => $"{Datum_als_Text(zeitpunkt.Datum)} {Uhrzeit_als_Text(zeitpunkt.Uhrzeit)}";

        private static Datum Heute()
        {
            var heute = DateTime.Now;
            return Datum.DD_MM_YYYY(heute.Day, heute.Month, heute.Year);
        }

        private static Mitarbeiter Heimeshoff() =>
            new Mitarbeiter(
                Guid.Parse("36A19CB3-8969-435A-A00E-B5CE3A71E6CD"),
                new ReadOnlyCollection<DM7_Mandantenzugehörigkeit>(
                    new List<DM7_Mandantenzugehörigkeit>
                    {
                        new DM7_Mandantenzugehörigkeit(Mandant_1, Datum.DD_MM_YYYY(01, 01, 2016), null)
                    }),
                "Pnr 007",
                Auswahllisten_1.Titel.Kein,
                "Marco",
                "Heimeshoff",
                null,
                "Hei",
                Datum.DD_MM_YYYY(19, 11, 1981),
                Auswahllisten_1.Geschlecht.Maennlich,
                Auswahllisten_1.Konfession.Keine,
                Auswahllisten_1.Familienstand.Verheiratet,
                new ReadOnlyCollection<Qualifikation>(new List<Qualifikation>()),
                new ReadOnlyCollection<Kontakt>(new List<Kontakt>())
            );

        private static Mitarbeiter Helmig() =>
            new Mitarbeiter(
                Guid.Parse("49C696E4-4A14-47F2-8211-F931E2C75148"),
                new ReadOnlyCollection<DM7_Mandantenzugehörigkeit>(
                    new List<DM7_Mandantenzugehörigkeit>
                    {
                        new DM7_Mandantenzugehörigkeit(Mandant_1, Datum.DD_MM_YYYY(01, 01, 2018), null)
                    }),
                "Pnr 001",
                Auswahllisten_1.Titel.Kein,
                "Nils",
                "Helmig",
                null,
                "Nil",
                Datum.DD_MM_YYYY(10, 08, 1995),
                Auswahllisten_1.Geschlecht.Maennlich,
                Auswahllisten_1.Konfession.Keine,
                Auswahllisten_1.Familienstand.Ledig,
                new ReadOnlyCollection<Qualifikation>(new List<Qualifikation>()),
                new ReadOnlyCollection<Kontakt>(new List<Kontakt>())
            );
        
        private static Mitarbeiter Willenborg() =>
            new Mitarbeiter(
                Guid.Parse("4A0694F4-02D1-466E-9F94-2D417140AE67"),
                new ReadOnlyCollection<DM7_Mandantenzugehörigkeit>(
                    new List<DM7_Mandantenzugehörigkeit>
                    {
                        new DM7_Mandantenzugehörigkeit(Mandant_2, Datum.DD_MM_YYYY(01, 01, 2010), null)
                    }),
                "Pnr DM7",
                Auswahllisten_1.Titel.Kein,
                "Dennis",
                "Willenborg",
                null,
                "Wil",
                Datum.DD_MM_YYYY(12, 05, 1975),
                Auswahllisten_1.Geschlecht.Maennlich,
                Auswahllisten_1.Konfession.Keine,
                Auswahllisten_1.Familienstand.Verheiratet,
                new ReadOnlyCollection<Qualifikation>(new List<Qualifikation>()),
                new ReadOnlyCollection<Kontakt>(new List<Kontakt>())
            );

        private static Dienst Frühtour() =>
            new Dienst(
                2,
                new ReadOnlyCollection<DM7_Mandantenzugehörigkeit>(
                    new List<DM7_Mandantenzugehörigkeit>
                    {
                        new DM7_Mandantenzugehörigkeit(Mandant_1, new Datum(01, 01, 2010), null)
                    }),
                "F",
                "Frühtour",
                new Dienst_Gültigkeit(true, true, true, true, true, false, false, true),
                false);

        private static Dienst Testdienst() =>
            new Dienst(
                1,
                new ReadOnlyCollection<DM7_Mandantenzugehörigkeit>(
                    new List<DM7_Mandantenzugehörigkeit>
                    {
                        new DM7_Mandantenzugehörigkeit(Mandant_1, Datum.DD_MM_YYYY(01, 01, 2010), Datum.DD_MM_YYYY(31, 12, 2014)),
                        new DM7_Mandantenzugehörigkeit(Mandant_2, Datum.DD_MM_YYYY(01, 01, 2015), null),
                        new DM7_Mandantenzugehörigkeit(Mandant_1, Datum.DD_MM_YYYY(01, 01, 2016), null)
                    }),
                "Test",
                "Testdienst",
                new Dienst_Gültigkeit(true, true, true, true, false, false, false, false),
                false);

        private static Dienstbuchung Fährt_Frühtour(Mitarbeiter mitarbeiter) =>
            new Dienstbuchung(
                mitarbeiter.Id,
                Frühtour().Id,
                Uhrzeit.HH_MM(06, 30));

        private static Dienstbuchung Ist_im_Testdienst(Mitarbeiter mitarbeiter) =>
            new Dienstbuchung(
                mitarbeiter.Id,
                Testdienst().Id,
                Uhrzeit.HH_MM(14, 45));

        private static Abwesenheit Krank(Mitarbeiter mitarbeiter) =>
            new Abwesenheit(
                mitarbeiter.Id,
                Zeitpunkt.DD_MM_YYYY_HH_MM(10, 02, 2021, 07, 45),
                Zeitpunkt.DD_MM_YYYY_HH_MM(10, 02, 2021, 17, 00),
                "Krank",
                Abwesenheitsart.Fehlzeit);

        private static Abwesenheit Urlaub(Mitarbeiter mitarbeiter) =>
            new Abwesenheit(
                mitarbeiter.Id,
                Zeitpunkt.DD_MM_YYYY_HH_MM(08, 02, 2021, 00, 00),
                Zeitpunkt.DD_MM_YYYY_HH_MM(22, 02, 2021, 00, 00),
                "Urlaub",
                Abwesenheitsart.Fehlzeit);

        private static Abwesenheit Verplant(Mitarbeiter mitarbeiter) =>
            new Abwesenheit(
                mitarbeiter.Id,
                Zeitpunkt.DD_MM_YYYY_HH_MM(04, 02, 2021, 11, 30),
                Zeitpunkt.DD_MM_YYYY_HH_MM(04, 02, 2021, 14, 15),
                "Testdienst",
                Abwesenheitsart.Andersweitig_verplant);

        private static Guid Mandant_1 => Guid.Parse("58B053FA-1501-4DC2-B575-88F20CD3EFE5");
        private static Guid Mandant_2 => Guid.Parse("19651D9E-7C12-49D4-8860-70E5C0CF0199");
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

        private static string Prefix => $"[{DateTime.Now:d-HH:mm:ss.fff}] ";
    }

    internal class NoLogger : Log
    {
        public void Info(string text)
        {
            // Blank
        }

        public void Debug(string text)
        {
            // Blank
        }
    }
}
