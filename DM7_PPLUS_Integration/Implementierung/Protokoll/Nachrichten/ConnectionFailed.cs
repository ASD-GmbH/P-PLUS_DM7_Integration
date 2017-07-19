namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public class ConnectionFailed : ConnectionResult
    {
        public ConnectionFailed(ConnectionFailure reason, string info)
        {
            Reason = reason;
            Info = info;
        }

        public readonly ConnectionFailure Reason;
        public readonly string Info;
    }
}