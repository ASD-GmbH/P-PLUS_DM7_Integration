//////////////////////////////////////////////////
// Generated code by BareNET - 11.02.2021 15:11 //
//////////////////////////////////////////////////
using System;
using System.Linq;
using System.Collections.Generic;
namespace Bare.Msg
{
	public interface Query { /* Base type of union */ }

	public readonly struct Mitarbeiter_abrufen_V1 : Query
	{
		public byte[] Encoded() { return new byte[0]; }
		public static Mitarbeiter_abrufen_V1 Decoded(byte[] data) { return new Mitarbeiter_abrufen_V1(); }
		public static ValueTuple<Mitarbeiter_abrufen_V1, byte[]> Decode(byte[] data) { return new ValueTuple<Mitarbeiter_abrufen_V1, byte[]>(new Mitarbeiter_abrufen_V1(), data); }
	}

	public readonly struct Mitarbeiter_abrufen_ab_V1 : Query
	{
		public readonly Datenstand Value;
	
		public Mitarbeiter_abrufen_ab_V1(Datenstand value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return Value.Encoded();
		}
	
		public static Mitarbeiter_abrufen_ab_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Mitarbeiter_abrufen_ab_V1, byte[]> Decode(byte[] data)
		{
			var value = Datenstand.Decode(data);
			return new ValueTuple<Mitarbeiter_abrufen_ab_V1, byte[]>(
				new Mitarbeiter_abrufen_ab_V1(value.Item1),
				value.Item2);
		}
	}

	public interface Response { /* Base type of union */ }

	public readonly struct Query_Failed : Response
	{
		public readonly string Reason;
	
		public Query_Failed(string reason)
		{
			Reason = reason;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_string(Reason);
		}
	
		public static Query_Failed Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Query_Failed, byte[]> Decode(byte[] data)
		{
			var reason = BareNET.Bare.Decode_string(data);
			return new ValueTuple<Query_Failed, byte[]>(
				new Query_Failed(reason.Item1),
				reason.Item2);
		}
	}

	public readonly struct Mitarbeiterliste_V1 : Response
	{
		public readonly Mitarbeiter_V1[] Mitarbeiter;
		public readonly Datenstand Stand;
	
		public Mitarbeiterliste_V1(Mitarbeiter_V1[] mitarbeiter, Datenstand stand)
		{
			Mitarbeiter = mitarbeiter;
			Stand = stand;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_list(Mitarbeiter, MitarbeiterList => MitarbeiterList.Encoded())
				.Concat(Stand.Encoded())
				.ToArray();
		}
	
		public static Mitarbeiterliste_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Mitarbeiterliste_V1, byte[]> Decode(byte[] data)
		{
			var mitarbeiter = BareNET.Bare.Decode_list(data, dataList => Mitarbeiter_V1.Decode(dataList));
			var stand = Datenstand.Decode(mitarbeiter.Item2);
			return new ValueTuple<Mitarbeiterliste_V1, byte[]>(
				new Mitarbeiterliste_V1(mitarbeiter.Item1.ToArray(), stand.Item1),
				stand.Item2);
		}
	}

	public readonly struct Mitarbeiter_V1
	{
		public readonly UUID Id;
		public readonly DM7_Mandantenzugehörigkeit_V1[] Mandantenzugehörigkeiten;
		public readonly UUID Titel;
		public readonly string Vorname;
		public readonly string Nachname;
		public readonly Postanschrift_V1? Postanschrift;
		public readonly Datum? Geburtstag;
		public readonly UUID Familienstand;
		public readonly UUID Konfession;
		public readonly Qualifikation_V1[] Qualifikationen;
		public readonly string Handzeichen;
		public readonly string Personalnummer;
		public readonly UUID Geschlecht;
		public readonly Kontakt_V1[] Kontakte;
	
		public Mitarbeiter_V1(UUID id, DM7_Mandantenzugehörigkeit_V1[] mandantenzugehörigkeiten, UUID titel, string vorname, string nachname, Postanschrift_V1? postanschrift, Datum? geburtstag, UUID familienstand, UUID konfession, Qualifikation_V1[] qualifikationen, string handzeichen, string personalnummer, UUID geschlecht, Kontakt_V1[] kontakte)
		{
			Id = id;
			Mandantenzugehörigkeiten = mandantenzugehörigkeiten;
			Titel = titel;
			Vorname = vorname;
			Nachname = nachname;
			Postanschrift = postanschrift;
			Geburtstag = geburtstag;
			Familienstand = familienstand;
			Konfession = konfession;
			Qualifikationen = qualifikationen;
			Handzeichen = handzeichen;
			Personalnummer = personalnummer;
			Geschlecht = geschlecht;
			Kontakte = kontakte;
		}
	
		public byte[] Encoded()
		{
			return Id.Encoded()
				.Concat(BareNET.Bare.Encode_list(Mandantenzugehörigkeiten, MandantenzugehörigkeitenList => MandantenzugehörigkeitenList.Encoded()))
				.Concat(Titel.Encoded())
				.Concat(BareNET.Bare.Encode_string(Vorname))
				.Concat(BareNET.Bare.Encode_string(Nachname))
				.Concat(BareNET.Bare.Encode_optional<Postanschrift_V1>(Postanschrift, PostanschriftOpt => PostanschriftOpt.Encoded()))
				.Concat(BareNET.Bare.Encode_optional<Datum>(Geburtstag, GeburtstagOpt => GeburtstagOpt.Encoded()))
				.Concat(Familienstand.Encoded())
				.Concat(Konfession.Encoded())
				.Concat(BareNET.Bare.Encode_list(Qualifikationen, QualifikationenList => QualifikationenList.Encoded()))
				.Concat(BareNET.Bare.Encode_string(Handzeichen))
				.Concat(BareNET.Bare.Encode_string(Personalnummer))
				.Concat(Geschlecht.Encoded())
				.Concat(BareNET.Bare.Encode_list(Kontakte, KontakteList => KontakteList.Encoded()))
				.ToArray();
		}
	
		public static Mitarbeiter_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Mitarbeiter_V1, byte[]> Decode(byte[] data)
		{
			var id = UUID.Decode(data);
			var mandantenzugehörigkeiten = BareNET.Bare.Decode_list(id.Item2, idList => DM7_Mandantenzugehörigkeit_V1.Decode(idList));
			var titel = UUID.Decode(mandantenzugehörigkeiten.Item2);
			var vorname = BareNET.Bare.Decode_string(titel.Item2);
			var nachname = BareNET.Bare.Decode_string(vorname.Item2);
			var postanschrift = BareNET.Bare.Decode_optional(nachname.Item2, nachnameOpt => Postanschrift_V1.Decode(nachnameOpt));
			var geburtstag = BareNET.Bare.Decode_optional(postanschrift.Item2, postanschriftOpt => Datum.Decode(postanschriftOpt));
			var familienstand = UUID.Decode(geburtstag.Item2);
			var konfession = UUID.Decode(familienstand.Item2);
			var qualifikationen = BareNET.Bare.Decode_list(konfession.Item2, konfessionList => Qualifikation_V1.Decode(konfessionList));
			var handzeichen = BareNET.Bare.Decode_string(qualifikationen.Item2);
			var personalnummer = BareNET.Bare.Decode_string(handzeichen.Item2);
			var geschlecht = UUID.Decode(personalnummer.Item2);
			var kontakte = BareNET.Bare.Decode_list(geschlecht.Item2, geschlechtList => Kontakt_V1.Decode(geschlechtList));
			return new ValueTuple<Mitarbeiter_V1, byte[]>(
				new Mitarbeiter_V1(id.Item1, mandantenzugehörigkeiten.Item1.ToArray(), titel.Item1, vorname.Item1, nachname.Item1, postanschrift.Item1, geburtstag.Item1, familienstand.Item1, konfession.Item1, qualifikationen.Item1.ToArray(), handzeichen.Item1, personalnummer.Item1, geschlecht.Item1, kontakte.Item1.ToArray()),
				kontakte.Item2);
		}
	}

	public readonly struct DM7_Mandantenzugehörigkeit_V1
	{
		public readonly UUID Mandantid;
		public readonly Datum Gültigab;
		public readonly Datum? Gültigbis;
	
		public DM7_Mandantenzugehörigkeit_V1(UUID mandantId, Datum gültigAb, Datum? gültigBis)
		{
			Mandantid = mandantId;
			Gültigab = gültigAb;
			Gültigbis = gültigBis;
		}
	
		public byte[] Encoded()
		{
			return Mandantid.Encoded()
				.Concat(Gültigab.Encoded())
				.Concat(BareNET.Bare.Encode_optional<Datum>(Gültigbis, GültigbisOpt => GültigbisOpt.Encoded()))
				.ToArray();
		}
	
		public static DM7_Mandantenzugehörigkeit_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<DM7_Mandantenzugehörigkeit_V1, byte[]> Decode(byte[] data)
		{
			var mandantId = UUID.Decode(data);
			var gültigAb = Datum.Decode(mandantId.Item2);
			var gültigBis = BareNET.Bare.Decode_optional(gültigAb.Item2, gültigAbOpt => Datum.Decode(gültigAbOpt));
			return new ValueTuple<DM7_Mandantenzugehörigkeit_V1, byte[]>(
				new DM7_Mandantenzugehörigkeit_V1(mandantId.Item1, gültigAb.Item1, gültigBis.Item1),
				gültigBis.Item2);
		}
	}

	public readonly struct Postanschrift_V1
	{
		public readonly UUID Id;
		public readonly string Strasse;
		public readonly string Postleitzahl;
		public readonly string Ort;
		public readonly string Land;
		public readonly string Adresszusatz;
	
		public Postanschrift_V1(UUID id, string strasse, string postleitzahl, string ort, string land, string adresszusatz)
		{
			Id = id;
			Strasse = strasse;
			Postleitzahl = postleitzahl;
			Ort = ort;
			Land = land;
			Adresszusatz = adresszusatz;
		}
	
		public byte[] Encoded()
		{
			return Id.Encoded()
				.Concat(BareNET.Bare.Encode_string(Strasse))
				.Concat(BareNET.Bare.Encode_string(Postleitzahl))
				.Concat(BareNET.Bare.Encode_string(Ort))
				.Concat(BareNET.Bare.Encode_string(Land))
				.Concat(BareNET.Bare.Encode_string(Adresszusatz))
				.ToArray();
		}
	
		public static Postanschrift_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Postanschrift_V1, byte[]> Decode(byte[] data)
		{
			var id = UUID.Decode(data);
			var strasse = BareNET.Bare.Decode_string(id.Item2);
			var postleitzahl = BareNET.Bare.Decode_string(strasse.Item2);
			var ort = BareNET.Bare.Decode_string(postleitzahl.Item2);
			var land = BareNET.Bare.Decode_string(ort.Item2);
			var adresszusatz = BareNET.Bare.Decode_string(land.Item2);
			return new ValueTuple<Postanschrift_V1, byte[]>(
				new Postanschrift_V1(id.Item1, strasse.Item1, postleitzahl.Item1, ort.Item1, land.Item1, adresszusatz.Item1),
				adresszusatz.Item2);
		}
	}

	public readonly struct Qualifikation_V1
	{
		public readonly byte Stufe;
		public readonly string Bezeichnung;
	
		public Qualifikation_V1(byte stufe, string bezeichnung)
		{
			Stufe = stufe;
			Bezeichnung = bezeichnung;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_u8(Stufe)
				.Concat(BareNET.Bare.Encode_string(Bezeichnung))
				.ToArray();
		}
	
		public static Qualifikation_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Qualifikation_V1, byte[]> Decode(byte[] data)
		{
			var stufe = BareNET.Bare.Decode_u8(data);
			var bezeichnung = BareNET.Bare.Decode_string(stufe.Item2);
			return new ValueTuple<Qualifikation_V1, byte[]>(
				new Qualifikation_V1(stufe.Item1, bezeichnung.Item1),
				bezeichnung.Item2);
		}
	}

	public readonly struct Kontakt_V1
	{
		public readonly UUID Art;
		public readonly UUID Form;
		public readonly string Eintrag;
	
		public Kontakt_V1(UUID art, UUID form, string eintrag)
		{
			Art = art;
			Form = form;
			Eintrag = eintrag;
		}
	
		public byte[] Encoded()
		{
			return Art.Encoded()
				.Concat(Form.Encoded())
				.Concat(BareNET.Bare.Encode_string(Eintrag))
				.ToArray();
		}
	
		public static Kontakt_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Kontakt_V1, byte[]> Decode(byte[] data)
		{
			var art = UUID.Decode(data);
			var form = UUID.Decode(art.Item2);
			var eintrag = BareNET.Bare.Decode_string(form.Item2);
			return new ValueTuple<Kontakt_V1, byte[]>(
				new Kontakt_V1(art.Item1, form.Item1, eintrag.Item1),
				eintrag.Item2);
		}
	}

	public readonly struct UUID
	{
		public readonly byte[] Value;
	
		public UUID(byte[] value)
		{
			if (value.Length != 16) throw new ArgumentException("Length of list must be 16", nameof(value));
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_data_fixed_length(16, Value);
		}
	
		public static UUID Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<UUID, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_data_fixed_length(16, data);
			return new ValueTuple<UUID, byte[]>(
				new UUID(value.Item1),
				value.Item2);
		}
	}

	public readonly struct Datum
	{
		public readonly byte Tag;
		public readonly byte Monat;
		public readonly ushort Jahr;
	
		public Datum(byte tag, byte monat, ushort jahr)
		{
			Tag = tag;
			Monat = monat;
			Jahr = jahr;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_u8(Tag)
				.Concat(BareNET.Bare.Encode_u8(Monat))
				.Concat(BareNET.Bare.Encode_u16(Jahr))
				.ToArray();
		}
	
		public static Datum Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Datum, byte[]> Decode(byte[] data)
		{
			var tag = BareNET.Bare.Decode_u8(data);
			var monat = BareNET.Bare.Decode_u8(tag.Item2);
			var jahr = BareNET.Bare.Decode_u16(monat.Item2);
			return new ValueTuple<Datum, byte[]>(
				new Datum(tag.Item1, monat.Item1, jahr.Item1),
				jahr.Item2);
		}
	}

	public readonly struct Datenstand
	{
		public readonly ulong Value;
	
		public Datenstand(ulong value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_u64(Value);
		}
	
		public static Datenstand Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Datenstand, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_u64(data);
			return new ValueTuple<Datenstand, byte[]>(
				new Datenstand(value.Item1),
				value.Item2);
		}
	}

	public static class Encoding
	{
		private static readonly BareNET.Union<Query> _Query =
			BareNET.Union<Query>.Register()
				.With_Case<Mitarbeiter_abrufen_V1>(v => ((Mitarbeiter_abrufen_V1) v).Encoded(), d => { var decoded = Mitarbeiter_abrufen_V1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Mitarbeiter_abrufen_ab_V1>(v => ((Mitarbeiter_abrufen_ab_V1) v).Encoded(), d => { var decoded = Mitarbeiter_abrufen_ab_V1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] Query_Encoded(Query value)
		{
			return BareNET.Bare.Encode_union(value, _Query);
		}
		
		public static Query Query_Decoded(byte[] data)
		{
			return Decode_Query(data).Item1;
		}
		
		public static ValueTuple<Query, byte[]> Decode_Query(byte[] data)
		{
			return BareNET.Bare.Decode_union<Query>(data, _Query);
		}


		private static readonly BareNET.Union<Response> _Response =
			BareNET.Union<Response>.Register()
				.With_Case<Mitarbeiterliste_V1>(v => ((Mitarbeiterliste_V1) v).Encoded(), d => { var decoded = Mitarbeiterliste_V1.Decode(d); return new ValueTuple<Response, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Query_Failed>(v => ((Query_Failed) v).Encoded(), d => { var decoded = Query_Failed.Decode(d); return new ValueTuple<Response, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] Response_Encoded(Response value)
		{
			return BareNET.Bare.Encode_union(value, _Response);
		}
		
		public static Response Response_Decoded(byte[] data)
		{
			return Decode_Response(data).Item1;
		}
		
		public static ValueTuple<Response, byte[]> Decode_Response(byte[] data)
		{
			return BareNET.Bare.Decode_union<Response>(data, _Response);
		}
	}
}
