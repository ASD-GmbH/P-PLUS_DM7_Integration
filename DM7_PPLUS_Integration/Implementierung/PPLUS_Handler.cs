using System.Threading.Tasks;
using Bare.Msg;

namespace DM7_PPLUS_Integration.Implementierung
{
    public interface PPLUS_Handler
    {
        Task<Token?> Authenticate(string user, string password);
        Task<Capabilities> Capabilities();
        Task<Query_Result> HandleQuery(Token token, Query query);
    }
}