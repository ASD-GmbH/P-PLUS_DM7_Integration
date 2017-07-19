namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public class NotificationsClosed : Notification
    {
        public NotificationsClosed(NotificationClosedReason reason, string info)
        {
            Reason = reason;
            Info = info;
        }

        public readonly NotificationClosedReason Reason;
        public readonly string Info;
    }
}