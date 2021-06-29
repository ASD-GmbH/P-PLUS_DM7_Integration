using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DM7_PPLUS_Integration;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.DM7;

namespace Demo_Implementierung
{
    public class Demo_DM7_Stammdaten_Service : IDisposable
    {
        private readonly Adapter adapter;

        public Demo_DM7_Stammdaten_Service(string addresse, int port, string encryptionKey)
        {
            var stammdaten = new Demo_DM7_Stammdaten_Produzent();
            adapter = new Adapter(addresse, port, stammdaten, encryptionKey);
        }

        public void Dispose()
        {
            adapter?.Dispose();
        }
    }

    public class Demo_DM7_Stammdaten_Produzent : DM7_Stammdaten
    {
        public Task<List<DM7_Mandant>> Alle_Mandanten()
        {
            return Task.FromResult(new List<DM7_Mandant>
            {
                new DM7_Mandant(Guid.Parse("D52EB39C-C7B7-49EA-8610-AA716D8FBDE1"), "Petershausen"),
                new DM7_Mandant(Guid.Parse("86E84E40-A336-4086-8BDE-F2C64E40CD3B"), "Heinzelhaus")
            });
        }

        public Task<List<Leistung>> Alle_Leistungen()
        {
            return Task.FromResult(new List<Leistung>
            {
                new Leistung(Guid.Parse("AE608ACB-75D0-46D7-BD20-9DAC478712F6"), "Einkaufen"),
                new Leistung(Guid.Parse("375FE4BB-7E57-4326-A7DD-B4DB11455181"), "Tanken")
            });
        }
    }
}