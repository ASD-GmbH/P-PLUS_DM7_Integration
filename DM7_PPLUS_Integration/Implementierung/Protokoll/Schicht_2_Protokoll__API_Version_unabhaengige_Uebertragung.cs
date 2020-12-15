using System;
using System.Threading.Tasks;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public interface Schicht_2_Protokoll__API_Version_unabhaengige_Uebertragung : IDisposable
    {
        Task<QueryResponse> Query(string credentials, int api_version, Guid session, int datenquelle, long von, long bis);
        IObservable<Notification> Notifications { get; }
    }
}