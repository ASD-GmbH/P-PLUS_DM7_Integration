namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public class QueryFailed : QueryResponse
    {
        public QueryFailed(QueryFailure reason, string info)
        {
            Reason = reason;
            Info = info;
        }

        public readonly QueryFailure Reason;
        public readonly string Info;
    }
}