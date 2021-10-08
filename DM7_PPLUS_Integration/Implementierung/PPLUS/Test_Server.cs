using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Messages.PPLUS;
using Datenstand = DM7_PPLUS_Integration.Daten.Datenstand;
using Datum = DM7_PPLUS_Integration.Daten.Datum;
using Uhrzeit = DM7_PPLUS_Integration.Daten.Uhrzeit;

namespace DM7_PPLUS_Integration.Implementierung.PPLUS
{
    public class Test_Server
    {
        public static Test_Server Instanz()
        {
            return new Test_Server();
        }

        private readonly List<Dienst> _dienste = new List<Dienst>();
        private readonly List<Mitarbeiter> _mitarbeiter = new List<Mitarbeiter>();
        private readonly Dictionary<(Guid mandant, Datum tag), List<Dienstbuchung>> _dienstbuchungen = new Dictionary<(Guid, Datum), List<Dienstbuchung>>();
        private readonly Dictionary<Guid, List<Abwesenheit>> _abwesenheiten = new Dictionary<Guid, List<Abwesenheit>>();
        private string _user = "anonymous";
        private string _password = "password";

        private Test_Server() { }

        public Test_Server Mit_Diensten(params Dienst[] dienste)
        {
            _dienste.Clear();
            _dienste.AddRange(dienste);
            return this;
        }

        public Test_Server Mit_Mitarbeitern(params Mitarbeiter[] mitarbeiter)
        {
            _mitarbeiter.Clear();
            _mitarbeiter.AddRange(mitarbeiter);
            return this;
        }

        public Test_Server Mit_Dienstbuchungen_für(Guid mandant, Datum tag, params Dienstbuchung[] dienstbuchungen)
        {
            var key = (mandant, tag);
            if (_dienstbuchungen.ContainsKey(key))
            {
                _dienstbuchungen[key].Clear();
                _dienstbuchungen[key].AddRange(dienstbuchungen);
            }
            else
            {
                _dienstbuchungen.Add(key, dienstbuchungen.ToList());
            }

            return this;
        }

        public Test_Server Mit_Abwesenheiten_für(Guid mandant, params Abwesenheit[] abwesenheiten)
        {
            if (_abwesenheiten.ContainsKey(mandant))
            {
                _abwesenheiten[mandant].Clear();
                _abwesenheiten[mandant].AddRange(abwesenheiten);
            }
            else
            {
                _abwesenheiten.Add(mandant, abwesenheiten.ToList());
            }

            return this;
        }

        public Test_Server Mit_Authentification(string user, string password)
        {
            _user = user;
            _password = password;
            return this;
        }

        public Test_Host Start(string adresse, int port, out string encryptionKey)
        {
            var backend = new Test_PPLUS_Handler(_user, _password, _mitarbeiter, _dienste, _dienstbuchungen, _abwesenheiten);
            encryptionKey = Encryption.Generate_encoded_Key();
            return new Test_Host(backend, adresse, port, encryptionKey);
        }
    }

    internal readonly struct Daten_mit_Version<T>
    {
        public readonly T Data;
        public readonly ulong Version;

        public Daten_mit_Version(T data, ulong version)
        {
            Data = data;
            Version = version;
        }
    }

    internal class Test_PPLUS_Handler : PPLUS_Handler
    {
        private readonly string _user;
        private readonly string _password;
        private Token? _token;
        private readonly List<Daten_mit_Version<Mitarbeiter>> _mitarbeiter;
        private readonly List<Daten_mit_Version<Dienst>> _dienste;
        private readonly Dictionary<(Guid mandant, Datum tag), List<Dienstbuchung>> _dienstbuchungen;
        private readonly Dictionary<Guid, List<Abwesenheit>> _abwesenheiten;
        private readonly List<Dienstplanabschluss> _dienstplanabschlüsse = new List<Dienstplanabschluss>();
        private Soll_Ist_Abgleich? _letzter_Soll_Ist_Abgleich;

        public Test_PPLUS_Handler(
            string user,
            string password,
            IEnumerable<Mitarbeiter> mitarbeiter,
            IEnumerable<Dienst> dienste,
            Dictionary<(Guid mandant, Datum tag), List<Dienstbuchung>> dienstbuchungen,
            Dictionary<Guid, List<Abwesenheit>> abwesenheiten)
        {
            _user = user;
            _password = password;
            _mitarbeiter = mitarbeiter.Select(_ => new Daten_mit_Version<Mitarbeiter>(_, 1)).ToList();
            _dienste = dienste.Select(_ => new Daten_mit_Version<Dienst>(_, 1)).ToList();
            _dienstbuchungen = dienstbuchungen;
            _abwesenheiten = abwesenheiten;
        }

        public Task<Token?> Authenticate(string user, string password, TimeSpan? timeout = null)
        {
            return Task.Run(() =>
            {
                if (user != _user || _password != password) return null;

                _token = new Token(69);
                return _token;
            });
        }

        public Task<Capabilities> Capabilities(TimeSpan? timeout = null)
        {
            return Task.FromResult(new Capabilities(new[]
            {
                Capability.MITARBEITER_V1,
                Capability.DIENSTE_V1,
                Capability.BEGINN_VON_DIENST_V1,
                Capability.ABWESENHEITEN_V1,
                Capability.DIENSTBUCHUNGEN_V1,
                Capability.SOLL_IST_ABGLEICH_V1
            }.ToList()));
        }

        public Task<QueryResult> HandleQuery(Token token, Query query, TimeSpan? timeout = null)
        {
            return Task.Run<QueryResult>(() =>
            {
                if (_token.HasValue == false || _token.Value != token)
                {
                    return new IOFehler("Unberechtigter Zugriff");
                }

                switch (query)
                {
                    case MitarbeiterAbrufenV1 _:
                        return Message_mapper.Mitarbeiterstammdaten_als_Message(Mitarbeiter_abrufen());

                    case MitarbeiterAbrufenAbV1 q:
                        return Message_mapper.Mitarbeiterstammdaten_als_Message(Mitarbeiter_abrufen_ab(Message_mapper.Stand_aus(q.Value)));

                    case DiensteAbrufenV1 _:
                        return Message_mapper.Dienste_als_Message(Dienste_abrufen());

                    case DiensteAbrufenAbV1 q:
                        return Message_mapper.Dienste_als_Message(Dienste_abrufen_ab(Message_mapper.Stand_aus(q.Value)));

                    case DienstbeginnZumStichtagV1 _:
                        return Message_mapper.Dienstbeginn_als_Message(Uhrzeit.HH_MM(08, 30));

                    case DienstbuchungenImZeitraumV1 q:
                        return Message_mapper.Dienstbuchungen_als_Message(Dienstbuchungen_im_Zeitraum(Message_mapper.Guid_aus(q.Mandant), Message_mapper.Datum_aus(q.Von), Message_mapper.Datum_aus(q.Bis)));

                    case AbwesenheitenImZeitraumV1 q:
                        return Message_mapper.Abwesenheiten_als_Message(Abwesenheiten_im_Zeitraum(Message_mapper.Guid_aus(q.Mandant), Message_mapper.Datum_aus(q.Von), Message_mapper.Datum_aus(q.Bis)));

                    default:
                        return new IOFehler($"Query '{query.GetType()}' nicht behandelt");
                }
            });
        }

        public Task<CommandResult> HandleCommand(Token token, Command command, TimeSpan? timeout = null)
        {
            return Task.Run<CommandResult>(() =>
            {
                if (_token.HasValue == false || _token.Value != token)
                {
                    return new IOFehler("Unberechtigter Zugriff");
                }

                switch (command)
                {
                    case SollIstAbgleichFreigebenV1 c:
                    {
                        var abgleich = Message_mapper.Soll_Ist_Abgleich_aus(c.Value);
                        _letzter_Soll_Ist_Abgleich = abgleich;
                        var ergebnis = Verarbeite_Soll_Ist_Abgleich(abgleich);
                        return Message_mapper.Soll_Ist_Abgleich_Verarbeitungsergebnis_als_Message(ergebnis);
                    }

                    default:
                        return new IOFehler($"Command '{command.GetType()}' nicht behandelt");
                }
            });
        }

        public event Action Mitarbeiteränderungen_liegen_bereit;
        public event Action Dienständerungen_liegen_bereit;

        private Soll_Ist_Abgleich_Verarbeitungsergebnis Verarbeite_Soll_Ist_Abgleich(Soll_Ist_Abgleich abgleich)
        {
            if (_dienstplanabschlüsse.Any())
            {
                return new Dienstplanabschluss_verhindert_Verarbeitung(new ReadOnlyCollection<Dienstplanabschluss>(_dienstplanabschlüsse));
            }

            foreach (var nichtGefahreneTour in abgleich.Nicht_gefahrene_Touren)
            {
                var key = (nichtGefahreneTour.MandantId, abgleich.Datum);
                if (!_dienstbuchungen.ContainsKey(key)) continue;
                _dienstbuchungen[key].RemoveAll(db => db.MitarbeiterId == nichtGefahreneTour.MitarbeiterId && db.DienstId == nichtGefahreneTour.Dienst);
            }
            return new Verarbeitet();
        }

        public void Dienste_setzen(IEnumerable<Dienst> dienste)
        {
            var nächste_Version = Nächste_Version(_dienste);
            _dienste.Clear();
            _dienste.AddRange(dienste.Select(_ => new Daten_mit_Version<Dienst>(_, nächste_Version)));
            Dienständerungen_liegen_bereit?.Invoke();
        }

        public void Dienste_hinzufügen(IEnumerable<Dienst> dienste)
        {
            var nächste_Version = Nächste_Version(_dienste);
            _dienste.AddRange(dienste.Select(_ => new Daten_mit_Version<Dienst>(_, nächste_Version)));
            Dienständerungen_liegen_bereit?.Invoke();
        }

        public void Dienst_löschen(int dienst)
        {
            var nächste_Version = Nächste_Version(_dienste);
            var neue_Dienste =
                _dienste
                    .Select(_ => _.Data.Id == dienst ? Dienst_als_gelöscht(_.Data) : _.Data)
                    .ToList();
            _dienste.Clear();
            _dienste.AddRange(neue_Dienste.Select(_ => new Daten_mit_Version<Dienst>(_, nächste_Version)));
            Dienständerungen_liegen_bereit?.Invoke();
        }

        private static Dienst Dienst_als_gelöscht(Dienst dienst)
        {
            return new Dienst(
                dienst.Id,
                dienst.Mandantenzugehörigkeiten,
                dienst.Kurzbezeichnung,
                dienst.Bezeichnung,
                dienst.Gültig_an,
                true
            );
        }

        public void Mitarbeiter_setzen(IEnumerable<Mitarbeiter> mitarbeiter)
        {
            var nächste_Version = Nächste_Version(_mitarbeiter);
            _mitarbeiter.Clear();
            _mitarbeiter.AddRange(mitarbeiter.Select(_ => new Daten_mit_Version<Mitarbeiter>(_, nächste_Version)));
            Mitarbeiteränderungen_liegen_bereit?.Invoke();
        }

        public void Mitarbeiter_hinzufügen(IEnumerable<Mitarbeiter> mitarbeiter)
        {
            var nächste_Version = Nächste_Version(_mitarbeiter);
            _mitarbeiter.AddRange(mitarbeiter.Select(_ => new Daten_mit_Version<Mitarbeiter>(_, nächste_Version)));
            Mitarbeiteränderungen_liegen_bereit?.Invoke();
        }

        public void Mitarbeiter_austreten_zum(Datum austrittsdatum, Guid mitarbeiter)
        {
            var nächste_Version = Nächste_Version(_mitarbeiter);
            var neue_Mitarbeiter =
                _mitarbeiter
                    .Select(_ =>
                        _.Data.Id == mitarbeiter
                            ? new Daten_mit_Version<Mitarbeiter>(
                                Mit_Mandantenzugehörigkeit_bis_zum(austrittsdatum, _.Data), nächste_Version)
                            : _)
                    .ToList();

            _mitarbeiter.Clear();
            _mitarbeiter.AddRange(neue_Mitarbeiter);
            Mitarbeiteränderungen_liegen_bereit?.Invoke();
        }

        private static Mitarbeiter Mit_Mandantenzugehörigkeit_bis_zum(Datum austrittsdatum, Mitarbeiter mitarbeiter)
        {
            return new Mitarbeiter(
                mitarbeiter.Id,
                mitarbeiter.PersonenId,
                new ReadOnlyCollection<DM7_Mandantenzugehörigkeit>(
                    mitarbeiter.DM7_Mandantenzugehörigkeiten
                        .Select(_ => new DM7_Mandantenzugehörigkeit(_.MandantId, _.GueltigAb, austrittsdatum))
                        .ToList()
                ),
                mitarbeiter.Personalnummer,
                mitarbeiter.Titel,
                mitarbeiter.Vorname,
                mitarbeiter.Nachname,
                mitarbeiter.Postanschrift,
                mitarbeiter.Handzeichen,
                mitarbeiter.Geburtstag,
                mitarbeiter.Geschlecht,
                mitarbeiter.Konfession,
                mitarbeiter.Familienstand,
                mitarbeiter.Qualifikationen,
                mitarbeiter.Kontakte,
                mitarbeiter.PIN_für_mobile_Datenerfassung
            );
        }

        public Stammdaten<Mitarbeiter> Mitarbeiter_abrufen() =>
            new Stammdaten<Mitarbeiter>(_mitarbeiter.Select(_ => _.Data).ToList(), Höchster_Datenstand(_mitarbeiter));

        public Stammdaten<Mitarbeiter> Mitarbeiter_abrufen_ab(Datenstand stand)
        {
            var mitarbeiter =
                _mitarbeiter
                    .Where(_ => _.Version > stand.Value)
                    .Select(_ => _.Data)
                    .ToList();
            return new Stammdaten<Mitarbeiter>(mitarbeiter, Höchster_Datenstand(_mitarbeiter));
        }

        public Stammdaten<Dienst> Dienste_abrufen() =>
            new Stammdaten<Dienst>(_dienste.Select(_ => _.Data).ToList(), Höchster_Datenstand(_dienste));

        public Stammdaten<Dienst> Dienste_abrufen_ab(Datenstand stand)
        {
            var dienste =
                _dienste
                    .Where(_ => _.Version > stand.Value)
                    .Select(_ => _.Data)
                    .ToList();
            return new Stammdaten<Dienst>(dienste, Höchster_Datenstand(_dienste));
        }

        public List<(Guid MandantId, Datum datum, Dienstbuchung dienstbuchung)> Dienstbuchungen_abrufen()
        {
            return _dienstbuchungen
                .SelectMany(kv => kv.Value.Select(buchung => (kv.Key.mandant, kv.Key.tag, buchung)))
                .ToList();
        }

        public List<(Guid MandantId, Abwesenheit abwesenheit)> Abwesenheiten_abrufen()
        {
            return _abwesenheiten
                .SelectMany(kv => kv.Value.Select(abwesenheit => (kv.Key, abwesenheit)))
                .ToList();
        }

        private Dictionary<Datum, ReadOnlyCollection<Dienstbuchung>> Dienstbuchungen_im_Zeitraum(Guid mandant, Datum von, Datum bis)
        {
            var dienstbuchungen = new Dictionary<Datum, ReadOnlyCollection<Dienstbuchung>>();
            var b = new DateTime(bis.Jahr, bis.Monat, bis.Tag);
            for (var v = new DateTime(von.Jahr, von.Monat, von.Tag); v <= b; v = v.AddDays(1))
            {
                var key = (mandant, Datum.DD_MM_YYYY(v.Day, v.Month, v.Year));
                if (_dienstbuchungen.ContainsKey(key)) dienstbuchungen.Add(key.Item2, _dienstbuchungen[key].AsReadOnly());
            }

            return dienstbuchungen;
        }

        public void Dienstbuchungen_leeren()
        {
            _dienstbuchungen.Clear();
        }

        public void Dienstbuchungen_hinzufügen(Guid mandant, Datum tag, IEnumerable<Dienstbuchung> dienstbuchungen)
        {
            var key = (mandant, tag);
            if (_dienstbuchungen.ContainsKey(key)) _dienstbuchungen[key].AddRange(dienstbuchungen);
            else _dienstbuchungen.Add(key, dienstbuchungen.ToList());
        }

        public void Dienst_streichen(Guid mandant, Datum tag, int dienst)
        {
            var key = (mandant, tag);
            if (_dienstbuchungen.ContainsKey(key)) _dienstbuchungen[key].RemoveAll(buchung => buchung.DienstId == dienst);
        }

        private ReadOnlyCollection<Abwesenheit> Abwesenheiten_im_Zeitraum(Guid mandant, Datum von, Datum bis)
        {
            var zeitraumbeginn = new DateTime(von.Jahr, von.Monat, von.Tag);
            var zeitraumende = new DateTime(bis.Jahr, bis.Monat, bis.Tag);
            bool Gilt_im_Zeitraum(Abwesenheit abwesenheit)
            {
                var v = new DateTime(abwesenheit.Abwesend_ab.Datum.Jahr, abwesenheit.Abwesend_ab.Datum.Monat, abwesenheit.Abwesend_ab.Datum.Tag);

                if (abwesenheit.Vorraussichtlich_wieder_verfügbar_ab.HasValue)
                {
                    var b = new DateTime(abwesenheit.Vorraussichtlich_wieder_verfügbar_ab.Value.Datum.Jahr, abwesenheit.Vorraussichtlich_wieder_verfügbar_ab.Value.Datum.Monat, abwesenheit.Vorraussichtlich_wieder_verfügbar_ab.Value.Datum.Tag);
                    return (v >= zeitraumbeginn || v <= zeitraumende) && (b <= zeitraumende || b >= zeitraumbeginn);
                }

                return (v >= zeitraumbeginn || v <= zeitraumende);
            }

            var abwesenheiten =
                _abwesenheiten.ContainsKey(mandant)
                    ? _abwesenheiten[mandant].Where(Gilt_im_Zeitraum).ToList()
                    : new List<Abwesenheit>();
            return abwesenheiten.AsReadOnly();
        }

        public void Abwesenheiten_leeren()
        {
            _abwesenheiten.Clear();
        }

        public void Abwesenheiten_eintragen(Guid mandant, IEnumerable<Abwesenheit> abwesenheiten)
        {
            if (_abwesenheiten.ContainsKey(mandant)) _abwesenheiten[mandant].AddRange(abwesenheiten);
            else _abwesenheiten.Add(mandant, abwesenheiten.ToList());
        }

        public void Abwesenheiten_streichen_von(Guid mandant, Guid mitarbeiter)
        {
            if (_abwesenheiten.ContainsKey(mandant)) _abwesenheiten[mandant].RemoveAll(abwesenheit => abwesenheit.MitarbeiterId == mitarbeiter);
        }

        public void Dienstplanbschlüsse_zum_verhindern_der_Soll_Ist_Abgleich_Verarbeitung(IEnumerable<Dienstplanabschluss> abschlüsse)
        {
            _dienstplanabschlüsse.Clear();
            _dienstplanabschlüsse.AddRange(abschlüsse);
        }

        public void Soll_Ist_Abgleich_Verarbeitung_nicht_verhindern()
        {
            _dienstplanabschlüsse.Clear();
        }

        public Soll_Ist_Abgleich? Letzter_Soll_Ist_Abgleich() => _letzter_Soll_Ist_Abgleich;

        private static Datenstand Höchster_Datenstand<T>(List<Daten_mit_Version<T>> daten) =>
            daten.Any() ? new Datenstand(daten.Max(_ => _.Version)) : new Datenstand(0);

        private static ulong Nächste_Version<T>(List<Daten_mit_Version<T>> daten) =>
            daten.Any() ? daten.Max(_ => _.Version) + 1 : 1;

        public void Revoke_Token()
        {
            _token = null;
        }

        public void Dispose()
        {
        }
    }

    public class Test_Host : IDisposable
    {
        private readonly Adapter _host;
        private readonly Test_PPLUS_Handler _pplusHandler;
        private readonly string _adresse;
        private readonly int _port;

        internal Test_Host(Test_PPLUS_Handler pplusHandler, string adresse, int port, string encryptionKey)
        {
            _pplusHandler = pplusHandler;
            _adresse = adresse;
            _port = port;
            _host = new Adapter(adresse, port, _pplusHandler, encryptionKey);
        }

        public string ConnectionString => $"tcp://{_adresse}:{_port}";

        public void Revoke_Token()
        {
            _pplusHandler.Revoke_Token();
        }

        public void Mitarbeiter_setzen(params Mitarbeiter[] mitarbeiter)
        {
            _pplusHandler.Mitarbeiter_setzen(mitarbeiter);
        }

        public void Mitarbeiter_hinzufügen(params Mitarbeiter[] mitarbeiter)
        {
            _pplusHandler.Mitarbeiter_hinzufügen(mitarbeiter);
        }

        public void Mitarbeiter_austreten_zum(Datum austrittsdatum, Guid mitarbeiter)
        {
            _pplusHandler.Mitarbeiter_austreten_zum(austrittsdatum, mitarbeiter);
        }

        public void Dienst_löschen(int dienstId)
        {
            _pplusHandler.Dienst_löschen(dienstId);
        }

        public void Dienste_anlegen(params Dienst[] dienste)
        {
            _pplusHandler.Dienste_hinzufügen(dienste);
        }

        public void Dienste_setzen(params Dienst[] dienste)
        {
            _pplusHandler.Dienste_setzen(dienste);
        }

        public void Dienstbuchungen_setzen(Guid mandantId, Datum datum, params Dienstbuchung[] dienstbuchungen)
        {
            _pplusHandler.Dienstbuchungen_leeren();
            _pplusHandler.Dienstbuchungen_hinzufügen(mandantId, datum, dienstbuchungen);
        }

        public void Dienste_buchen(Guid mandantId, Datum datum, params Dienstbuchung[] dienstbuchungen)
        {
            _pplusHandler.Dienstbuchungen_hinzufügen(mandantId, datum, dienstbuchungen);
        }

        public void Dienst_streichen(Guid mandantId, Datum tag, int dienstId)
        {
            _pplusHandler.Dienst_streichen(mandantId, tag, dienstId);
        }

        public void Abwesenheiten_setzen(Guid mandantId, params Abwesenheit[] abwesenheiten)
        {
            _pplusHandler.Abwesenheiten_leeren();
            _pplusHandler.Abwesenheiten_eintragen(mandantId, abwesenheiten);
        }

        public void Abwesenheiten_eintragen(Guid mandantId, params Abwesenheit[] abwesenheiten)
        {
            _pplusHandler.Abwesenheiten_eintragen(mandantId, abwesenheiten);
        }

        public void Abwesenheiten_streichen_von(Guid mandantId, Guid mitarbeiter)
        {
            _pplusHandler.Abwesenheiten_streichen_von(mandantId, mitarbeiter);
        }

        public void Dienstplanbschlüsse_zum_verhindern_der_Soll_Ist_Abgleich_Verarbeitung(params Dienstplanabschluss[] abschlüsse)
        {
            _pplusHandler.Dienstplanbschlüsse_zum_verhindern_der_Soll_Ist_Abgleich_Verarbeitung(abschlüsse);
        }

        public void Soll_Ist_Abgleich_Verarbeitung_nicht_verhindern()
        {
            _pplusHandler.Soll_Ist_Abgleich_Verarbeitung_nicht_verhindern();
        }

        public List<Dienst> Dienste() => _pplusHandler.Dienste_abrufen().ToList();
        public List<Mitarbeiter> Mitarbeiter() => _pplusHandler.Mitarbeiter_abrufen().ToList();
        public List<(Guid MandantId, Datum datum, Dienstbuchung dienstbuchung)> Dienstbuchungen() => _pplusHandler.Dienstbuchungen_abrufen();
        public List<(Guid mandantId, Abwesenheit abwesenheit)> Abwesenheiten() => _pplusHandler.Abwesenheiten_abrufen();
        public Soll_Ist_Abgleich? Letzter_Soll_Ist_Abgleich() => _pplusHandler.Letzter_Soll_Ist_Abgleich();

        public void Dispose()
        {
            _host?.Dispose();
            _pplusHandler?.Dispose();
        }
    }
}