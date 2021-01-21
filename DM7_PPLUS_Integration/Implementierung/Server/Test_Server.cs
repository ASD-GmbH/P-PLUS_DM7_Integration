using System;
using System.Collections.Generic;
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

        private Test_Server()
        {

        }

        public Test_Server Mit_Diensten(params Dienst[] dienste)
        {
            _dienste.Clear();
            _dienste.AddRange(dienste);
            return this;
        }

        public Test_Host Start(string adresse, int port, Log log, out string publickey)
        {
            var backend = new Test_Backend(_dienste);
            var privatekey = CryptoService.GenerateRSAKeyPair();
            publickey = CryptoService.GetPublicKey(privatekey);
            return new Test_Host(backend, adresse, port, privatekey, log);
        }
    }

    internal class Test_Backend : PPLUS_Backend
    {
        private readonly List<Dienst> _dienste;

        public Test_Backend(List<Dienst> dienste)
        {
            _dienste = dienste;
            AuswahllistenVersion = 1;
            Aenderungen_an_Mitarbeiterstammdaten = new Subject<IEnumerable<string>>();
        }

        public int AuswahllistenVersion { get; }
        public IObservable<IEnumerable<string>> Aenderungen_an_Mitarbeiterstammdaten { get; }

        public IEnumerable<string> Alle_Mitarbeiterdatensaetze()
        {
            return new string[0];
        }

        public IEnumerable<Mitarbeiterdatensatz> Mitarbeiterdatensaetze_abrufen(IEnumerable<string> datensatz_ids)
        {
            return new Mitarbeiterdatensatz[0];
        }

        public IEnumerable<Dienst> Dienste_abrufen()
        {
            return _dienste;
        }

        public void Dienste_setzen(IEnumerable<Dienst> dienste)
        {
            _dienste.Clear();
            _dienste.AddRange(dienste);
        }

        public void Dienste_hinzufügen(IEnumerable<Dienst> dienste)
        {
            _dienste.AddRange(dienste);
        }

        public void Dienst_löschen(int dienst)
        {
            var neue_Dienste =
                _dienste
                    .Select(_ => _.Id == dienst ? Dienst_als_gelöscht(_) : _)
                    .ToList();
            _dienste.Clear();
            _dienste.AddRange(neue_Dienste);
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

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}