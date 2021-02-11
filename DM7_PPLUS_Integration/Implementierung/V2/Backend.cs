using System.Threading.Tasks;
using Bare.Msg;

namespace DM7_PPLUS_Integration.Implementierung.V2
{
    public interface Backend
    {
        Task<Response> HandleQuery(Query query);
    }
}