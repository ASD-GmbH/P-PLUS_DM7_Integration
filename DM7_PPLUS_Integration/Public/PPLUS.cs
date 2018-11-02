using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.Client;

namespace DM7_PPLUS_Integration
{
    public static class PPLUS
    {
        /// <summary>
        /// Herstellen der Verbindung mit einer P-PLUS Server Instanz
        /// </summary>
        /// <param name="network_address">Netzwerkadresse des P-PLUS DM7_PPLUS_Integrations Endpunktes</param>
        /// <param name="credentials">Authentifizierungstoken für den Zugriff auf P-PLUS</param>
        /// <param name="log">Adapter für Statusmeldungen der DM7_PPLUS_Integrationsschnittstelle</param>
        /// <param name="cancellationToken_Verbindung">Token zum kontrollierten Abbruch während des Verbindens mit dem Server</param>
        /// <returns>Instanz der DM7_PPLUS_Integrationsschnittstelle</returns>
        public static Task<DM7_PPLUS_API> Connect(string network_address, string credentials, Log log, CancellationToken cancellationToken_Verbindung)
        {
            return Connector.Instance_API_Level_3(network_address, credentials, log, cancellationToken_Verbindung);
        }
    }
}