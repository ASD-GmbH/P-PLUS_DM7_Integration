using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using DM7_PPLUS_Integration;
using DM7_PPLUS_Integration.Auswahllisten;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.V2;

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

            var testServer = Test_Server.Instanz()
                .Mit_Mitarbeitern(test_Mitarbeiter)
                .Start("127.0.0.1", 2000, new ConsoleLogger(), out _);

            using (var api = DM7_PPLUS_Integration.Implementierung.V2.PPLUS.Connect(testServer.ConnectionString).Result)
            {
                Console.WriteLine("### Initiale Mitarbeiter");
                Mitarbeiter_anzeigen(api);

                testServer.Mitarbeiter_hinzufügen(Willenborg());
                Console.WriteLine("\n### Mitarbeiter nach Hinzufügen");
                Mitarbeiter_anzeigen(api);
            }

            Thread.Sleep(2000);
            Console.ReadKey();
            testServer.Dispose();
        }

        private static void Mitarbeiter_anzeigen(DM7_PPLUS_API api)
        {
            var alle_Mitarbeiter = api.Mitarbeiter_abrufen().Result;
            foreach (var mitarbeiter in alle_Mitarbeiter)
            {
                var mandantenzugehörigkeit = mitarbeiter.DM7_Mandantenzugehörigkeiten.First();
                Console.WriteLine($" - {mitarbeiter.Nachname}, {mitarbeiter.Vorname} - Zugehörig ab {Datum_als_Text(mandantenzugehörigkeit.GueltigAb)} bis {Datum_als_Text(mandantenzugehörigkeit.GueltigBis)}");
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
