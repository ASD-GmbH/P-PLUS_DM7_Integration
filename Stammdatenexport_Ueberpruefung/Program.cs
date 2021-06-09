#nullable enable
using System;
using System.Collections.Generic;
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

    internal interface Geplante_Touren {}
    internal readonly struct Geplante_Touren_mit_Mitarbeiter : Geplante_Touren
    {
        public readonly Mitarbeiter Mitarbeiter;
        public readonly List<Dienstbuchung> Dienstbuchungen;

        public Geplante_Touren_mit_Mitarbeiter(Mitarbeiter mitarbeiter, List<Dienstbuchung> dienstbuchungen)
        {
            Mitarbeiter = mitarbeiter;
            Dienstbuchungen = dienstbuchungen;
        }
    }
    internal readonly struct Geplante_Touren_ohne_exportierten_Mitarbeiter : Geplante_Touren
    {
        public readonly Guid Mitarbeiter;
        public readonly List<Dienstbuchung> Dienstbuchungen;

        public Geplante_Touren_ohne_exportierten_Mitarbeiter(Guid mitarbeiter, List<Dienstbuchung> dienstbuchungen)
        {
            Mitarbeiter = mitarbeiter;
            Dienstbuchungen = dienstbuchungen;
        }
    }

    internal interface Abwesenheiten {}

    internal readonly struct Abwesenheiten_mit_Mitarbeiter : Abwesenheiten
    {
        public readonly Mitarbeiter Mitarbeiter;
        public readonly List<Abwesenheit> Abwesenheiten;

        public Abwesenheiten_mit_Mitarbeiter(Mitarbeiter mitarbeiter, List<Abwesenheit> abwesenheiten)
        {
            Mitarbeiter = mitarbeiter;
            Abwesenheiten = abwesenheiten;
        }
    }
    internal readonly struct Abwesenheiten_ohne_exportierten_Mitarbeiter : Abwesenheiten
    {
        public readonly Guid Mitarbeiter;
        public readonly List<Abwesenheit> Abwesenheiten;

        public Abwesenheiten_ohne_exportierten_Mitarbeiter(Guid mitarbeiter, List<Abwesenheit> abwesenheiten)
        {
            Mitarbeiter = mitarbeiter;
            Abwesenheiten = abwesenheiten;
        }
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
            var __ when __ == Auswahllisten_1.Titel.Dr => "Dr.",
            var __ when __ == Auswahllisten_1.Titel.Prof => "Prof.",
            var __ when __ == Auswahllisten_1.Titel.Prof_Dr => "Prof. Dr.",
            var __ when __ == Auswahllisten_1.Titel.Kein => "",
            _ => "Unbekannter Titel"
        };

        private static string Geschlecht(Guid geschlecht) => geschlecht switch
        {
            var __ when __ == Auswahllisten_1.Geschlecht.Maennlich => "Männlich",
            var __ when __ == Auswahllisten_1.Geschlecht.Weiblich => "Weiblich",
            var __ when __ == Auswahllisten_1.Geschlecht.Divers => "Divers",
            var __ when __ == Auswahllisten_1.Geschlecht.NichtAngegeben => "Nicht angegeben",
            _ => "Unbekannt"
        };

        private static string Familienstand(Guid stand) => stand switch
        {
            var __ when __ == Auswahllisten_1.Familienstand.Entpartnert => "Entpartnert",
            var __ when __ == Auswahllisten_1.Familienstand.Geschieden => "Geschieden",
            var __ when __ == Auswahllisten_1.Familienstand.Getrennt_lebend => "Getrennt lebend",
            var __ when __ == Auswahllisten_1.Familienstand.Ledig => "Ledig",
            var __ when __ == Auswahllisten_1.Familienstand.Partnerhinterbliebend => "Partnerhinterbliebend",
            var __ when __ == Auswahllisten_1.Familienstand.Verheiratet => "Verheiratet",
            var __ when __ == Auswahllisten_1.Familienstand.Verpartnert => "Verpartnert",
            var __ when __ == Auswahllisten_1.Familienstand.Verwitwet => "Verwitwet",
            _ => "Unbekannt"
        };

        private static string Konfession(Guid konfession) => konfession switch
        {
            var __ when __ == Auswahllisten_1.Konfession.Keine => "Keine",
            var __ when __ == Auswahllisten_1.Konfession.altkatholische_Kirchensteuer => "altkatholische Kirchensteuer",
            var __ when __ == Auswahllisten_1.Konfession.evangelische_Kirchensteuer => "evangelische Kirchensteuer",
            var __ when __ == Auswahllisten_1.Konfession.freie_Religionsgemeinschaft_Alzey => "freie Religionsgemeinschaft Alzey",
            var __ when __ == Auswahllisten_1.Konfession.Kirchensteuer_der_Freireligioesen_Landesgemeinde_Baden => "Kirchensteuer der Freireligiösen Landesgemeinde Baden",
            var __ when __ == Auswahllisten_1.Konfession.freireligioese_Landesgemeinde_Pfalz => "freireligiöse Landesgemeinde Pfalz",
            var __ when __ == Auswahllisten_1.Konfession.freireligioese_Gemeinde_Mainz => "freireligiöse Gemeinde Mainz",
            var __ when __ == Auswahllisten_1.Konfession.franzoesisch_reformiert => "französisch reformiert",
            var __ when __ == Auswahllisten_1.Konfession.freireligioese_Gemeinde_Offenbach_Mainz => "freireligiöse Gemeinde Offenbach Mainz",
            var __ when __ == Auswahllisten_1.Konfession.griechisch_orthodox => "griechisch orthodox",
            var __ when __ == Auswahllisten_1.Konfession.Kirchensteuer_der_Israelitischen_Religionsgemeinschaft_Baden => "Kirchensteuer der Isrälitischen Religionsgemeinschaft Baden",
            var __ when __ == Auswahllisten_1.Konfession.israelitische_Kultussteuer_der_Kultusberechtigten_Gemeinden => "isrälitische Kultussteuer der Kultusberechtigten Gemeinden",
            var __ when __ == Auswahllisten_1.Konfession.israelitische_Kultussteuer => "isrälitische Kultussteuer",
            var __ when __ == Auswahllisten_1.Konfession.Kirchensteuer_der_Israelitischen_Religionsgemeinschaft_Wuerttemberg => "Kirchensteuer der Isrälitischen Religionsgemeinschaft Württemberg",
            var __ when __ == Auswahllisten_1.Konfession.juedische_Kultussteuer => "jüdische Kultussteuer",
            var __ when __ == Auswahllisten_1.Konfession.evangelisch_lutherisch => "evangelisch lutherisch",
            var __ when __ == Auswahllisten_1.Konfession.muslimisch => "muslimisch",
            var __ when __ == Auswahllisten_1.Konfession.evangelisch_reformiert => "evangelisch reformiert",
            var __ when __ == Auswahllisten_1.Konfession.roemisch_katholische_Kirchensteuer => "römisch katholische Kirchensteuer",
            var __ when __ == Auswahllisten_1.Konfession.russisch_orthodox => "russisch orthodox",
            var __ when __ == Auswahllisten_1.Konfession.unitarische_Religionsgemeinschaft_freie_Protestanten => "unitarische Religionsgemeinschaft freie Protestanten",
            _ => "Unbekannt"
        };

        private static string Kontaktform(Guid form) => form switch
        {
            var __ when __ == Auswahllisten_1.Kontaktform.Privat => "Privat",
            var __ when __ == Auswahllisten_1.Kontaktform.Geschaeftlich => "Geschäftlich",
            var __ when __ == Auswahllisten_1.Kontaktform.Bereitschaft => "Bereitschaft",
            _ => ""
        };

        private static string Kontaktart(Guid kontaktart) => kontaktart switch
        {
            var __ when __ == Auswahllisten_1.Kontaktart.Email => "E-Mail",
            var __ when __ == Auswahllisten_1.Kontaktart.Fax => "Fax",
            var __ when __ == Auswahllisten_1.Kontaktart.Funkruf => "Funkruf",
            var __ when __ == Auswahllisten_1.Kontaktart.Telefon => "Telefon",
            var __ when __ == Auswahllisten_1.Kontaktart.Web => "Web",
            var __ when __ == Auswahllisten_1.Kontaktart.Zusaetzlich => "Zusätzlich",
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

            var alle_dienstbuchungen = api.Dienstbuchungen_zum_Stichtag(DM7_PPLUS_Integration.Daten.Datum.DD_MM_YYYY(stichtag.Day, stichtag.Month, stichtag.Year), gewünschter_Mandant).Result;
            if (alle_dienstbuchungen.Count == 0)
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

            var geplante_Touren =
                alle_dienstbuchungen
                    .GroupBy(_ => _.MitarbeiterId)
                    .Select(_ => mitarbeiterLookup.ContainsKey(_.Key)
                        ? (Geplante_Touren) new Geplante_Touren_mit_Mitarbeiter(mitarbeiterLookup[_.Key], _.ToList())
                        : new Geplante_Touren_ohne_exportierten_Mitarbeiter(_.Key, _.ToList()))
                    .ToList();

            string Dienstbezeichnung(int id)
            {
                return diensteLookup.ContainsKey(id) ? diensteLookup[id].Bezeichnung : "[Nicht exportierter Dienst]";
            }

            string Dienstbuchungsinfo(string mitarbeiter, IEnumerable<Dienstbuchung> dienstbuchungen)
            {
                var sb = new StringBuilder();
                sb.AppendLine(mitarbeiter);
                sb.AppendJoin(
                    Environment.NewLine,
                    dienstbuchungen
                        .OrderBy(_ => _.Beginnt_um.Stunden * 60 + _.Beginnt_um.Minuten)
                        .Select(dienstbuchung => $"\t{dienstbuchung.Beginnt_um.Stunden:00}:{dienstbuchung.Beginnt_um.Minuten:00} Uhr geplant für '{Dienstbezeichnung(dienstbuchung.DienstId)}'"));
                return sb.ToString();
            }

            Console.WriteLine($"Übertragene Dienstbuchungen {alle_dienstbuchungen.Count} zum {stichtag.ToLongDateString()}");
            Console.WriteLine(
                string.Join("\n-----------\n",
                    geplante_Touren
                        .OfType<Geplante_Touren_mit_Mitarbeiter>()
                        .OrderBy(_ => _.Mitarbeiter.Nachname)
                        .Select(dienstbuchungen_pro_Mitarbeiter => Dienstbuchungsinfo($"{dienstbuchungen_pro_Mitarbeiter.Mitarbeiter.Nachname}, {dienstbuchungen_pro_Mitarbeiter.Mitarbeiter.Vorname}", dienstbuchungen_pro_Mitarbeiter.Dienstbuchungen))
                        .Concat(
                            geplante_Touren
                                .OfType<Geplante_Touren_ohne_exportierten_Mitarbeiter>()
                                .Select(_ => Dienstbuchungsinfo($"Nicht exportierter Mitarbeiter {_.Mitarbeiter}", _.Dienstbuchungen)))));
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

            var alle_abwesenheiten = api.Abwesenheiten_zum_Stichtag(DM7_PPLUS_Integration.Daten.Datum.DD_MM_YYYY(stichtag.Day, stichtag.Month, stichtag.Year), gewünschter_Mandant).Result;
            if (alle_abwesenheiten.Count == 0)
            {
                Console.WriteLine($"Keine Abwesenheiten zum Stichtag {stichtag.ToLongDateString()} übertragen");
                return;
            }

            var mitarbeiterLookup =
                api.Mitarbeiter_abrufen()
                    .Result
                    .ToDictionary(_ => _.Id);

            static string Abwesenheiteninfo(string mitarbeiter, IEnumerable<Abwesenheit> abwesenheiten)
            {
                var sb = new StringBuilder();
                sb.AppendLine(mitarbeiter);
                sb.AppendJoin(
                    Environment.NewLine,
                    abwesenheiten.Select(abwesenheit => $"\t{abwesenheit.Art}: {abwesenheit.Grund}\n\tAb {Zeitpunkt_als_Text(abwesenheit.Abwesend_ab)}\n\tBis {Zeitpunkt_als_Text(abwesenheit.Vorraussichtlich_wieder_verfügbar_ab)}"));
                return sb.ToString();
            }

            var abwesenheiten_pro_Mitarbeiter =
                alle_abwesenheiten
                    .GroupBy(_ => _.MitarbeiterId)
                    .Select(_ =>
                        mitarbeiterLookup.ContainsKey(_.Key)
                            ? (Abwesenheiten) new Abwesenheiten_mit_Mitarbeiter(mitarbeiterLookup[_.Key], _.ToList())
                            : new Abwesenheiten_ohne_exportierten_Mitarbeiter(_.Key, _.ToList()))
                    .ToList();

            Console.WriteLine($"Übertragene Abwesenheiten {alle_abwesenheiten.Count} zum {stichtag.ToLongDateString()}");
            Console.WriteLine(
                string.Join("\n-----------\n",
                    abwesenheiten_pro_Mitarbeiter
                        .OfType<Abwesenheiten_mit_Mitarbeiter>()
                        .OrderBy(_ => _.Mitarbeiter.Nachname)
                        .Select(_ =>
                            Abwesenheiteninfo($"{_.Mitarbeiter.Nachname}, {_.Mitarbeiter.Vorname}", _.Abwesenheiten))
                        .Concat(
                            abwesenheiten_pro_Mitarbeiter
                                .OfType<Abwesenheiten_ohne_exportierten_Mitarbeiter>()
                                .Select(_ => Abwesenheiteninfo($"Nicht exportierter Mitarbeiter {_.Mitarbeiter}",
                                    _.Abwesenheiten))
                        )));
        }

        private static string Zeitpunkt_als_Text(Zeitpunkt zeitpunkt) =>
            $"{zeitpunkt.Datum.Tag:00}.{zeitpunkt.Datum.Monat:00}.{zeitpunkt.Datum.Jahr:0000} {zeitpunkt.Uhrzeit.Stunden:00}:{zeitpunkt.Uhrzeit.Minuten:00}";
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
