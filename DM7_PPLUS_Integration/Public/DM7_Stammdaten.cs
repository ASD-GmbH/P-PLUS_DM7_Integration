using System.Collections.Generic;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration
{
    public interface DM7_Stammdaten
    {
        Task<List<DM7_Mandant>> Alle_Mandanten();
        Task<List<Leistung>> Alle_Leistungen();
    }
}