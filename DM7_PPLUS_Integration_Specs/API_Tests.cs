using System;
using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Client;
using DM7_PPLUS_Integration.Implementierung.Server;
using DM7_PPLUS_Integration.Implementierung.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace DM7_PPLUS_Integration_Specs
{
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
            var proxy = TestConnector.Instance_API_Level_1("test://test", new TestLog("[client] "), CancellationToken.None, new LoopbackFactory(host, 2)).Result;

            Setup_Testframework(proxy, server);
        }
    }

    [TestFixture]
    public class API_Level_1__Ebene_3 : API_Test_Base
    {
        protected override void Erzeuge_Infrastruktur(int auswahllistenversion)
        {
            var server = new Test_PPLUS_Backend(auswahllistenversion);
            var log = new TestLog("[server] ");

            var host = DM7_PPLUS_Host.Starten(server, log, ex => Assert.Fail(ex.ToString()));
            var proxy = TestConnector.Instance_API_Level_1("test://test", new TestLog("[client] "), CancellationToken.None, new LoopbackFactory(host, 3)).Result;

            Setup_Testframework(proxy, server);
        }
    }

    // TODO Test: Fehlerfälle abdecken, Exceptions auslösen und prüfen, dass diese am richtigen Ort registriert werden (und der Server eine Exception überlebt)

    // TODO: Authentifizierung

    // TODO: Verschlüsselung


    [TestFixture]
    public class API_Level_1__Ebene_4 : API_Test_Base
    {

        [Test]
        public void Nach_Serverneustart_wird_die_vollstaendige_Mitarbeiterliste_uebertragen_1()
        {
            Mitarbeiter_anlegen("Martha", "Musterfrau");
            Mitarbeiter_anlegen("Marco", "Mustermann");
            Mitarbeiter_anlegen("Martin", "Mustermaus");
            var mitabeiterdatensaetze = API.Mitarbeiterdaten_abrufen();
            var bekannter_Stand = Stand(mitabeiterdatensaetze.Result);
            var verfuegbarerStand = bekannter_Stand;
            API.Stand_Mitarbeiterdaten.Subscribe(stand =>
            {
                verfuegbarerStand = stand;
            });

            Serverneustart();
            Warte_auf_Konsistenz();

            Mitarbeiter_anlegen("Tester", "Testerino");
            Warte_auf_Konsistenz();

            var data = API.Mitarbeiterdaten_abrufen(bekannter_Stand, verfuegbarerStand).Result;
            Anzahl(data).Should().Be(4);
            Teilnemge(data).Should().BeFalse();
        }

        [Test]
        public void Nach_Serverneustart_wird_die_vollstaendige_Mitarbeiterliste_uebertragen_2()
        {
            Mitarbeiter_anlegen("Martha", "Musterfrau");
            Mitarbeiter_anlegen("Marco", "Mustermann");
            Mitarbeiter_anlegen("Martin", "Mustermaus");
            var mitabeiterdatensaetze = API.Mitarbeiterdaten_abrufen();
            var bekannter_Stand = Stand(mitabeiterdatensaetze.Result);
            var verfuegbarerStand = bekannter_Stand;
            API.Stand_Mitarbeiterdaten.Subscribe(stand =>
            {
                verfuegbarerStand = stand;
            });

            Serverneustart();
            Warte_auf_Konsistenz();

            Mitarbeiter_anlegen("Tester", "Testerino");
            Warte_auf_Konsistenz();

            var data1 = API.Mitarbeiterdaten_abrufen(bekannter_Stand, verfuegbarerStand).Result;
            bekannter_Stand = Stand(data1);

            Mitarbeiter_anlegen("Tester", "Testerina");
            Warte_auf_Konsistenz();

            var data = API.Mitarbeiterdaten_abrufen(bekannter_Stand, verfuegbarerStand).Result;


            Anzahl(data).Should().Be(1);
            Teilnemge(data).Should().BeTrue();
        }

        private IDisposable _host;

        private static readonly Random r = new Random();
        private DM7_PPLUS_API _proxy;


        protected override void Erzeuge_Infrastruktur(int auswahllistenversion)
        {
            var port = r.Next(20000, 32000);
            var server = new Test_PPLUS_Backend(auswahllistenversion);
            var log = new TestLog("[server] ");

            Action start = () => { _host = DM7_PPLUS_Host.Starten(server, "tcp://127.0.0.1", port, log, ex => Assert.Fail(ex.ToString())); };
            start();
            _reset = () =>
            {
                _host.Dispose();
                Thread.Sleep(100);
                start();
            };

            _proxy = PPLUS.Connect("tcp://127.0.0.1:" + port, new TestLog("[client] "), CancellationToken.None).Result;
            Setup_Testframework(_proxy, server);
        }

        private Action _reset;

        protected void Serverneustart()
        {
            _reset();
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
        protected DM7_PPLUS_API API;
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
            var bekannter_Stand = Stand(mitabeiterdatensaetze.Result);
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
            var bekannter_Stand = Stand(mitabeiterdatensaetze.Result);
            var verfuegbarerStand = bekannter_Stand;
            API.Stand_Mitarbeiterdaten.Subscribe(stand => { verfuegbarerStand = stand; });

            Mitarbeiter_anlegen("Tester", "Testerino");
            Warte_auf_Konsistenz();

            var data = API.Mitarbeiterdaten_abrufen(bekannter_Stand, verfuegbarerStand).Result;
            Teilnemge(data).Should().Be(true);
        }

        protected bool Teilnemge(Mitarbeiterdatensaetze data)
        {
            return data.Teilmenge;
        }


        protected Stand Stand(Mitarbeiterdatensaetze data) => data.Stand;

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
