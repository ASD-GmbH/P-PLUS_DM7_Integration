using System;
using System.Collections.Generic;
using System.Linq;
using DM7_PPLUS_Integration.Messages.DM7;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration.Implementierung.DM7
{
    public static class Message_mapper
    {
        public static List<Leistung> Als_Leistungen(Leistungen_V1 leistungen) => leistungen.Value.Select(_ => new Leistung(AlsGuid(_.Id), _.Bezeichnung)).ToList();
        public static Leistungen_V1 Von_Leistungen(IEnumerable<Leistung> leistungen) => new Leistungen_V1(leistungen.Select(_ => new Leistung_V1(AlsUUID(_.Id), _.Bezeichnung)).ToList());
        public static List<DM7_Mandant> Als_Mandanten(Mandanten_V1 mandanten) => mandanten.Value.Select(_ => new DM7_Mandant(AlsGuid(_.Id), _.Bezeichnung)).ToList();
        public static Mandanten_V1 Von_Mandanten(IEnumerable<DM7_Mandant> mandanten) => new Mandanten_V1(mandanten.Select(_ => new Mandant_V1(AlsUUID(_.Id), _.Bezeichnung)).ToList());

        private static Guid AlsGuid(UUID uuid) => new Guid(uuid.Value);
        private static UUID AlsUUID(Guid guid) => new UUID(guid.ToByteArray());
    }
}