using System;
using System.Threading.Tasks;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public interface Schicht_3_Protokoll__Data : IDisposable
    {
        Task<byte[]> Request(byte[] request);
        Task<byte[]> Request_Mitarbeiter(byte[] request);
        Task<byte[]> Request_Mitarbeiterfotos(byte[] request);
        Task<byte[]> Request_Dienste(byte[] request);
        IObservable<byte[]> Notifications { get; }
    }
}