using System;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public class ConnectionSucceeded : ConnectionResult
    {
        public readonly int Api_Version;
        public readonly int Auswahllistenversion;

        public ConnectionSucceeded(int apiVersion, int auswahllistenversion)
        {
            Api_Version = apiVersion;
            Auswahllistenversion = auswahllistenversion;
        }
    }
}