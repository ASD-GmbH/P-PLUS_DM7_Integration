using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Auswahllisten;
using DM7_PPLUS_Integration.Daten;
using Bare.Msg;
using Datum = DM7_PPLUS_Integration.Daten.Datum;

namespace DM7_PPLUS_Integration.Implementierung
{
    public class Demo_PPLUS_Handler : PPLUS_Handler
    {
        private readonly Random random = new Random();

        private Mitarbeiter Demo_Mitarbeiter()
        {
            return new Mitarbeiter(
                Guid.NewGuid(),
                new ReadOnlyCollection<DM7_Mandantenzugehörigkeit>(new List<DM7_Mandantenzugehörigkeit> { new DM7_Mandantenzugehörigkeit(Guid.NewGuid(), Zufaelliges_Datum(2014, 2016), null) }),
                Personalnummer(),
                Auswahllisten_1.Titel.Kein,
                Vorname(),
                Nachname(),
                null,
                "HEI",
                Zufaelliges_Datum(2000, 2015),
                Auswahllisten_1.Geschlecht.Maennlich,
                Auswahllisten_1.Konfession.Keine,
                Auswahllisten_1.Familienstand.Unbekannt,
                new ReadOnlyCollection<Qualifikation>(new List<Qualifikation>()),
                new ReadOnlyCollection<Kontakt>(new List<Kontakt>()));
        }
        private string Personalnummer() => random.Next(1000, 9999).ToString();
        private Datum Zufaelliges_Datum(int minJahr, int maxJahr) => new Datum(random.Next(1, 28), random.Next(1, 12), random.Next(minJahr, maxJahr));

        private string Nachname()
        {
            var namen = "Lane,Dane,Alonso,Nigel,Harlan,Benjamin,Rogelio,Archie,Maynard,Ulysses,Fritz,Tod,Cesar,Abe,Loren,Francis,Nicholas,Lorenzo,Wilbur,Hector".Split(',');
            return namen[random.Next(0, namen.Length)];
        }

        private string Vorname()
        {
            var namen = "David,Boyd,Ollie,Hilton,Francesco,Hugh,Milford,Kasey,Rupert,King,Huey,Von,Roscoe,Dino,Warner,Stewart,Peter,Dannie,Edgar,Eusebio".Split(',');
            return namen[random.Next(0, namen.Length)];
        }

        public Task<Query_Result> HandleQuery(Token token, Query query)
        {
            return Task.Run<Query_Result>(() =>
            {
                switch (query)
                {
                    case Mitarbeiter_abrufen_V1 _:
                    {
                        var mitarbeiter =
                            Enumerable.Range(0, 10)
                                .Select(_ => Demo_Mitarbeiter())
                                .ToList();
                        return Message_mapper.Mitarbeiterstammdaten_als_Message(new Stammdaten<Mitarbeiter>(mitarbeiter, new Datenstand(1)));
                    }
                }

                return new IO_Fehler($"'{query.GetType()}' wurde nicht behandelt");
            });
        }

        public Task<Token?> Authenticate(string user, string password)
        {
            return Task.FromResult<Token?>(Token.Demo());
        }
    }
}