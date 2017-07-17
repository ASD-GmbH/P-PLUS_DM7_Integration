using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung;
using FluentAssertions;
using NUnit.Framework;

namespace DM7_PPLUS_Integration_Specs
{

    [TestFixture]
    public class Verbindungsaufbau_Tests
    {
        private Type API_Level_1()
        {
            return typeof(API_Level_1_Proxy);
        }

        private Type API_Level_2()
        {
            return typeof(API_Level_2_Proxy);
        }

        private DM7_PPLUS_API Verbindungsaufbau(string netzwerkadresse, int server_min_api_level, int server_max_api_level)
        {
            var session = Guid.NewGuid();
            var server = new Test_PPLUS_Backend();

            var testProxyFactory =
                new Test_ProxyFactory(
                    new Test_Connector(server_max_api_level, server_min_api_level, 0, session),
                    new API_Router(new API_Level_1_Adapter(server, ex => { throw new Exception("Unexpected exception", ex); }, session, 0)));
            return Connector.Instance(netzwerkadresse, null, testProxyFactory).Result;
        }

        [Test]
        public void Verbindungsaufbau__Proxy_fuer_API_Level_1__Host_unterstuetzt_nur_API_Level_1__liefert_API_Level_1()
        {
            var api = Verbindungsaufbau("test://|1|1", 1, 1);
            api.Should().BeOfType(API_Level_1(), "Kleinster gemeinsamer API-Level zwischen Client und Server ist 1");
        }

        [Test]
        public void Verbindungsaufbau__Proxy_fuer_API_Level_1__Host_unterstuetzt_nur_API_Level_2__schlaegt_fehl()
        {
            Action connect = () => Verbindungsaufbau("test://|1|1", 2, 2);
            connect.ShouldThrow<UnsupportedVersionException>();
        }

        [Test]
        public void Verbindungsaufbau__Proxy_fuer_API_Level_1__Host_unterstuetzt_API_Level_1_und_2__liefert_API_Level_1()
        {
            var api = Verbindungsaufbau("test://|1|1", 1, 2);
            api.Should().BeOfType(API_Level_1(), "Kleinster gemeinsamer API-Level zwischen Client und Server ist 1");
        }

        [Test]
        public void Verbindungsaufbau__Proxy_fuer_API_Level_1_bis_2__Host_unterstuetzt_API_Level_1_und_2__liefert_API_Level_2()
        {
            var api = Verbindungsaufbau("test://|1|2", 1, 2);
            api.Should().BeOfType(API_Level_2(), "Größter gemeinsamer API-Level zwischen Client und Server ist 2");
        }

        [Test]
        public void Verbindungsaufbau__Proxy_fuer_API_Level_1_bis_3__Host_unterstuetzt_API_Level_1_und_2__liefert_API_Level_2()
        {
            var api = Verbindungsaufbau("test://|1|3", 1, 2);
            api.Should().BeOfType(API_Level_2(), "Größter gemeinsamer API-Level zwischen Client und Server ist 2");
        }
    }

    internal class Test_ProxyFactory : ProxyFactory
    {
        private readonly Test_Connector _verbindung;
        private readonly API_Router _api;

        public Test_ProxyFactory(Test_Connector verbindung, API_Router api)
        {
            _verbindung = verbindung;
            _api = api;
        }

        public Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>> Connect(string networkAddress, Log log)
        {
            var task =
                new Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>>(
                    () => new Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>(
                        _verbindung,
                        _api));
            task.RunSynchronously();
            return task;
        }
    }

    internal class Test_Connector : Ebene_2_Protokoll__Verbindungsaufbau
    {
        private readonly int _maxApiLevel;
        private readonly int _minApiLevel;
        private readonly int _auswahllistenversion;
        private readonly Guid _session;

        public Test_Connector(int maxApiLevel, int minApiLevel, int auswahllistenversion, Guid session)
        {
            _maxApiLevel = maxApiLevel;
            _minApiLevel = minApiLevel;
            _auswahllistenversion = auswahllistenversion;
            _session = session;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<ConnectionResult> Connect(string login, int maxApiLevel, int minApiLevel)
        {
            var task = new Task<ConnectionResult>(() =>
            {
                var max = Math.Min(maxApiLevel, _maxApiLevel);
                var min = Math.Max(minApiLevel, _minApiLevel);
                if (max >= min) return new ConnectionSucceeded(max, _auswahllistenversion, _session);
                var range = (_maxApiLevel == _minApiLevel) ? $"des Levels {_maxApiLevel}" : $"von Level {_minApiLevel} bis {_maxApiLevel}";
                return new ConnectionFailed(ConnectionFailure.Unable_to_provide_API_level, $"Dieser P-PLUS-Server kann nur APIs {range} bereitstellen.");
            });
            task.RunSynchronously();
            return task;
        }
    }

    [TestFixture]
    public sealed class API_Level_1__Ebene_1 : API_Test_Base
    {
        protected override void Erzeuge_Infrastruktur(int auswahllistenversion)
        {
            var session = Guid.NewGuid();

            var server = new Test_PPLUS_Backend();
            var adapter = new API_Level_1_Adapter(server, ex => { throw new Exception("Unexpected exception", ex); }, session, auswahllistenversion);

            Setup_Testframework(adapter, server);
        }
    }

    [TestFixture]
    public class API_Level_1__Ebene_2 : API_Test_Base
    {
        protected override void Erzeuge_Infrastruktur(int auswahllistenversion)
        {
            var session = Guid.NewGuid();

            var server = new Test_PPLUS_Backend();
            var adapter = new API_Level_1_Adapter(server, ex => { throw new Exception("Unexpected exception", ex); }, session, auswahllistenversion);
            var router = new API_Router(adapter);
            var proxy = new API_Level_1_Proxy(router, session, auswahllistenversion);

            Setup_Testframework(proxy, server);
        }
    }

    [TestFixture]
    public class API_Level_1__Ebene_3 : API_Test_Base
    {
        private NetMQ_Server _netMQServer;

        protected override void Erzeuge_Infrastruktur(int auswahllistenversion)
        {
            var session = Guid.NewGuid();
            var server = new Test_PPLUS_Backend();
            var adapter = new API_Level_1_Adapter(server, ex => { throw new Exception("Unexpected exception", ex); }, session, auswahllistenversion);
            var router = new API_Router(adapter);
            var deserialisierung = new Deserialisierung_Adapter(router);
            _netMQServer = new NetMQ_Server(deserialisierung);


            var netMqClient = new Fake_NetMQ_Client(_netMQServer);
            var serialisierung = new Serialisierung_Proxy(netMqClient);
            var proxy = new API_Level_1_Proxy(serialisierung, session, auswahllistenversion);
            Setup_Testframework(proxy, server);
        }
    }

    public abstract class API_Test_Base
    {
        private DM7_PPLUS_API API;
        private Test_PPLUS_Backend _server;

        protected void Setup_Testframework(DM7_PPLUS_API level_1_API, Test_PPLUS_Backend server)
        {
            API = level_1_API;
            _server = server;
        }

        [SetUp]
        public void Setup()
        {
            Erzeuge_Infrastruktur(0);
        }

        protected abstract void Erzeuge_Infrastruktur(int auswahllistenversion);

        [Test]
        public void Leere_Mitarbeiterliste_wird_zurueckgeliefert()
        {
            var data = API.Mitarbeiterdaten_abrufen().Result;
            Anzahl(data).Should().Be(0);
        }

        [Test]
        public void Ein_vorhandener_Mitarbeiter_wird_zurueckgeliefert()
        {
            Mitarbeiter_anlegen("Martha", "Musterfrau");

            var data = API.Mitarbeiterdaten_abrufen().Result;
            Anzahl(data).Should().Be(1);
        }

        [Test]
        public void Auswahllistenversion_wird_uebertragen()
        {
            Auswahllisten_Version(2);
            var data = API.Auswahllisten_Version;
            data.Should().Be(2);
        }

        [Test]
        public void Der_Client_wird_ueber_neue_Mitarbeiter_benachrichtigt()
        {
            int benachrichtigt = 0;

            API.Stand_Mitarbeiterdaten.Subscribe(new Observer<Stand>(
                stand => { benachrichtigt++; },
                ex => { throw new Exception("Unexpected exception", ex); }));

            Mitarbeiter_anlegen("Martha", "Musterfrau");

            Warte_auf_Konsistenz();

            benachrichtigt.Should().Be(1);
        }

        [Test]
        public void Nur_neue_Mitarbeiter_werden_abgefragt()
        {
            Mitarbeiter_anlegen("Martha", "Musterfrau");
            Mitarbeiter_anlegen("Marco", "Mustermann");
            Mitarbeiter_anlegen("Martin", "Mustermaus");
            var mitabeiterdatensaetze = API.Mitarbeiterdaten_abrufen();
            var bekannter_Stand = Stand(mitabeiterdatensaetze);
            var verfuegbarerStand = bekannter_Stand;
            API.Stand_Mitarbeiterdaten.Subscribe(stand => { verfuegbarerStand = stand; });

            Mitarbeiter_anlegen("Tester", "Testerino");
            Warte_auf_Konsistenz();

            var data = API.Mitarbeiterdaten_abrufen(bekannter_Stand, verfuegbarerStand).Result;
            Anzahl(data).Should().Be(1);
        }


        private Stand Stand(Task<Mitarbeiterdatensaetze> data) => data.Result.Stand;

        private void Warte_auf_Konsistenz()
        {
        }

        private void Auswahllisten_Version(int auswahllistenversion)
        {
            Erzeuge_Infrastruktur(auswahllistenversion);
        }

        protected void Mitarbeiter_anlegen(string name, string nachname)
        {
            _server.Mitarbeiter_hinzufuegen(name, nachname);
        }

        protected int Anzahl(Mitarbeiterdatensaetze data)
        {
            return data.Mitarbeiter.Count;
        }
    }
}
