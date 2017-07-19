using System;
using System.Threading.Tasks;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public interface Ebene_3_Protokoll__Netzwerkuebertragung : IDisposable
    {
        Task<byte[]> Request(byte[] request);
        IObservable<byte[]> Notifications { get; }
    }
}