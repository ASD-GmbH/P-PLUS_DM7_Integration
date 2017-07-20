using System;
using System.Threading.Tasks;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public interface Ebene_3_Protokoll__Service : IDisposable
    {
        Task<byte[]> ServiceRequest(byte[] request);
    }
}