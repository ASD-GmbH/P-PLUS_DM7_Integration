#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DM7_PPLUS_Integration;
using DM7_PPLUS_Integration.Auswahllisten;
using DM7_PPLUS_Integration.Daten;

namespace Stammdatenexport_Überprüfung
{
    internal enum Auswahl
    {
        Mitarbeiter = 1,
        Mitarbeiterdetails,
        Dienste,
        Dienstdetails,
        Dienstbuchungen,
        Abwesenheiten,
        Debug_umschalten,
        Quit,
        Unbekannt
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Falsche Parameter!");
                Console.WriteLine("Verwendung: Stammdatenexport_Überprüfung.exe \"tcp://127.0.0.1:21000|8c8MRi8ohcS7IPEbu2ZH4AzRsZHEGlWpX0tJocRLhM4=\" user password");
                return;
            }

            var adressparts = args[0].Split("|");
            if (adressparts.Length != 2)
            {
                Console.WriteLine("Verbindungsstring unvollständig!");
                return;
            }
            var address = adressparts[0];
            var encryptionKey = adressparts[1];
            var user = args[1];
            var password = args[2];
            
            var debug_aktiv = false;

            while (true)
            {
                Console.Clear();
                var auswahl = Auswahlmenu(debug_aktiv);
                Console.Clear();
                var api = PPLUS.Connect(address, user, password, encryptionKey, new Logger(debug_aktiv)).Result;
                switch (auswahl)
                {
                    case Auswahl.Mitarbeiter:
                        Mitarbeiter_abfragen(api);
                        Console.ReadKey();
                        break;
                    case Auswahl.Mitarbeiterdetails:
                        Mitarbeiterdetails_abfragen(api);
                        Console.ReadKey();
                        break;
                    case Auswahl.Dienste:
                        Dienste_abfragen(api);
                        Console.ReadKey();
                        break;
                    case Auswahl.Dienstdetails:
                        Dienstdetails_abfragen(api);
                        Console.ReadKey();
                        break;
                    case Auswahl.Dienstbuchungen:
                        Dienstbuchungen_abfragen(api);
                        Console.ReadKey();
                        break;
                    case Auswahl.Abwesenheiten:
                        Abwesenheiten_abfragen(api);
                        Console.ReadKey();
                        break;
                    case Auswahl.Debug_umschalten:
                        debug_aktiv = !debug_aktiv;
                        break;
                    case Auswahl.Quit:
                        return;
                    case Auswahl.Unbekannt:
                        break;
                }
            }
        }

        private static Auswahl Auswahlmenu(bool debug_aktiv)
        {
            Console.WriteLine("Verfügbare Abfragen");
            foreach (var auswahl in new []
            {
                Auswahl.Mitarbeiter, 
                Auswahl.Mitarbeiterdetails, 
                Auswahl.Dienste, 
                Auswahl.Dienstdetails, 
                Auswahl.Dienstbuchungen,
                Auswahl.Abwesenheiten,
                Auswahl.Debug_umschalten,
                Auswahl.Quit
            })
            {
                Console.WriteLine(auswahl == Auswahl.Debug_umschalten
                    ? $"\t{(int) auswahl}. Debug {(debug_aktiv ? "Ausschalten" : "Einschalten")}"
                    : $"\t{(int) auswahl}. {auswahl}");
            }

            try
            {
                var key = Console.ReadLine() ?? "";
                return (Auswahl) int.Parse(key);
            }
            catch (Exception)
            {
                return Auswahl.Unbekannt;
            }
        }

        private static void Mitarbeiter_abfragen(DM7_PPLUS_API api)
        {
            var mitarbeiter = api.Mitarbeiter_abrufen().Result;
            if (mitarbeiter.Count == 0)
            {
                Console.WriteLine("Keine Mitarbeiter übertragen");
                return;
            }

            Console.WriteLine($"Übertragene Mitarbeiter {mitarbeiter.Count}. Stand zum {new DateTime((long)mitarbeiter.Stand.Value).ToLongDateString()}");
            var ausgabe =
                string.Join("\n",
                    mitarbeiter
                        .OrderBy(_ => _.Nachname)
                        .Select(ma => $"{ma.Id} | {ma.Nachname}, {ma.Vorname}"));
            Console.WriteLine(ausgabe);
        }

        private static void Mitarbeiterdetails_abfragen(DM7_PPLUS_API api)
        {
            Console.WriteLine("Id des gewünschten Mitarbeiters eingeben: ");
            Guid gewünschter_Mitarbeiter;
            try
            {
                gewünschter_Mitarbeiter = Guid.Parse(Console.ReadLine() ?? "");
                Console.Clear();
            }
            catch (Exception)
            {
                Console.Clear();
                Console.WriteLine("Keine gültige Mitarbeiter ID. Bitte versuchen Sie es erneut");
                Console.ReadKey();
                return;
            }

            var mitarbeiter = api.Mitarbeiter_abrufen().Result.Where(_ => _.Id == gewünschter_Mitarbeiter).ToList();
            if (mitarbeiter.Count == 0)
            {
                Console.WriteLine($"Ein Mitarbeiter der ID {gewünschter_Mitarbeiter} wurde im Abfrageergebnis nicht gefunden");
                return;
            }

            var ma = mitarbeiter.First();
            Console.WriteLine($"{ma.Nachname}, {ma.Vorname}{Titel(ma.Titel)}");
            if (ma.Geburtstag.HasValue) Console.WriteLine($"Geboren am {Datum(ma.Geburtstag.Value)}");
            Console.WriteLine($"Familienstand: {Familienstand(ma.Familienstand)}");
            Console.WriteLine($"Geschlecht: {Geschlecht(ma.Geschlecht)}");
            Console.WriteLine($"Konfession: {Konfession(ma.Konfession)}");
            if (ma.Postanschrift.HasValue) Console.WriteLine($"Postanschrift: {ma.Postanschrift.Value.Strasse}, {ma.Postanschrift.Value.Postleitzahl} {ma.Postanschrift.Value.Ort} {ma.Postanschrift.Value.Land}");
            if (ma.Kontakte.Count > 0)
            {
                Console.WriteLine("Erreichbar unter");
                foreach (var kontakt in ma.Kontakte) Console.WriteLine($"\t{Kontaktform(kontakt.Kontaktform)} {Kontaktart(kontakt.Kontaktart)}: {kontakt.Eintrag}");
            }
            Console.WriteLine($"Personalnummer: {ma.Personalnummer}, Handzeichen: {ma.Handzeichen}");
            if (ma.Qualifikationen.Count > 0)
            {
                Console.WriteLine("Qualifiziert für");
                foreach (var qualifikation in ma.Qualifikationen)
                {
                    Console.WriteLine($"\tStufe {qualifikation.Stufe}: {qualifikation.Bezeichnung}");
                    Console.WriteLine($"\t\tSeit {Datum(qualifikation.Gültig_ab)}{(qualifikation.Gültig_bis.HasValue ? $" - Bis {Datum(qualifikation.Gültig_bis.Value)}" : "")}");
                }
            }
            Console.WriteLine("Mandantenzugehörigkeiten");
            Console.WriteLine(Mandantenzugehörigkeiten(ma.DM7_Mandantenzugehörigkeiten));
        }

        private static string Datum(Datum datum) => $"{datum.Tag:00}.{datum.Monat:00}.{datum.Jahr:0000}";

        private static string Titel(Guid titel) => titel switch
        {
            { } __ when __ == Auswahllisten_1.Titel.Dr => "Dr.",
            { } __ when __ == Auswahllisten_1.Titel.Prof => "Prof.",
            { } __ when __ == Auswahllisten_1.Titel.Prof_Dr => "Prof. Dr.",
            { } __ when __ == Auswahllisten_1.Titel.Kein => "",
            _ => "Unbekannter Titel"
        };

        private static string Geschlecht(Guid geschlecht) => geschlecht switch
        {
            { } __ when __ == Auswahllisten_1.Geschlecht.Maennlich => "Männlich",
            { } __ when __ == Auswahllisten_1.Geschlecht.Weiblich => "Weiblich",
            { } __ when __ == Auswahllisten_1.Geschlecht.Divers => "Divers",
            { } __ when __ == Auswahllisten_1.Geschlecht.NichtAngegeben => "Nicht angegeben",
            _ => "Unbekannt"
        };

        private static string Familienstand(Guid stand) => stand switch
        {
            { } __ when __ == Auswahllisten_1.Familienstand.Entpartnert => "Entpartnert",
            { } __ when __ == Auswahllisten_1.Familienstand.Geschieden => "Geschieden",
            { } __ when __ == Auswahllisten_1.Familienstand.Getrennt_lebend => "Getrennt lebend",
            { } __ when __ == Auswahllisten_1.Familienstand.Ledig => "Ledig",
            { } __ when __ == Auswahllisten_1.Familienstand.Partnerhinterbliebend => "Partnerhinterbliebend",
            { } __ when __ == Auswahllisten_1.Familienstand.Verheiratet => "Verheiratet",
            { } __ when __ == Auswahllisten_1.Familienstand.Verpartnert => "Verpartnert",
            { } __ when __ == Auswahllisten_1.Familienstand.Verwitwet => "Verwitwet",
            _ => "Unbekannt"
        };

        private static string Konfession(Guid konfession) => konfession switch
        {
            { } __ when __ == Auswahllisten_1.Konfession.Keine => "Keine",
            { } __ when __ == Auswahllisten_1.Konfession.altkatholische_Kirchensteuer => "altkatholische Kirchensteuer",
            { } __ when __ == Auswahllisten_1.Konfession.evangelische_Kirchensteuer => "evangelische Kirchensteuer",
            { } __ when __ == Auswahllisten_1.Konfession.freie_Religionsgemeinschaft_Alzey => "freie Religionsgemeinschaft Alzey",
            { } __ when __ == Auswahllisten_1.Konfession.Kirchensteuer_der_Freireligioesen_Landesgemeinde_Baden => "Kirchensteuer der Freireligiösen Landesgemeinde Baden",
            { } __ when __ == Auswahllisten_1.Konfession.freireligioese_Landesgemeinde_Pfalz => "freireligiöse Landesgemeinde Pfalz",
            { } __ when __ == Auswahllisten_1.Konfession.freireligioese_Gemeinde_Mainz => "freireligiöse Gemeinde Mainz",
            { } __ when __ == Auswahllisten_1.Konfession.franzoesisch_reformiert => "französisch reformiert",
            { } __ when __ == Auswahllisten_1.Konfession.freireligioese_Gemeinde_Offenbach_Mainz => "freireligiöse Gemeinde Offenbach Mainz",
            { } __ when __ == Auswahllisten_1.Konfession.griechisch_orthodox => "griechisch orthodox",
            { } __ when __ == Auswahllisten_1.Konfession.Kirchensteuer_der_Israelitischen_Religionsgemeinschaft_Baden => "Kirchensteuer der Isrälitischen Religionsgemeinschaft Baden",
            { } __ when __ == Auswahllisten_1.Konfession.israelitische_Kultussteuer_der_Kultusberechtigten_Gemeinden => "isrälitische Kultussteuer der Kultusberechtigten Gemeinden",
            { } __ when __ == Auswahllisten_1.Konfession.israelitische_Kultussteuer => "isrälitische Kultussteuer",
            { } __ when __ == Auswahllisten_1.Konfession.Kirchensteuer_der_Israelitischen_Religionsgemeinschaft_Wuerttemberg => "Kirchensteuer der Isrälitischen Religionsgemeinschaft Württemberg",
            { } __ when __ == Auswahllisten_1.Konfession.juedische_Kultussteuer => "jüdische Kultussteuer",
            { } __ when __ == Auswahllisten_1.Konfession.evangelisch_lutherisch => "evangelisch lutherisch",
            { } __ when __ == Auswahllisten_1.Konfession.muslimisch => "muslimisch",
            { } __ when __ == Auswahllisten_1.Konfession.evangelisch_reformiert => "evangelisch reformiert",
            { } __ when __ == Auswahllisten_1.Konfession.roemisch_katholische_Kirchensteuer => "römisch katholische Kirchensteuer",
            { } __ when __ == Auswahllisten_1.Konfession.russisch_orthodox => "russisch orthodox",
            { } __ when __ == Auswahllisten_1.Konfession.unitarische_Religionsgemeinschaft_freie_Protestanten => "unitarische Religionsgemeinschaft freie Protestanten",
            _ => "Unbekannt"
        };

        private static string Kontaktform(Guid form) => form switch
        {
            { } __ when __ == Auswahllisten_1.Kontaktform.Privat => "Privat",
            { } __ when __ == Auswahllisten_1.Kontaktform.Geschaeftlich => "Geschäftlich",
            { } __ when __ == Auswahllisten_1.Kontaktform.Bereitschaft => "Bereitschaft",
            _ => ""
        };

        private static string Kontaktart(Guid kontaktart) => kontaktart switch
        {
            { } __ when __ == Auswahllisten_1.Kontaktart.Email => "E-Mail",
            { } __ when __ == Auswahllisten_1.Kontaktart.Fax => "Fax",
            { } __ when __ == Auswahllisten_1.Kontaktart.Funkruf => "Funkruf",
            { } __ when __ == Auswahllisten_1.Kontaktart.Telefon => "Telefon",
            { } __ when __ == Auswahllisten_1.Kontaktart.Web => "Web",
            { } __ when __ == Auswahllisten_1.Kontaktart.Zusaetzlich => "Zusätzlich",
            _ => ""
        };

        private static string Mandantenzugehörigkeiten(IEnumerable<DM7_Mandantenzugehörigkeit> zugehörigkeiten) =>
            string.Join("\n-------\n",
                zugehörigkeiten
                    .GroupBy(_ => _.MandantId)
                    .Select(zuordnungen =>
                        {
                            var auflistung =
                                string.Join(Environment.NewLine,
                                    zuordnungen
                                        .OrderBy(_ => new DateTime(_.GueltigAb.Jahr, _.GueltigAb.Monat, _.GueltigAb.Tag))
                                        .Select(_ => $"\t\tSeit {Datum(_.GueltigAb)}{(_.GueltigBis.HasValue ? $" bis {Datum(_.GueltigBis.Value)}" : "")}"));
                            return $"\tMandant {zuordnungen.Key}\n{auflistung}";
                        }));
            

        private static void Dienste_abfragen(DM7_PPLUS_API api)
        {
            var dienste = api.Dienste_abrufen().Result;
            if (dienste.Count == 0)
            {
                Console.WriteLine("Keine Dienste übertragen");
                return;
            }

            Console.WriteLine($"Übertragene Dienste {dienste.Count}. Stand zum {new DateTime((long) dienste.Stand.Value).ToLongDateString()}");
            Console.WriteLine(string.Join("\n-----------\n",
                dienste
                    .OrderBy(_ => _.Bezeichnung)
                    .Select(dienst => $"{dienst.Id:000} | {dienst.Bezeichnung} ({dienst.Kurzbezeichnung}){(dienst.Gelöscht ? "GELÖSCHT" : "")}")));
        }

        private static void Dienstdetails_abfragen(DM7_PPLUS_API api)
        {
            Console.WriteLine("Id des gewünschten Dienstes eingeben: ");
            int gewünschter_Dienst;
            try
            {
                gewünschter_Dienst = int.Parse(Console.ReadLine() ?? "");
                Console.Clear();
            }
            catch (Exception)
            {
                Console.Clear();
                Console.WriteLine("Keine gültige Dienst ID. Bitte versuchen Sie es erneut");
                Console.ReadKey();
                return;
            }

            var dienste = api.Dienste_abrufen().Result.Where(_ => _.Id == gewünschter_Dienst).ToList();
            if (dienste.Count == 0)
            {
                Console.WriteLine($"Ein Dienst der ID {gewünschter_Dienst} wurde im Abfrageergebnis nicht gefunden");
                return;
            }

            var dienst = dienste.First();
            Console.WriteLine($"{dienst.Bezeichnung} ({dienst.Kurzbezeichnung}) {(dienst.Gelöscht ? "GELÖSCHT" : "")}");
            Console.WriteLine($"\t{Ausgabe_Option("Mo", dienst.Gültig_an.Montag)}, {Ausgabe_Option("Di", dienst.Gültig_an.Dienstag)}, {Ausgabe_Option("Mi", dienst.Gültig_an.Mittwoch)}, {Ausgabe_Option("Do", dienst.Gültig_an.Donnerstag)}, {Ausgabe_Option("Fr", dienst.Gültig_an.Freitag)}, {Ausgabe_Option("Sa", dienst.Gültig_an.Samstag)}, {Ausgabe_Option("So", dienst.Gültig_an.Sonntag)}, {Ausgabe_Option("Fei", dienst.Gültig_an.Feiertags)}");
            Console.WriteLine("Mandantenzugehörigkeiten");
            Console.WriteLine(Mandantenzugehörigkeiten(dienst.Mandantenzugehörigkeiten));
        }

        private static string Ausgabe_Option(string option, bool aktiv) => $"{option}: [{(aktiv ? "x" : " ")}]";

        private static void Dienstbuchungen_abfragen(DM7_PPLUS_API api)
        {
            Console.WriteLine("ID des Mandanten, für den die Dienstbuchungen abgefragt werden: ");
            Guid gewünschter_Mandant;
            try
            {
                gewünschter_Mandant = Guid.Parse(Console.ReadLine() ?? "");
                Console.Clear();
            }
            catch (Exception)
            {
                Console.Clear();
                Console.WriteLine("Keine gültige Mandant ID. Bitte versuchen Sie es erneut");
                Console.ReadKey();
                return;
            }

            var stichtag = DateTime.Now;
            Console.WriteLine($"Stichtag [{stichtag:dd.MM.yyyy}]: ");
            try
            {
                var text = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    stichtag = DateTime.Parse(text);
                }
                Console.Clear();
            }
            catch (Exception)
            {
                Console.Clear();
                Console.WriteLine("Keine gültiges Datum. Bitte versuchen Sie es erneut");
                Console.ReadKey();
                return;
            }

            var dienstbuchungen = api.Dienstbuchungen_zum_Stichtag(DM7_PPLUS_Integration.Daten.Datum.DD_MM_YYYY(stichtag.Day, stichtag.Month, stichtag.Year), gewünschter_Mandant).Result;
            if (dienstbuchungen.Count == 0)
            {
                Console.WriteLine($"Keine Dienstbuchungen zum Stichtag {stichtag.ToLongDateString()} übertragen");
                return;
            }

            var mitarbeiterLookup =
                api.Mitarbeiter_abrufen()
                    .Result
                    .ToDictionary(_ => _.Id);
            var diensteLookup =
                api.Dienste_abrufen()
                    .Result
                    .ToDictionary(_ => _.Id);

            Console.WriteLine($"Übertragene Dienstbuchungen {dienstbuchungen.Count} zum {stichtag.ToLongDateString()}");
            var ausgabe =
                string.Join("\n-----------\n",
                    dienstbuchungen
                        .GroupBy(_ => _.MitarbeiterId)
                        .Select(_ => new { mitarbeiter = mitarbeiterLookup[_.Key], dienstbuchungen = _.ToList() })
                        .OrderBy(_ => _.mitarbeiter.Nachname)
                        .Select(dienstbuchungen_pro_Mitarbeiter =>
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine($"{dienstbuchungen_pro_Mitarbeiter.mitarbeiter.Nachname}, {dienstbuchungen_pro_Mitarbeiter.mitarbeiter.Vorname}");
                            sb.AppendJoin(
                                Environment.NewLine,
                                dienstbuchungen_pro_Mitarbeiter.dienstbuchungen
                                    .OrderBy(_ => _.Beginnt_um.Stunden*60+_.Beginnt_um.Minuten)
                                    .Select(dienstbuchung => $"\t{dienstbuchung.Beginnt_um.Stunden:00}:{dienstbuchung.Beginnt_um.Minuten:00} Uhr geplant für '{diensteLookup[dienstbuchung.DienstId].Bezeichnung}'"));
                            return sb.ToString();
                        }));
            Console.WriteLine(ausgabe);
        }

        private static void Abwesenheiten_abfragen(DM7_PPLUS_API api)
        {
            Console.WriteLine("ID des Mandanten, für den die Abwesenheiten abgefragt werden: ");
            Guid gewünschter_Mandant;
            try
            {
                gewünschter_Mandant = Guid.Parse(Console.ReadLine() ?? "");
                Console.Clear();
            }
            catch (Exception)
            {
                Console.Clear();
                Console.WriteLine("Keine gültige Mandant ID. Bitte versuchen Sie es erneut");
                Console.ReadKey();
                return;
            }

            var stichtag = DateTime.Now;
            Console.WriteLine($"Stichtag [{stichtag:dd.MM.yyyy}]: ");
            try
            {
                var text = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    stichtag = DateTime.Parse(text);
                }
                Console.Clear();
            }
            catch (Exception)
            {
                Console.Clear();
                Console.WriteLine("Keine gültiges Datum. Bitte versuchen Sie es erneut");
                Console.ReadKey();
                return;
            }

            var abwesenheiten = api.Abwesenheiten_zum_Stichtag(DM7_PPLUS_Integration.Daten.Datum.DD_MM_YYYY(stichtag.Day, stichtag.Month, stichtag.Year), gewünschter_Mandant).Result;
            if (abwesenheiten.Count == 0)
            {
                Console.WriteLine($"Keine Abwesenheiten zum Stichtag {stichtag.ToLongDateString()} übertragen");
                return;
            }

            var mitarbeiterLookup =
                api.Mitarbeiter_abrufen()
                    .Result
                    .ToDictionary(_ => _.Id);

            Console.WriteLine($"Übertragene Abwesenheiten {abwesenheiten.Count} zum {stichtag.ToLongDateString()}");
            var ausgabe =
                string.Join("\n-----------\n",
                    abwesenheiten
                        .GroupBy(_ => _.MitarbeiterId)
                        .Select(_ => new { mitarbeiter = mitarbeiterLookup[_.Key], abwesenheiten = _.ToList() })
                        .OrderBy(_ => _.mitarbeiter.Nachname)
                        .Select(abwesenheiten_pro_Mitarbeiter =>
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine($"{abwesenheiten_pro_Mitarbeiter.mitarbeiter.Nachname}, {abwesenheiten_pro_Mitarbeiter.mitarbeiter.Vorname}");
                            sb.AppendJoin(
                                Environment.NewLine,
                                abwesenheiten_pro_Mitarbeiter.abwesenheiten
                                    .Select(abwesenheit => $"\t{abwesenheit.Art}: {abwesenheit.Grund}"));
                            return sb.ToString();
                        }));
            Console.WriteLine(ausgabe);
        }
    }

    internal class Logger : Log
    {
        private readonly bool debug_logging;

        public Logger(bool debugLogging)
        {
            debug_logging = debugLogging;
        }

        public void Info(string text)
        {
            Console.WriteLine("[INFO] " + text);
        }

        public void Debug(string text)
        {
            if (debug_logging) Console.WriteLine("[DEBUG] " + text);
        }
    }
}
