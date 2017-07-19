using System;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public class ConnectionSucceeded : ConnectionResult
    {
        public readonly int Api_Level;
        public readonly int Auswahllistenversion;
        public readonly Guid Session;

        public ConnectionSucceeded(int apiLevel, int auswahllistenversion, Guid session)
        {
            Api_Level = apiLevel;
            Auswahllistenversion = auswahllistenversion;
            Session = session;
        }
    }
}