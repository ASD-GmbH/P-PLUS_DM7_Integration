using System.Threading.Tasks;
using Bare.Msg;

namespace DM7_PPLUS_Integration.Implementierung.V2
{
    public interface PPLUS_Handler
    {
        Task<Token?> Authenticate(string user, string password);
        Task<Response> HandleQuery(Query_Message message);
    }
}