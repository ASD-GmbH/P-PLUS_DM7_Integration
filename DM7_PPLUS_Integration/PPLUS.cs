using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung;

namespace DM7_PPLUS_Integration
{
    public static class PPLUS
    {
        /// <summary>
        /// Herstellen der Verbindung mit einer P-PLUS Server Instanz
        /// </summary>
        /// <param name="network_address">Netzwerkadresse des P-PLUS DM7_PPLUS_Integrations Endpunktes</param>
        /// <param name="log">Adapter für Statusmeldungen der DM7_PPLUS_Integrationsschnittstelle</param>
        /// <returns>Instanz der DM7_PPLUS_Integrationsschnittstelle</returns>
        public static Task<DM7_PPLUS_API> Connect(string network_address, Log log)
        {
            return Connector.Instance_API_Level_1(network_address, log);
        }
    }
}