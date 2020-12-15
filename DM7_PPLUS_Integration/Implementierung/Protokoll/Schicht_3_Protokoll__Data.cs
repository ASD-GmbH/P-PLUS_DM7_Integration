using System;
using System.Threading.Tasks;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public interface Schicht_3_Protokoll__Data : IDisposable
    {
        Task<byte[]> Request(byte[] request);
        IObservable<byte[]> Notifications { get; }
    }
}