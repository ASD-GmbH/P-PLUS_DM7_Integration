using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DM7_PPLUS_Integration;
using DM7_PPLUS_Integration.Auswahllisten;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.V2;
using PPLUS = DM7_PPLUS_Integration.Implementierung.V2.PPLUS;

namespace Demo_Implementierung
{
    /// <summary>
    /// Beispiel Client für die DM7-PPLUS API.
    /// Diese Programm ruft Daten von einem unter der angegebenen Adresse laufenden P-PLUS (oder eines Demodaten Server) ab.
    /// </summary>
    public class Program
    {
        private const string DEMO_URL = "demo://60";

        public static void Main()
        {
            var test_Mitarbeiter = new[]
            {
                Heimeshoff(),
                Helmig()
            };

            var logger = new ConsoleLogger();

            var testServer = Test_Server.Instanz()
                .Mit_Authentification("user", "password")
                .Mit_Mitarbeitern(test_Mitarbeiter)
                .Mit_Mitarbeiterfotos(Leeres_Foto_für(Heimeshoff().Id), Leeres_Foto_für(Helmig().Id))
                .Mit_Diensten(Testdienst())
                .Start("127.0.0.1", 2000, out _);

            using (var api = PPLUS.Connect(testServer.ConnectionString, "user", "password", logger).Result)
            {
                Console.WriteLine($"Daten arbeiten mit Auswahllisten Version {api.Auswahllisten_Version}");

                Console.WriteLine("### Initiale Mitarbeiter");
                var mitarbeiter = api.Mitarbeiter_abrufen().Result;
                Mitarbeiter_anzeigen(mitarbeiter);

                testServer.Mitarbeiter_hinzufügen(Willenborg());
                Console.WriteLine("\n### Mitarbeiter nach Hinzufügen");
                Mitarbeiter_anzeigen(api);

                Console.WriteLine("\n### Nur Mitarbeiter seit neuen Stand");
                Mitarbeiter_anzeigen(api.Mitarbeiter_abrufen_ab(mitarbeiter.Stand).Result);

                Console.WriteLine("\n### Initiale Mitarbeiterfotos");
                var fotos = api.Mitarbeiterfotos_abrufen().Result;
                Mitarbeiter_mit_Fotos_anzeigen(fotos, api);

                testServer.Mitarbeiterfotos_hinzufügen(Leeres_Foto_für(Willenborg().Id));
                Console.WriteLine("\n### Neue Mitarbeiterfotos");
                Mitarbeiter_mit_Fotos_anzeigen(api.Mitarbeiterfotos_abrufen_ab(fotos.Stand).Result, api);

                Console.WriteLine("\n### Initiale Dienste");
                Dienste_anzeigen(api);

                testServer.Dienste_anlegen(Frühtour());
                Console.WriteLine("\n### Dienste nach Neuanlage");
                Dienste_anzeigen(api);

                testServer.Revoke_Token();
                Console.WriteLine("\n### Token revoked");
                try { Mitarbeiter_anzeigen(api); }
                catch (AggregateException e) { Console.WriteLine(e.InnerExceptions.First().Message); }
            }

            Console.WriteLine("Beliebige Taste drücken zum Beenden...");
            Console.ReadKey();
            testServer.Dispose();
        }

        private static void Mitarbeiter_anzeigen(DM7_PPLUS_API api)
        {
            var alle_Mitarbeiter = api.Mitarbeiter_abrufen().Result;
            Mitarbeiter_anzeigen(alle_Mitarbeiter);
        }

        private static void Mitarbeiter_mit_Fotos_anzeigen(IEnumerable<Mitarbeiterfoto> fotos, DM7_PPLUS_API api)
        {
            var mitarbeiterbezeichnungen = api.Mitarbeiter_abrufen().Result.ToDictionary(_ => _.Id, _ => $"{_.Nachname}, {_.Vorname}");
            foreach (var foto in fotos)
            {
                Console.WriteLine($" - Bild für {mitarbeiterbezeichnungen[foto.Mitarbeiter]}");
            }
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

        private static string Datum_als_Text(Datum? datum)
        {
            return datum.HasValue
                ? $"{datum.Value.Tag:00}.{datum.Value.Monat:00}.{datum.Value.Jahr:0000}"
                : "[Ende offen]";
        }

        private static Mitarbeiter Heimeshoff() =>
            new Mitarbeiter(
                Guid.Parse("36A19CB3-8969-435A-A00E-B5CE3A71E6CD"),
                new ReadOnlyCollection<DM7_Mandantenzugehörigkeit>(
                    new List<DM7_Mandantenzugehörigkeit>
                    {
                        new DM7_Mandantenzugehörigkeit(Mandant_1, new Datum(01, 01, 2016), null)
                    }),
                "Pnr 007",
                Auswahllisten_1.Titel.Kein,
                "Marco",
                "Heimeshoff",
                null,
                "Hei",
                new Datum(19, 11, 1981),
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
                        new DM7_Mandantenzugehörigkeit(Mandant_1, new Datum(01, 01, 2018), null)
                    }),
                "Pnr 001",
                Auswahllisten_1.Titel.Kein,
                "Nils",
                "Helmig",
                null,
                "Nil",
                new Datum(10, 08, 1995),
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
                        new DM7_Mandantenzugehörigkeit(Mandant_2, new Datum(01, 01, 2010), null)
                    }),
                "Pnr DM7",
                Auswahllisten_1.Titel.Kein,
                "Dennis",
                "Willenborg",
                null,
                "Wil",
                new Datum(12, 05, 1975),
                Auswahllisten_1.Geschlecht.Maennlich,
                Auswahllisten_1.Konfession.Keine,
                Auswahllisten_1.Familienstand.Verheiratet,
                new ReadOnlyCollection<Qualifikation>(new List<Qualifikation>()),
                new ReadOnlyCollection<Kontakt>(new List<Kontakt>())
            );

        private static Mitarbeiterfoto Leeres_Foto_für(Guid mitarbeiter) =>
            new Mitarbeiterfoto(mitarbeiter, Guid.Empty, new byte[0]);

        private static Dienst Frühtour() =>
            new Dienst(
                2,
                Guid.NewGuid(),
                "F",
                "Frühtour",
                new Datum(21, 01, 2019),
                new Datum(31, 12, 2021),
                Uhrzeit.HHMM(05, 00),
                new Dienst_Gültigkeit(true, true, true, true, true, false, false, true),
                false);

        private static Dienst Testdienst() =>
            new Dienst(
                1,
                Guid.NewGuid(),
                "Test",
                "Testdienst",
                new Datum(21, 01, 2020),
                null,
                Uhrzeit.HHMM(09, 30),
                new Dienst_Gültigkeit(true, true, true, true, false, false, false, false),
                false);

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
