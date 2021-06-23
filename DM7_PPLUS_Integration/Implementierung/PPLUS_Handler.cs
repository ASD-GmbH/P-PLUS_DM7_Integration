using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Messages;

namespace DM7_PPLUS_Integration.Implementierung
{
    public interface PPLUS_Handler
    {
        Task<Token?> Authenticate(string user, string password, TimeSpan? timeout = null);
        Task<Capabilities> Capabilities(TimeSpan? timeout = null);
        Task<QueryResult> HandleQuery(Token token, Query query, TimeSpan? timeout = null);
        Task<CommandResult> HandleCommand(Token token, Command command, TimeSpan? timeout = null);
    }
}