using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Client;

namespace DM7_PPLUS_Integration.Implementierung.Testing
{
    public static class TestConnector
    {
        public static Task<Version_0_Test_API> Instance_API_version_0_nur_fuer_Testzwecke(string networkAddress, Log log, Schicht_2_Proxy_Factory factory = null)
        {
            return Connector.Instance_API_version_0_nur_fuer_Testzwecke(networkAddress, "test", log, CancellationToken.None, factory);
        }

        public static Task<Legacy_DM7_PPLUS_API> Instance_API_Version_1(string networkAddress, Log log, CancellationToken cancellationToken_Verbindung, Schicht_2_Proxy_Factory factory = null)
        {
            return Connector.Instance_API_Version_1(networkAddress, "test", log, cancellationToken_Verbindung, factory);
        }

        public static Task<Legacy_DM7_PPLUS_API> Instance_API_Version_3(string networkAddress, Log log, CancellationToken cancellationToken_Verbindung, Schicht_2_Proxy_Factory factory = null)
        {
            return Connector.Instance_API_Version_3(networkAddress, "test", log, cancellationToken_Verbindung, factory);
        }
    }
}