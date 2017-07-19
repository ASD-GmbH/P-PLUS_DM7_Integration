namespace DM7_PPLUS_Integration.Implementierung.Protokoll
{
    public class QueryResult : QueryResponse
    {
        public QueryResult(byte[] data)
        {
            Data = data;
        }

        public readonly byte[] Data;
    }
}