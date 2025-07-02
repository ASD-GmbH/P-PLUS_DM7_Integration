using System;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Messages.PPLUS;

namespace DM7_PPLUS_Integration.Implementierung.PPLUS
{
    public interface PPLUS_Handler : IDisposable
    {
        Task<Token?> Authenticate(string user, string password, TimeSpan? timeout = null);
        Task<Capabilities> Capabilities(TimeSpan? timeout = null);
        Task<QueryResult> HandleQuery(Token token, Query query, TimeSpan? timeout = null);
        Task<CommandResult> HandleCommand(Token token, Command command, TimeSpan? timeout = null);
        event Action Mitarbeiteränderungen_liegen_bereit;
        event Action Dienständerungen_liegen_bereit;
        event Action Dienstbuchungsänderungen_liegen_bereit;
    }
}