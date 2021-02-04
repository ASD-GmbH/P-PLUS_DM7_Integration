using System;
using System.Threading.Tasks;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public interface Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung : IDisposable
    {
        Task<QueryResponse> Query_Mitarbeiterdatensätze(string credentials, int api_version, Guid session, int datenquelle, long von, long bis);
        Task<QueryResponse> Query_Mitarbeiter(string credentials, int api_version, Guid session, Datenstand? stand);
        Task<QueryResponse> Query_Mitarbeiterfotos(string credentials, int api_version, Guid session, Datenstand? stand);
        Task<QueryResponse> Query_Dienste(string credentials, int api_version, Guid session, Datenstand? stand);
        IObservable<Notification> Notifications { get; }
    }
}