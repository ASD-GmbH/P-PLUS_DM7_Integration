using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Client;

namespace DM7_PPLUS_Integration.Implementierung.Testing
{
    public static class TestConnector
    {
        public static Task<Level_0_Test_API> Instance_API_level_0_nur_fuer_Testzwecke(string networkAddress, Log log, Ebene_2_Proxy_Factory factory = null)
        {
            return Connector.Instance_API_level_0_nur_fuer_Testzwecke(networkAddress, log, CancellationToken.None, factory);
        }

        public static Task<DM7_PPLUS_API> Instance_API_Level_1(string networkAddress, Log log, CancellationToken cancellationToken_Verbindung, Ebene_2_Proxy_Factory factory = null)
        {
            return Connector.Instance_API_Level_1(networkAddress, log, cancellationToken_Verbindung, factory);
        }
    }
}