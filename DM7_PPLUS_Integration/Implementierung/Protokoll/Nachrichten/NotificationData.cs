using System;

namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public class NotificationData : Notification
    {
        public readonly Guid Session;
        public readonly int Datenquelle;
        public readonly long Version;

        public NotificationData(Guid session, int datenquelle, long version)
        {
            Session = session;
            Datenquelle = datenquelle;
            Version = version;
        }
    }
}