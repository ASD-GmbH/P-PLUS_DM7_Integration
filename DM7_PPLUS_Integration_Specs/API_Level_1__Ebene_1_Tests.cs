using System;
using System.Threading;
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
            return Connector.Instance_API_level_0_nur_fuer_Testzwecke(netzwerkadresse, new TestLog("[client] "), _factory).Result;
        }

        private DM7_PPLUS_API Verbindungsaufbau_1(string netzwerkadresse)
        {
            return Connector.Instance_API_Level_1(netzwerkadresse, new TestLog("[client] "), _factory).Result;
        }

        private void Server_kompatibel_mit_API_level(int server_min_api_level, int server_max_api_level)
        {
            var session = Guid.NewGuid();
            _server = new Test_PPLUS_Backend(0);


            var level0 = new TestBackend_Level_0();
            var level1 = new API_Level_1_Adapter(_server, ex => { throw new Exception("Unexpected exception", ex); }, session, new TestLog("server "));

            if (server_min_api_level > 0) level0 = null;
            if (server_max_api_level < 1) level1 = null;

            var router = new API_Router(new TestLog("[server] "), session, 0, level0, level1);
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

    internal class Test_ProxyFactory : Ebene_2_Proxy_Factory
    {
        private readonly Ebene_2_Protokoll__Verbindungsaufbau _verbindung;
        private readonly Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung _api;

        public Test_ProxyFactory(Ebene_2_Protokoll__Verbindungsaufbau verbindung, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung api)
        {
            _verbindung = verbindung;
            _api = api;
        }

        public Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>> Connect_Ebene_2(string networkAddress, Log log)
        {
            var adapter = new Service_Adapter(_verbindung);
            var client = new Service_Proxy(adapter);

            var task =
                new Task<Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>>(
                    () => new Tuple<Ebene_2_Protokoll__Verbindungsaufbau, Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung>(
                        client,
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
            var server = new Test_PPLUS_Backend(auswahllistenversion);
            var log = new TestLog("[server] ");

            var host = DM7_PPLUS_Host.Starten(server, log, ex => Assert.Fail(ex.ToString()));

            Setup_Testframework(host.Ebene_1_API_Level_1, server);
        }
    }

    [TestFixture]
    public class API_Level_1__Ebene_2 : API_Test_Base
    {
        protected override void Erzeuge_Infrastruktur(int auswahllistenversion)
        {
            var server = new Test_PPLUS_Backend(auswahllistenversion);
            var log = new TestLog("[server] ");

            var host = DM7_PPLUS_Host.Starten(server, log, ex => Assert.Fail(ex.ToString()));

            var connection = (ConnectionSucceeded)host.Ebene_2_Service.Connect_Ebene_1("test", 1, 1).Result;
            var proxy = new API_Level_1_Proxy(host.Ebene_2_Data, connection.Session, connection.Auswahllistenversion, new TestLog("[client] "));

            Setup_Testframework(proxy, server);
        }
    }

    [TestFixture]
    public class API_Level_1__Ebene_3__mit_Connect_nur_auf_Ebene_2 : API_Test_Base
    {
        protected override void Erzeuge_Infrastruktur(int auswahllistenversion)
        {
            var server = new Test_PPLUS_Backend(auswahllistenversion);
            var log = new TestLog("[server] ");

            var host = DM7_PPLUS_Host.Starten(server, log, ex => Assert.Fail(ex.ToString()));

            var deserialisierung = new Data_Adapter(host.Ebene_2_Data);
            var serialisierung = new Data_Proxy(deserialisierung);

            var connection = (ConnectionSucceeded)host.Ebene_2_Service.Connect_Ebene_1("test", 1, 1).Result;
            var proxy = new API_Level_1_Proxy(serialisierung, connection.Session, connection.Auswahllistenversion, new TestLog("[client] "));
            Setup_Testframework(proxy, server);
        }
    }

    [TestFixture]
    public class API_Level_1__Ebene_3__mit_Connect_auf_Ebene_3 : API_Test_Base
    {
        protected override void Erzeuge_Infrastruktur(int auswahllistenversion)
        {
            var server = new Test_PPLUS_Backend(auswahllistenversion);
            var log = new TestLog("[server] ");

            var host = DM7_PPLUS_Host.Starten(server, log, ex => Assert.Fail(ex.ToString()));

            var connector = new Service_Proxy(host.Ebene_3_Service);
            var serialisierung = new Data_Proxy(host.Ebene_3_Data);
            var connection = (ConnectionSucceeded)connector.Connect_Ebene_1("test", 1, 1).Result;
            var proxy = new API_Level_1_Proxy(serialisierung, connection.Session, connection.Auswahllistenversion, new TestLog("[client] "));
            Setup_Testframework(proxy, server);
        }
    }

    // TODO Test: Connect without Server present can be aborted (Test in voller NetMQ Infrastruktur)

    // TODO Test: Neue Session führt zu Neuübertragung aller Daten

    // TODO: Authentifizierung

    // TODO: Verschlüsselung

    [TestFixture]
    public class API_Level_1__Ebene_5 : API_Test_Base
    {
        private IDisposable _host;

        private static readonly Random r = new Random();
        private DM7_PPLUS_API _proxy;


        protected override void Erzeuge_Infrastruktur(int auswahllistenversion)
        {
            var port = r.Next(20000, 32000);
            var server = new Test_PPLUS_Backend(auswahllistenversion);
            var log = new TestLog("[server] ");

            _host = DM7_PPLUS_Host.Starten(server, "tcp://127.0.0.1", port, log, ex => Assert.Fail(ex.ToString()));

            _proxy = PPLUS.Connect("tcp://127.0.0.1:" + port, new TestLog("[client] ")).Result;
            Setup_Testframework(_proxy, server);
        }

        protected override void Beende_Infrastruktur()
        {
            _proxy.Dispose();
            _host.Dispose();
        }

        protected override void Warte_auf_Konsistenz()
        {
            Thread.Sleep(100);
        }


        [Test]
        public void TerminationTest()
        {
            Assert.Pass();
        }
    }

    public class TestLog : Log
    {
        private readonly string _prefix;

        public TestLog(string prefix)
        {
            _prefix = prefix;
        }

        private string Meta => $"[{Thread.CurrentThread.ManagedThreadId}] ";

        public void Info(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Meta+_prefix+text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void Debug(string text)
        {
            Console.WriteLine(Meta + _prefix + text);
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

        [TearDown]
        public void TearDown()
        {
            Beende_Infrastruktur();
        }

        protected abstract void Erzeuge_Infrastruktur(int auswahllistenversion);
        protected virtual void Beende_Infrastruktur() { }

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

        [Test]
        public void Teilweise_Daten_werden_als_Teilmenge_gekennzeichnet()
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
            Teilnemge(data).Should().Be(true);
        }

        private bool Teilnemge(Mitarbeiterdatensaetze data)
        {
            return data.Teilmenge;
        }


        private Stand Stand(Task<Mitarbeiterdatensaetze> data) => data.Result.Stand;

        protected virtual void Warte_auf_Konsistenz()
        {
        }

        private void Auswahllisten_Version(int auswahllistenversion)
        {
            Beende_Infrastruktur();
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
