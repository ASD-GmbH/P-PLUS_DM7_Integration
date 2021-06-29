using System;
using System.Collections.Generic;
using System.Linq;
using DM7_PPLUS_Integration.Messages.DM7;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration.Implementierung.DM7
{
    public static class Message_mapper
    {
        public static List<Leistung> Als_Leistungen(LeistungenV1 leistungen) => leistungen.Value.Select(_ => new Leistung(AlsGuid(_.Id), _.Bezeichnung)).ToList();
        public static LeistungenV1 Von_Leistungen(IEnumerable<Leistung> leistungen) => new LeistungenV1(leistungen.Select(_ => new LeistungV1(AlsUUID(_.Id), _.Bezeichnung)).ToList());
        public static List<DM7_Mandant> Als_Mandanten(MandantenV1 mandanten) => mandanten.Value.Select(_ => new DM7_Mandant(AlsGuid(_.Id), _.Bezeichnung)).ToList();
        public static MandantenV1 Von_Mandanten(IEnumerable<DM7_Mandant> mandanten) => new MandantenV1(mandanten.Select(_ => new MandantV1(AlsUUID(_.Id), _.Bezeichnung)).ToList());

        private static Guid AlsGuid(UUID uuid) => new Guid(uuid.Value);
        private static UUID AlsUUID(Guid guid) => new UUID(guid.ToByteArray());
    }
}