using System;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public class ConnectionSucceeded : ConnectionResult
    {
        public readonly int Api_Level;
        public readonly int Auswahllistenversion;

        public ConnectionSucceeded(int apiLevel, int auswahllistenversion)
        {
            Api_Level = apiLevel;
            Auswahllistenversion = auswahllistenversion;
        }
    }
}