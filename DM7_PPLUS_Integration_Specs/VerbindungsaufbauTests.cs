using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration;
using DM7_PPLUS_Integration.Implementierung.Client;
using DM7_PPLUS_Integration.Implementierung.Server;
using DM7_PPLUS_Integration.Implementierung.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace DM7_PPLUS_Integration_Specs
{

    [TestFixture]
    public class VerbindungsaufbauTests_async
    {
        private Task<DM7_PPLUS_API> Verbindungsaufbau_1_async(string netzwerkadresse, out Action cancel)
        {
            cancel = () => { throw new NotImplementedException(); };
            return TestConnector.Instance_API_Level_1(netzwerkadresse, new TestLog("[client] "));
        }


        // TODO: Verbindungsaufbau blockiert im Moment unendlich lange. Es muss möglich sein, den Verbindungsaufbau abzubrechen...
        [Test]
        public void Verbindungsaufbau_kann_abgebrochen_werden__Modell_1_Timeout()
        {
            Action cancel;
            var task = Verbindungsaufbau_1_async("tcp://127.0.0.1:4711", out cancel);
            task.Wait(TimeSpan.FromMilliseconds(100));
            Warte_auf_Konsistenz();
            cancel();
            Assert.IsTrue(task.IsCanceled);
        }

        private void Warte_auf_Konsistenz()
        {
        }
    }

    [TestFixture]
    public class VerbindungsaufbauTests
    {
        private Test_PPLUS_Backend _server;
        private Ebene_2_Proxy_Factory _factory;
        private DM7_PPLUS_Host _host;

        private Level_0_Test_API Verbindungsaufbau_0(string netzwerkadresse)
        {
            return TestConnector.Instance_API_level_0_nur_fuer_Testzwecke(netzwerkadresse, new TestLog("[client] "), _factory).Result;
        }

        private DM7_PPLUS_API Verbindungsaufbau_1(string netzwerkadresse)
        {
            return TestConnector.Instance_API_Level_1(netzwerkadresse, new TestLog("[client] "), _factory).Result;
        }

        private void Server_kompatibel_mit_API_level(int server_min_api_level, int server_max_api_level)
        {
            _server = new Test_PPLUS_Backend(0);
            var levels = new[] {0, 1}.Where(_ => _ >= server_min_api_level && _ <= server_max_api_level);
            _host = DM7_PPLUS_Host.Starten(_server, new TestLog("[server] "), ex => { throw new Exception("Unexpected exception", ex); }, levels);
            _factory = new LoopbackFactory(_host, 3);
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
}