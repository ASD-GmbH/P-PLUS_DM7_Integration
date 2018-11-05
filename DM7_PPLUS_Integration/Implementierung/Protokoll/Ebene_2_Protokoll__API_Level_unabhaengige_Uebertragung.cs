using System;
using System.Threading.Tasks;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public interface Ebene_2_Protokoll__API_Level_unabhaengige_Uebertragung : IDisposable
    {
        Task<QueryResponse> Query(string credentials, int api_level, Guid session, int datenquelle, long von, long bis);
        IObservable<Notification> Notifications { get; }
    }
}