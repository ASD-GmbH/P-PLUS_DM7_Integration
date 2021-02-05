﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Shared;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    public class Test_Server
    {
        public static Test_Server Instanz()
        {
            return new Test_Server();
        }

        private readonly List<Dienst> _dienste = new List<Dienst>();
        private readonly List<Mitarbeiter> _mitarbeiter = new List<Mitarbeiter>();
        private readonly List<Mitarbeiterfoto> _mitarbeiterfotos = new List<Mitarbeiterfoto>();

        private Test_Server()
        {

        }

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

        public Test_Server Mit_Mitarbeiterfotos(params Mitarbeiterfoto[] mitarbeiterfotos)
        {
            _mitarbeiterfotos.Clear();
            _mitarbeiterfotos.AddRange(mitarbeiterfotos);
            return this;
        }

        public Test_Host Start(string adresse, int port, Log log, out string publickey)
        {
            var backend = new Test_Backend(_mitarbeiter, _mitarbeiterfotos, _dienste);
            var privatekey = CryptoService.GenerateRSAKeyPair();
            publickey = CryptoService.GetPublicKey(privatekey);
            return new Test_Host(backend, adresse, port, privatekey, log);
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

    internal class Test_Backend : PPLUS_Backend
    {
        private readonly List<Daten_mit_Version<Mitarbeiter>> _mitarbeiter;
        private readonly List<Daten_mit_Version<Mitarbeiterfoto>> _mitarbeiterfotos;
        private readonly List<Daten_mit_Version<Dienst>> _dienste;

        public Test_Backend(IEnumerable<Mitarbeiter> mitarbeiter, IEnumerable<Mitarbeiterfoto> fotos, IEnumerable<Dienst> dienste)
        {
            _mitarbeiter = mitarbeiter.Select(_ => new Daten_mit_Version<Mitarbeiter>(_, 1)).ToList();
            _mitarbeiterfotos = fotos.Select(_ => new Daten_mit_Version<Mitarbeiterfoto>(_, 1)).ToList();
            _dienste = dienste.Select(_ => new Daten_mit_Version<Dienst>(_, 1)).ToList();
            AuswahllistenVersion = 1;
            Aenderungen_an_Mitarbeiterstammdaten = new Subject<IEnumerable<string>>();
        }

        public int AuswahllistenVersion { get; }
        public IObservable<IEnumerable<string>> Aenderungen_an_Mitarbeiterstammdaten { get; }

        public IEnumerable<string> Alle_Mitarbeiterdatensaetze()
        {
            return new string[0];
        }

        public IEnumerable<Mitarbeiter> Mitarbeiterdatensaetze_abrufen(IEnumerable<string> datensatz_ids)
        {
            return new Mitarbeiter[0];
        }

        public void Dienste_setzen(IEnumerable<Dienst> dienste)
        {
            var nächste_Version = Nächste_Version(_dienste);
            _dienste.Clear();
            _dienste.AddRange(dienste.Select(_ => new Daten_mit_Version<Dienst>(_, nächste_Version)));
        }

        public void Dienste_hinzufügen(IEnumerable<Dienst> dienste)
        {
            var nächste_Version = Nächste_Version(_dienste);
            _dienste.AddRange(dienste.Select(_ => new Daten_mit_Version<Dienst>(_, nächste_Version)));
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
        }

        private static Dienst Dienst_als_gelöscht(Dienst dienst)
        {
            return new Dienst(
                dienst.Id,
                dienst.Mandant,
                dienst.Kurzbezeichnung,
                dienst.Bezeichnung,
                dienst.Gültig_ab,
                dienst.Gültig_bis,
                dienst.Beginn,
                dienst.Gültig_an,
                true
            );
        }

        public void Mitarbeiter_setzen(IEnumerable<Mitarbeiter> mitarbeiter)
        {
            var nächste_Version = Nächste_Version(_mitarbeiter);
            _mitarbeiter.Clear();
            _mitarbeiter.AddRange(mitarbeiter.Select(_ => new Daten_mit_Version<Mitarbeiter>(_, nächste_Version)));
        }

        public void Mitarbeiter_hinzufügen(IEnumerable<Mitarbeiter> mitarbeiter)
        {
            var nächste_Version = Nächste_Version(_mitarbeiter);
            _mitarbeiter.AddRange(mitarbeiter.Select(_ => new Daten_mit_Version<Mitarbeiter>(_, nächste_Version)));
        }

        public void Mitarbeiter_austreten_zum(Datum austrittsdatum, Guid mitarbeiter)
        {
            var nächste_Version = Nächste_Version(_mitarbeiter);
            var neue_Mitarbeiter =
                _mitarbeiter
                    .Select(_ =>
                        _.Data.PPLUS_Id == mitarbeiter
                            ? new Daten_mit_Version<Mitarbeiter>(
                                Mit_Mandantenzugehörigkeit_bis_zum(austrittsdatum, _.Data), nächste_Version)
                            : _)
                    .ToList();

            _mitarbeiter.Clear();
            _mitarbeiter.AddRange(neue_Mitarbeiter);
        }

        private static Mitarbeiter Mit_Mandantenzugehörigkeit_bis_zum(Datum austrittsdatum, Mitarbeiter mitarbeiter)
        {
            return new Mitarbeiter(
                mitarbeiter.PPLUS_Id,
                new ReadOnlyCollection<DM7_Mandantenzugehörigkeiten>(
                    mitarbeiter.DM7_Mandantenzugehörigkeiten
                        .Select(_ => new DM7_Mandantenzugehörigkeiten(_.MandantId, _.GueltigAb, austrittsdatum))
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
                mitarbeiter.Qualifikation,
                mitarbeiter.Kontakte
            );
        }

        public void Mitarbeiterfotos_setzen(IEnumerable<Mitarbeiterfoto> mitarbeiterfotos)
        {
            var nächste_Version = Nächste_Version(_mitarbeiterfotos);
            _mitarbeiterfotos.Clear();
            _mitarbeiterfotos.AddRange(mitarbeiterfotos.Select(_ => new Daten_mit_Version<Mitarbeiterfoto>(_, nächste_Version)));
        }

        public void Mitarbeiterfotos_hinzufügen(IEnumerable<Mitarbeiterfoto> mitarbeiterfotos)
        {
            var nächste_Version = Nächste_Version(_mitarbeiterfotos);
            _mitarbeiterfotos.AddRange(mitarbeiterfotos.Select(_ => new Daten_mit_Version<Mitarbeiterfoto>(_, nächste_Version)));
        }

        public void Mitarbeiterfoto_löschen(Guid mitarbeiter)
        {
            _mitarbeiterfotos.RemoveAll(_ => _.Data.Mitarbeiter == mitarbeiter);
        }

        public Stammdaten<Mitarbeiter> Mitarbeiter_abrufen()
        {
            return new Stammdaten<Mitarbeiter>(_mitarbeiter.Select(_ => _.Data).ToList(), Höchster_Datenstand(_mitarbeiter));
        }

        public Stammdaten<Mitarbeiter> Mitarbeiter_abrufen_ab(Datenstand stand)
        {
            var mitarbeiter =
                _mitarbeiter
                    .Where(_ => _.Version > stand.Value)
                    .Select(_ => _.Data)
                    .ToList();
            return new Stammdaten<Mitarbeiter>(mitarbeiter, Höchster_Datenstand(_mitarbeiter));
        }

        public Stammdaten<Mitarbeiterfoto> Mitarbeiterfotos_abrufen()
        {
            return new Stammdaten<Mitarbeiterfoto>(_mitarbeiterfotos.Select(_ => _.Data).ToList(), Höchster_Datenstand(_mitarbeiterfotos));
        }

        public Stammdaten<Mitarbeiterfoto> Mitarbeiterfotos_abrufen_ab(Datenstand stand)
        {
            var fotos =
                _mitarbeiterfotos
                    .Where(_ => _.Version > stand.Value)
                    .Select(_ => _.Data)
                    .ToList();
            return new Stammdaten<Mitarbeiterfoto>(fotos, Höchster_Datenstand(_mitarbeiterfotos));
        }

        public Stammdaten<Dienst> Dienste_abrufen()
        {
            return new Stammdaten<Dienst>(_dienste.Select(_ => _.Data).ToList(), Höchster_Datenstand(_dienste));
        }

        public Stammdaten<Dienst> Dienste_abrufen_ab(Datenstand stand)
        {
            var dienste =
                _dienste
                    .Where(_ => _.Version > stand.Value)
                    .Select(_ => _.Data)
                    .ToList();
            return new Stammdaten<Dienst>(dienste, Höchster_Datenstand(_dienste));
        }

        private static Datenstand Höchster_Datenstand<T>(List<Daten_mit_Version<T>> daten) => daten.Any() ? new Datenstand(daten.Max(_ => _.Version)) : new Datenstand(0);
        private static ulong Nächste_Version<T>(List<Daten_mit_Version<T>> daten) => daten.Any() ? daten.Max(_ => _.Version)+1 : 1;
    }

    public class Test_Host : IDisposable
    {
        private readonly DM7_PPLUS_Host _host;
        private readonly Test_Backend _backend;

        internal Test_Host(Test_Backend backend, string adresse, int port, string privatekey, Log log)
        {
            _backend = backend;
            _host = DM7_PPLUS_Host.Starten(
                backend,
                new StaticAuthentication("anonymous"),
                adresse,
                port,
                privatekey,
                log,
                ex => log.Info(ex.ToString()));
        }

        public void Mitarbeiter_setzen(params Mitarbeiter[] mitarbeiter)
        {
            _backend.Mitarbeiter_setzen(mitarbeiter);
        }

        public void Mitarbeiter_hinzufügen(params Mitarbeiter[] mitarbeiter)
        {
            _backend.Mitarbeiter_hinzufügen(mitarbeiter);
        }

        public void Mitarbeiter_austreten_zum(Datum austrittsdatum, Guid mitarbeiter)
        {
            _backend.Mitarbeiter_austreten_zum(austrittsdatum, mitarbeiter);
        }

        public void Mitarbeiterfotos_setzen(params Mitarbeiterfoto[] mitarbeiterfotos)
        {
            _backend.Mitarbeiterfotos_setzen(mitarbeiterfotos);
        }

        public void Mitarbeiterfotos_hinzufügen(params Mitarbeiterfoto[] mitarbeiterfotos)
        {
            _backend.Mitarbeiterfotos_hinzufügen(mitarbeiterfotos);
        }

        public void Mitarbeiterfoto_löschen(Guid mitarbeiter)
        {
            _backend.Mitarbeiterfoto_löschen(mitarbeiter);
        }

        public void Dienst_löschen(int dienstId)
        {
            _backend.Dienst_löschen(dienstId);
        }

        public void Dienste_anlegen(params Dienst[] dienste)
        {
            _backend.Dienste_hinzufügen(dienste);
        }

        public void Dienste_setzen(params Dienst[] dienste)
        {
            _backend.Dienste_setzen(dienste);
        }

        public List<Dienst> Dienste() => _backend.Dienste_abrufen().ToList();

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}