using System.Threading.Tasks;
using DM7_PPLUS_Integration.Messages;

namespace DM7_PPLUS_Integration.Implementierung
{
    public interface PPLUS_Handler
    {
        Task<Token?> Authenticate(string user, string password);
        Task<Capabilities> Capabilities();
        Task<QueryResult> HandleQuery(Token token, Query query);
        Task<CommandResult> HandleCommand(Token token, Command command);
    }
}