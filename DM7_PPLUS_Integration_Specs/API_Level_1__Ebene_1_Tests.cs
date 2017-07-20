using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Client;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Server;
using DM7_PPLUS_Integration.Implementierung.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace DM7_PPLUS_Integration_Specs
{

    [TestFixture]
    public class Verbindungsaufbau_Tests
    {
        private Test_PPLUS_Backend _server;
        private Test_ProxyFactory _factory;

        private Level_0_Test_API Verbindungsaufbau_0(string netzwerkadresse)
        {
            return Connector.Instance_API_level_0_nur_fuer_Testzwecke(netzwerkadresse, null, _factory).Result;
        }

        private DM7_PPLUS_API Verbindungsaufbau_1(string netzwerkadresse)
        {
            return Connector.Instance_API_Level_1(netzwerkadresse, null, _factory).Result;
        }

        private void Server_kompatibel_mit_API_level(int server_min_api_level, int server_max_api_level)
        {
            var session = Guid.NewGuid();
            _server = new Test_PPLUS_Backend();


            var level0 = new TestBackend_Level_0();
            var level1 = new API_Level_1_Adapter(_server, ex => throw new Exception("Unexpected exception", ex), session, 0);

            if (server_min_api_level > 0) level0 = null;
            if (server_max_api_level < 1) level1 = null;

            var router = new API_Router(session, 0, level0, level1);
            _factory = new Test_ProxyFactory(router, router);
        }

        [Test]
        public void Verbindungsaufbau__Proxy_fuer_API_Level_1__Host_unterstuetzt_API_Level_1__liefert_API_Level_1()
        {
            Server_kompatibel_mit_API_level(1, 1);
            var api = Verbindungsaufbau_1("test://test");
            api.Should().NotBeNull("Kein gemeinsames API Level gefunden!");
        }

        [Test]
        public void Verbindungsaufbau__Proxy_fuer_API_Level_0__Host_unterstuetzt_nur_API_Level_1__schlaegt_fehl()
        {
            Server_kompatibel_mit_API_level(1, 1);
            Action connect = () => Verbindungsaufbau_0("test://test");
            connect.ShouldThrow<UnsupportedVersionException>();
        }

        [Test]
        public void Verbindungsaufbau__Proxy_fuer_API_Level_1__Host_unterstuetzt_nur_API_Level_0__schlaegt_fehl()
        {
            Server_kompatibel_mit_API_level(0, 0);
            Action connect = () => Verbindungsaufbau_1("test://test");
            connect.ShouldThrow<UnsupportedVersionException>();
        }

        [Test]
        public void
            Verbindungsaufbau__Proxy_fuer_API_Level_0__Host_unterstuetzt_API_Level_0_und_1__liefert_API_Level_0()
        {
            Server_kompatibel_mit_API_level(0, 1);
            var api = Verbindungsaufbau_0("test://test");
            api.Should().NotBeNull("Kein gemeinsames API Level gefunden!");
        }

        [Test]
        public void
            Verbindungsaufbau__Proxy_fuer_API_Level_1__Host_unterstuetzt_API_Level_0_und_1__liefert_API_Level_1()
        {
            Server_kompatibel_mit_API_level(0, 1);
            var api = Verbindungsaufbau_1("test://test");
            api.Should().NotBeNull("Kein gemeinsames API Level gefunden!");
        }

        [Test]
        public void
            Verbindungsaufbau__Proxy_fuer_API_Level_0_und_1_angefragt_level_0__Host_unterstuetzt_API_Level_0__liefert_API_Level_0()
        {
            Server_kompatibel_mit_API_level(0, 0);
            var api = Verbindungsaufbau_0("test://test");
            api.Should().NotBeNull("Kein gemeinsames API Level gefunden!");
        }

        [Test]
        public void
            Verbindungsaufbau__Proxy_fuer_API_Level_0_und_1_angefragt_level_1__Host_unterstuetzt_API_Level_0__liefert_API_Level_1_downgrade()
        {
            Server_kompatibel_mit_API_level(0, 0);
            var api = Verbindungsaufbau_1("test://0");
            api.Should().NotBeNull("Kein gemeinsames API Level gefunden!");
            api.Auswahllisten_Version.Should().Be(4711);
        }
    }

    internal class Test_ProxyFactory : ProxyFactory
    {
        private readonly Ebene_2_Protokoll__Verbindungsaufbau _verbindung;
        private readonly API_Router _api;

        public Test_ProxyFactory(Ebene_2_Protokoll__Verbindungsaufbau verbindung, API_Router api)
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


    [TestFixture]
    public sealed class API_Level_1__Ebene_1 : API_Test_Base
    {
        protected override void Erzeuge_Infrastruktur(int auswahllistenversion)
        {
            var session = Guid.NewGuid();

            var server = new Test_PPLUS_Backend();
            var adapter = new API_Level_1_Adapter(server, ex => throw new Exception("Unexpected exception", ex), session, auswahllistenversion);

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
            var adapter = new API_Level_1_Adapter(server, ex => throw new Exception("Unexpected exception", ex), session, auswahllistenversion);
            var router = new API_Router(session, auswahllistenversion, null, adapter);
            var connector = (Ebene_2_Protokoll__Verbindungsaufbau) router;
            var connection = (ConnectionSucceeded)connector.Connect("test", 1, 1).Result;
            var proxy = new API_Level_1_Proxy(router, session, connection.Auswahllistenversion);

            Setup_Testframework(proxy, server);
        }
    }

    [TestFixture]
    public class API_Level_1__Ebene_3 : API_Test_Base
    {
        protected override void Erzeuge_Infrastruktur(int auswahllistenversion)
        {
            var session = Guid.NewGuid();

            var server = new Test_PPLUS_Backend();
            var adapter = new API_Level_1_Adapter(server, ex => throw new Exception("Unexpected exception", ex), session, auswahllistenversion);
            var router = new API_Router(session, auswahllistenversion, null, adapter);
            var connector = (Ebene_2_Protokoll__Verbindungsaufbau)router;

            var deserialisierung = new Serialization_Adapter(router);
            var serialisierung = new Serialization_Proxy(deserialisierung);

            var connection = (ConnectionSucceeded)connector.Connect("test", 1, 1).Result;
            var proxy = new API_Level_1_Proxy(serialisierung, connection.Session, connection.Auswahllistenversion);
            Setup_Testframework(proxy, server);
        }
    }

    [TestFixture]
    public class API_Level_1__Ebene_3_service : API_Test_Base
    {
        protected override void Erzeuge_Infrastruktur(int auswahllistenversion)
        {
            var session = Guid.NewGuid();
            var server = new Test_PPLUS_Backend();
            var adapter = new API_Level_1_Adapter(server, ex => throw new Exception("Unexpected exception", ex), session, auswahllistenversion);
            var router = new API_Router(session, auswahllistenversion, null, adapter);
            var deserialisierung = new Serialization_Adapter(router);
            var c_adapter = new ServiceAdapter(router);

            var connector = new ServiceClient(c_adapter);
            var serialisierung = new Serialization_Proxy(deserialisierung);
            var connection = (ConnectionSucceeded)connector.Connect("test", 1, 1).Result;
            var proxy = new API_Level_1_Proxy(serialisierung, connection.Session, connection.Auswahllistenversion);
            Setup_Testframework(proxy, server);
        }
    }

    // TODO Test: Connect without Server present can be aborted

    // TODO Test: Neue Session führt zu Neuübertragung aller Daten

    [TestFixture, Ignore]
    public class API_Level_1__Ebene_5 : API_Test_Base
    {
        private IDisposable _host;

        private static readonly Random r = new Random();

        [Test]
        public void TerminationTest()
        {
            Assert.Pass();
        }

        protected override void Erzeuge_Infrastruktur(int auswahllistenversion)
        {
            var port = r.Next(20000, 32000);
            var session = Guid.NewGuid();
            var server = new Test_PPLUS_Backend();
            var adapter = new API_Level_1_Adapter(server, ex => throw new Exception("Unexpected exception", ex), session, auswahllistenversion);
            var router = new API_Router(Guid.NewGuid(), auswahllistenversion, null, adapter);
            var deserialisierung = new Serialization_Adapter(router);
            _host = new NetMQ_Server(deserialisierung, "tcp://127.0.0.1:" + port);
            var proxy = PPLUS.Connect("tcp://127.0.0.1:" + port, new TestLog()).Result;
            Setup_Testframework(proxy, server);
        }

        [TearDown]
        public void Cleanup()
        {
            _host.Dispose();
        }
    }

    public class TestLog : Log
    {
        public void Info(string text)
        {
            Console.WriteLine(text);
        }

        public void Debug(string text)
        {
            Console.WriteLine(text);
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
                ex => throw new Exception("Unexpected exception", ex)));

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
