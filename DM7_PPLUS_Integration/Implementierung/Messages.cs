//////////////////////////////////////////////////
// Generated code by BareNET - 21.06.2021 16:41 //
//////////////////////////////////////////////////
using System;
using System.Linq;
using System.Collections.Generic;
namespace DM7_PPLUS_Integration.Messages
{
	public readonly struct AuthenticationRequest
	{
		public readonly string User;
		public readonly string Password;
	
		public AuthenticationRequest(string user, string password)
		{
			User = user;
			Password = password;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_string(User)
				.Concat(BareNET.Bare.Encode_string(Password))
				.ToArray();
		}
	
		public static AuthenticationRequest Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<AuthenticationRequest, byte[]> Decode(byte[] data)
		{
			var user = BareNET.Bare.Decode_string(data);
			var password = BareNET.Bare.Decode_string(user.Item2);
			return new ValueTuple<AuthenticationRequest, byte[]>(
				new AuthenticationRequest(user.Item1, password.Item1),
				password.Item2);
		}
	}

	public interface AuthenticationResult { /* Base type of union */ }

	public readonly struct AuthenticationSucceeded : AuthenticationResult
	{
		public readonly int Token;
	
		public AuthenticationSucceeded(int token)
		{
			Token = token;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_i32(Token);
		}
	
		public static AuthenticationSucceeded Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<AuthenticationSucceeded, byte[]> Decode(byte[] data)
		{
			var token = BareNET.Bare.Decode_i32(data);
			return new ValueTuple<AuthenticationSucceeded, byte[]>(
				new AuthenticationSucceeded(token.Item1),
				token.Item2);
		}
	}

	public readonly struct AuthenticationFailed : AuthenticationResult
	{
		public readonly string Reason;
	
		public AuthenticationFailed(string reason)
		{
			Reason = reason;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_string(Reason);
		}
	
		public static AuthenticationFailed Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<AuthenticationFailed, byte[]> Decode(byte[] data)
		{
			var reason = BareNET.Bare.Decode_string(data);
			return new ValueTuple<AuthenticationFailed, byte[]>(
				new AuthenticationFailed(reason.Item1),
				reason.Item2);
		}
	}

	public readonly struct QueryMessage
	{
		public readonly int Token;
		public readonly byte[] Query;
	
		public QueryMessage(int token, byte[] query)
		{
			Token = token;
			Query = query;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_i32(Token)
				.Concat(BareNET.Bare.Encode_data(Query))
				.ToArray();
		}
	
		public static QueryMessage Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<QueryMessage, byte[]> Decode(byte[] data)
		{
			var token = BareNET.Bare.Decode_i32(data);
			var query = BareNET.Bare.Decode_data(token.Item2);
			return new ValueTuple<QueryMessage, byte[]>(
				new QueryMessage(token.Item1, query.Item1),
				query.Item2);
		}
	}

	public interface ResponseMessage { /* Base type of union */ }

	public readonly struct QuerySucceeded : ResponseMessage
	{
		public readonly byte[] Value;
	
		public QuerySucceeded(byte[] value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_data(Value);
		}
	
		public static QuerySucceeded Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<QuerySucceeded, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_data(data);
			return new ValueTuple<QuerySucceeded, byte[]>(
				new QuerySucceeded(value.Item1),
				value.Item2);
		}
	}

	public readonly struct QueryFailed : ResponseMessage
	{
		public readonly string Reason;
	
		public QueryFailed(string reason)
		{
			Reason = reason;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_string(Reason);
		}
	
		public static QueryFailed Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<QueryFailed, byte[]> Decode(byte[] data)
		{
			var reason = BareNET.Bare.Decode_string(data);
			return new ValueTuple<QueryFailed, byte[]>(
				new QueryFailed(reason.Item1),
				reason.Item2);
		}
	}

	public readonly struct CommandMessage
	{
		public readonly int Token;
		public readonly byte[] Command;
	
		public CommandMessage(int token, byte[] command)
		{
			Token = token;
			Command = command;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_i32(Token)
				.Concat(BareNET.Bare.Encode_data(Command))
				.ToArray();
		}
	
		public static CommandMessage Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<CommandMessage, byte[]> Decode(byte[] data)
		{
			var token = BareNET.Bare.Decode_i32(data);
			var command = BareNET.Bare.Decode_data(token.Item2);
			return new ValueTuple<CommandMessage, byte[]>(
				new CommandMessage(token.Item1, command.Item1),
				command.Item2);
		}
	}

	public interface CommandResponseMessage { /* Base type of union */ }

	public readonly struct CommandSucceeded : CommandResponseMessage
	{
		public readonly byte[] Value;
	
		public CommandSucceeded(byte[] value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_data(Value);
		}
	
		public static CommandSucceeded Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<CommandSucceeded, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_data(data);
			return new ValueTuple<CommandSucceeded, byte[]>(
				new CommandSucceeded(value.Item1),
				value.Item2);
		}
	}

	public readonly struct CommandFailed : CommandResponseMessage
	{
		public readonly string Reason;
	
		public CommandFailed(string reason)
		{
			Reason = reason;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_string(Reason);
		}
	
		public static CommandFailed Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<CommandFailed, byte[]> Decode(byte[] data)
		{
			var reason = BareNET.Bare.Decode_string(data);
			return new ValueTuple<CommandFailed, byte[]>(
				new CommandFailed(reason.Item1),
				reason.Item2);
		}
	}

	public readonly struct Capabilities
	{
		public readonly List<Capability> Value;
	
		public Capabilities(List<Capability> value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_list(Value, ValueList => Encoding.Capability_Encoded(ValueList));
		}
	
		public static Capabilities Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Capabilities, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_list(data, dataList => Encoding.Decode_Capability(dataList));
			return new ValueTuple<Capabilities, byte[]>(
				new Capabilities(value.Item1.ToList()),
				value.Item2);
		}
	}

	public enum Capability
	{
		MITARBEITER_V1,
		DIENSTE_V1,
		DIENSTBUCHUNGEN_V1,
		ABWESENHEITEN_V1,
		SOLL_IST_ABGLEICH_V1
	}

	public interface Query { /* Base type of union */ }

	public readonly struct MitarbeiterAbrufenV1 : Query
	{
		public byte[] Encoded() { return new byte[0]; }
		public static MitarbeiterAbrufenV1 Decoded(byte[] data) { return new MitarbeiterAbrufenV1(); }
		public static ValueTuple<MitarbeiterAbrufenV1, byte[]> Decode(byte[] data) { return new ValueTuple<MitarbeiterAbrufenV1, byte[]>(new MitarbeiterAbrufenV1(), data); }
	}

	public readonly struct MitarbeiterAbrufenAbV1 : Query
	{
		public readonly Datenstand Value;
	
		public MitarbeiterAbrufenAbV1(Datenstand value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return Value.Encoded();
		}
	
		public static MitarbeiterAbrufenAbV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<MitarbeiterAbrufenAbV1, byte[]> Decode(byte[] data)
		{
			var value = Datenstand.Decode(data);
			return new ValueTuple<MitarbeiterAbrufenAbV1, byte[]>(
				new MitarbeiterAbrufenAbV1(value.Item1),
				value.Item2);
		}
	}

	public readonly struct DiensteAbrufenV1 : Query
	{
		public byte[] Encoded() { return new byte[0]; }
		public static DiensteAbrufenV1 Decoded(byte[] data) { return new DiensteAbrufenV1(); }
		public static ValueTuple<DiensteAbrufenV1, byte[]> Decode(byte[] data) { return new ValueTuple<DiensteAbrufenV1, byte[]>(new DiensteAbrufenV1(), data); }
	}

	public readonly struct DiensteAbrufenAbV1 : Query
	{
		public readonly Datenstand Value;
	
		public DiensteAbrufenAbV1(Datenstand value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return Value.Encoded();
		}
	
		public static DiensteAbrufenAbV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<DiensteAbrufenAbV1, byte[]> Decode(byte[] data)
		{
			var value = Datenstand.Decode(data);
			return new ValueTuple<DiensteAbrufenAbV1, byte[]>(
				new DiensteAbrufenAbV1(value.Item1),
				value.Item2);
		}
	}

	public readonly struct DienstbuchungenZumStichtagV1 : Query
	{
		public readonly Datum Stichtag;
		public readonly UUID Mandant;
	
		public DienstbuchungenZumStichtagV1(Datum stichtag, UUID mandant)
		{
			Stichtag = stichtag;
			Mandant = mandant;
		}
	
		public byte[] Encoded()
		{
			return Stichtag.Encoded()
				.Concat(Mandant.Encoded())
				.ToArray();
		}
	
		public static DienstbuchungenZumStichtagV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<DienstbuchungenZumStichtagV1, byte[]> Decode(byte[] data)
		{
			var stichtag = Datum.Decode(data);
			var mandant = UUID.Decode(stichtag.Item2);
			return new ValueTuple<DienstbuchungenZumStichtagV1, byte[]>(
				new DienstbuchungenZumStichtagV1(stichtag.Item1, mandant.Item1),
				mandant.Item2);
		}
	}

	public readonly struct AbwesenheitenZumStichtagV1 : Query
	{
		public readonly Datum Stichtag;
		public readonly UUID Mandant;
	
		public AbwesenheitenZumStichtagV1(Datum stichtag, UUID mandant)
		{
			Stichtag = stichtag;
			Mandant = mandant;
		}
	
		public byte[] Encoded()
		{
			return Stichtag.Encoded()
				.Concat(Mandant.Encoded())
				.ToArray();
		}
	
		public static AbwesenheitenZumStichtagV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<AbwesenheitenZumStichtagV1, byte[]> Decode(byte[] data)
		{
			var stichtag = Datum.Decode(data);
			var mandant = UUID.Decode(stichtag.Item2);
			return new ValueTuple<AbwesenheitenZumStichtagV1, byte[]>(
				new AbwesenheitenZumStichtagV1(stichtag.Item1, mandant.Item1),
				mandant.Item2);
		}
	}

	public interface QueryResult { /* Base type of union */ }

	public readonly struct IOFehler : QueryResult, CommandResult
	{
		public readonly string Reason;
	
		public IOFehler(string reason)
		{
			Reason = reason;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_string(Reason);
		}
	
		public static IOFehler Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<IOFehler, byte[]> Decode(byte[] data)
		{
			var reason = BareNET.Bare.Decode_string(data);
			return new ValueTuple<IOFehler, byte[]>(
				new IOFehler(reason.Item1),
				reason.Item2);
		}
	}

	public readonly struct MitarbeiterlisteV1 : QueryResult
	{
		public readonly List<MitarbeiterV1> Mitarbeiter;
		public readonly Datenstand Stand;
	
		public MitarbeiterlisteV1(List<MitarbeiterV1> mitarbeiter, Datenstand stand)
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
	
		public static MitarbeiterlisteV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<MitarbeiterlisteV1, byte[]> Decode(byte[] data)
		{
			var mitarbeiter = BareNET.Bare.Decode_list(data, dataList => MitarbeiterV1.Decode(dataList));
			var stand = Datenstand.Decode(mitarbeiter.Item2);
			return new ValueTuple<MitarbeiterlisteV1, byte[]>(
				new MitarbeiterlisteV1(mitarbeiter.Item1.ToList(), stand.Item1),
				stand.Item2);
		}
	}

	public readonly struct DiensteV1 : QueryResult
	{
		public readonly List<DienstV1> Dienste;
		public readonly Datenstand Stand;
	
		public DiensteV1(List<DienstV1> dienste, Datenstand stand)
		{
			Dienste = dienste;
			Stand = stand;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_list(Dienste, DiensteList => DiensteList.Encoded())
				.Concat(Stand.Encoded())
				.ToArray();
		}
	
		public static DiensteV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<DiensteV1, byte[]> Decode(byte[] data)
		{
			var dienste = BareNET.Bare.Decode_list(data, dataList => DienstV1.Decode(dataList));
			var stand = Datenstand.Decode(dienste.Item2);
			return new ValueTuple<DiensteV1, byte[]>(
				new DiensteV1(dienste.Item1.ToList(), stand.Item1),
				stand.Item2);
		}
	}

	public readonly struct DienstbuchungenV1 : QueryResult
	{
		public readonly List<DienstbuchungV1> Value;
	
		public DienstbuchungenV1(List<DienstbuchungV1> value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_list(Value, ValueList => ValueList.Encoded());
		}
	
		public static DienstbuchungenV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<DienstbuchungenV1, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_list(data, dataList => DienstbuchungV1.Decode(dataList));
			return new ValueTuple<DienstbuchungenV1, byte[]>(
				new DienstbuchungenV1(value.Item1.ToList()),
				value.Item2);
		}
	}

	public readonly struct AbwesenheitenV1 : QueryResult
	{
		public readonly List<AbwesenheitV1> Value;
	
		public AbwesenheitenV1(List<AbwesenheitV1> value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_list(Value, ValueList => ValueList.Encoded());
		}
	
		public static AbwesenheitenV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<AbwesenheitenV1, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_list(data, dataList => AbwesenheitV1.Decode(dataList));
			return new ValueTuple<AbwesenheitenV1, byte[]>(
				new AbwesenheitenV1(value.Item1.ToList()),
				value.Item2);
		}
	}

	public interface Command { /* Base type of union */ }

	public interface CommandResult { /* Base type of union */ }

	public readonly struct SollIstAbgleichFreigebenV1 : Command
	{
		public readonly SollIstAbgleichV1 Value;
	
		public SollIstAbgleichFreigebenV1(SollIstAbgleichV1 value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return Value.Encoded();
		}
	
		public static SollIstAbgleichFreigebenV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<SollIstAbgleichFreigebenV1, byte[]> Decode(byte[] data)
		{
			var value = SollIstAbgleichV1.Decode(data);
			return new ValueTuple<SollIstAbgleichFreigebenV1, byte[]>(
				new SollIstAbgleichFreigebenV1(value.Item1),
				value.Item2);
		}
	}

	public readonly struct MitarbeiterV1
	{
		public readonly UUID Id;
		public readonly List<DM7MandantenzugehoerigkeitV1> Mandantenzugehoerigkeiten;
		public readonly UUID Titel;
		public readonly string Vorname;
		public readonly string Nachname;
		public readonly PostanschriftV1? Postanschrift;
		public readonly Datum? Geburtstag;
		public readonly UUID Familienstand;
		public readonly UUID Konfession;
		public readonly List<QualifikationV1> Qualifikationen;
		public readonly string Handzeichen;
		public readonly string Personalnummer;
		public readonly UUID Geschlecht;
		public readonly List<KontaktV1> Kontakte;
	
		public MitarbeiterV1(UUID id, List<DM7MandantenzugehoerigkeitV1> mandantenzugehoerigkeiten, UUID titel, string vorname, string nachname, PostanschriftV1? postanschrift, Datum? geburtstag, UUID familienstand, UUID konfession, List<QualifikationV1> qualifikationen, string handzeichen, string personalnummer, UUID geschlecht, List<KontaktV1> kontakte)
		{
			Id = id;
			Mandantenzugehoerigkeiten = mandantenzugehoerigkeiten;
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
				.Concat(BareNET.Bare.Encode_list(Mandantenzugehoerigkeiten, MandantenzugehoerigkeitenList => MandantenzugehoerigkeitenList.Encoded()))
				.Concat(Titel.Encoded())
				.Concat(BareNET.Bare.Encode_string(Vorname))
				.Concat(BareNET.Bare.Encode_string(Nachname))
				.Concat(BareNET.Bare.Encode_optional<PostanschriftV1>(Postanschrift, PostanschriftOpt => PostanschriftOpt.Encoded()))
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
	
		public static MitarbeiterV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<MitarbeiterV1, byte[]> Decode(byte[] data)
		{
			var id = UUID.Decode(data);
			var mandantenzugehoerigkeiten = BareNET.Bare.Decode_list(id.Item2, idList => DM7MandantenzugehoerigkeitV1.Decode(idList));
			var titel = UUID.Decode(mandantenzugehoerigkeiten.Item2);
			var vorname = BareNET.Bare.Decode_string(titel.Item2);
			var nachname = BareNET.Bare.Decode_string(vorname.Item2);
			var postanschrift = BareNET.Bare.Decode_optional(nachname.Item2, nachnameOpt => PostanschriftV1.Decode(nachnameOpt));
			var geburtstag = BareNET.Bare.Decode_optional(postanschrift.Item2, postanschriftOpt => Datum.Decode(postanschriftOpt));
			var familienstand = UUID.Decode(geburtstag.Item2);
			var konfession = UUID.Decode(familienstand.Item2);
			var qualifikationen = BareNET.Bare.Decode_list(konfession.Item2, konfessionList => QualifikationV1.Decode(konfessionList));
			var handzeichen = BareNET.Bare.Decode_string(qualifikationen.Item2);
			var personalnummer = BareNET.Bare.Decode_string(handzeichen.Item2);
			var geschlecht = UUID.Decode(personalnummer.Item2);
			var kontakte = BareNET.Bare.Decode_list(geschlecht.Item2, geschlechtList => KontaktV1.Decode(geschlechtList));
			return new ValueTuple<MitarbeiterV1, byte[]>(
				new MitarbeiterV1(id.Item1, mandantenzugehoerigkeiten.Item1.ToList(), titel.Item1, vorname.Item1, nachname.Item1, postanschrift.Item1, geburtstag.Item1, familienstand.Item1, konfession.Item1, qualifikationen.Item1.ToList(), handzeichen.Item1, personalnummer.Item1, geschlecht.Item1, kontakte.Item1.ToList()),
				kontakte.Item2);
		}
	}

	public readonly struct DM7MandantenzugehoerigkeitV1
	{
		public readonly UUID MandantId;
		public readonly Datum GueltigAb;
		public readonly Datum? GueltigBis;
	
		public DM7MandantenzugehoerigkeitV1(UUID mandantId, Datum gueltigAb, Datum? gueltigBis)
		{
			MandantId = mandantId;
			GueltigAb = gueltigAb;
			GueltigBis = gueltigBis;
		}
	
		public byte[] Encoded()
		{
			return MandantId.Encoded()
				.Concat(GueltigAb.Encoded())
				.Concat(BareNET.Bare.Encode_optional<Datum>(GueltigBis, GueltigBisOpt => GueltigBisOpt.Encoded()))
				.ToArray();
		}
	
		public static DM7MandantenzugehoerigkeitV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<DM7MandantenzugehoerigkeitV1, byte[]> Decode(byte[] data)
		{
			var mandantId = UUID.Decode(data);
			var gueltigAb = Datum.Decode(mandantId.Item2);
			var gueltigBis = BareNET.Bare.Decode_optional(gueltigAb.Item2, gueltigAbOpt => Datum.Decode(gueltigAbOpt));
			return new ValueTuple<DM7MandantenzugehoerigkeitV1, byte[]>(
				new DM7MandantenzugehoerigkeitV1(mandantId.Item1, gueltigAb.Item1, gueltigBis.Item1),
				gueltigBis.Item2);
		}
	}

	public readonly struct PostanschriftV1
	{
		public readonly UUID Id;
		public readonly string Strasse;
		public readonly string Postleitzahl;
		public readonly string Ort;
		public readonly string Land;
	
		public PostanschriftV1(UUID id, string strasse, string postleitzahl, string ort, string land)
		{
			Id = id;
			Strasse = strasse;
			Postleitzahl = postleitzahl;
			Ort = ort;
			Land = land;
		}
	
		public byte[] Encoded()
		{
			return Id.Encoded()
				.Concat(BareNET.Bare.Encode_string(Strasse))
				.Concat(BareNET.Bare.Encode_string(Postleitzahl))
				.Concat(BareNET.Bare.Encode_string(Ort))
				.Concat(BareNET.Bare.Encode_string(Land))
				.ToArray();
		}
	
		public static PostanschriftV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<PostanschriftV1, byte[]> Decode(byte[] data)
		{
			var id = UUID.Decode(data);
			var strasse = BareNET.Bare.Decode_string(id.Item2);
			var postleitzahl = BareNET.Bare.Decode_string(strasse.Item2);
			var ort = BareNET.Bare.Decode_string(postleitzahl.Item2);
			var land = BareNET.Bare.Decode_string(ort.Item2);
			return new ValueTuple<PostanschriftV1, byte[]>(
				new PostanschriftV1(id.Item1, strasse.Item1, postleitzahl.Item1, ort.Item1, land.Item1),
				land.Item2);
		}
	}

	public readonly struct QualifikationV1
	{
		public readonly byte Stufe;
		public readonly string Bezeichnung;
		public readonly Datum GueltigAb;
		public readonly Datum? GueltigBis;
	
		public QualifikationV1(byte stufe, string bezeichnung, Datum gueltigAb, Datum? gueltigBis)
		{
			Stufe = stufe;
			Bezeichnung = bezeichnung;
			GueltigAb = gueltigAb;
			GueltigBis = gueltigBis;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_u8(Stufe)
				.Concat(BareNET.Bare.Encode_string(Bezeichnung))
				.Concat(GueltigAb.Encoded())
				.Concat(BareNET.Bare.Encode_optional<Datum>(GueltigBis, GueltigBisOpt => GueltigBisOpt.Encoded()))
				.ToArray();
		}
	
		public static QualifikationV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<QualifikationV1, byte[]> Decode(byte[] data)
		{
			var stufe = BareNET.Bare.Decode_u8(data);
			var bezeichnung = BareNET.Bare.Decode_string(stufe.Item2);
			var gueltigAb = Datum.Decode(bezeichnung.Item2);
			var gueltigBis = BareNET.Bare.Decode_optional(gueltigAb.Item2, gueltigAbOpt => Datum.Decode(gueltigAbOpt));
			return new ValueTuple<QualifikationV1, byte[]>(
				new QualifikationV1(stufe.Item1, bezeichnung.Item1, gueltigAb.Item1, gueltigBis.Item1),
				gueltigBis.Item2);
		}
	}

	public readonly struct KontaktV1
	{
		public readonly UUID Art;
		public readonly UUID Form;
		public readonly string Eintrag;
		public readonly bool Hauptkontakt;
	
		public KontaktV1(UUID art, UUID form, string eintrag, bool hauptkontakt)
		{
			Art = art;
			Form = form;
			Eintrag = eintrag;
			Hauptkontakt = hauptkontakt;
		}
	
		public byte[] Encoded()
		{
			return Art.Encoded()
				.Concat(Form.Encoded())
				.Concat(BareNET.Bare.Encode_string(Eintrag))
				.Concat(BareNET.Bare.Encode_bool(Hauptkontakt))
				.ToArray();
		}
	
		public static KontaktV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<KontaktV1, byte[]> Decode(byte[] data)
		{
			var art = UUID.Decode(data);
			var form = UUID.Decode(art.Item2);
			var eintrag = BareNET.Bare.Decode_string(form.Item2);
			var hauptkontakt = BareNET.Bare.Decode_bool(eintrag.Item2);
			return new ValueTuple<KontaktV1, byte[]>(
				new KontaktV1(art.Item1, form.Item1, eintrag.Item1, hauptkontakt.Item1),
				hauptkontakt.Item2);
		}
	}

	public readonly struct DienstV1
	{
		public readonly ulong Id;
		public readonly List<DM7MandantenzugehoerigkeitV1> Mandantenzugehoerigkeiten;
		public readonly string Kurzbezeichnung;
		public readonly string Bezeichnung;
		public readonly DienstGueltigkeitV1 GueltigAn;
		public readonly bool Geloescht;
	
		public DienstV1(ulong id, List<DM7MandantenzugehoerigkeitV1> mandantenzugehoerigkeiten, string kurzbezeichnung, string bezeichnung, DienstGueltigkeitV1 gueltigAn, bool geloescht)
		{
			Id = id;
			Mandantenzugehoerigkeiten = mandantenzugehoerigkeiten;
			Kurzbezeichnung = kurzbezeichnung;
			Bezeichnung = bezeichnung;
			GueltigAn = gueltigAn;
			Geloescht = geloescht;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_uint(Id)
				.Concat(BareNET.Bare.Encode_list(Mandantenzugehoerigkeiten, MandantenzugehoerigkeitenList => MandantenzugehoerigkeitenList.Encoded()))
				.Concat(BareNET.Bare.Encode_string(Kurzbezeichnung))
				.Concat(BareNET.Bare.Encode_string(Bezeichnung))
				.Concat(GueltigAn.Encoded())
				.Concat(BareNET.Bare.Encode_bool(Geloescht))
				.ToArray();
		}
	
		public static DienstV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<DienstV1, byte[]> Decode(byte[] data)
		{
			var id = BareNET.Bare.Decode_uint(data);
			var mandantenzugehoerigkeiten = BareNET.Bare.Decode_list(id.Item2, idList => DM7MandantenzugehoerigkeitV1.Decode(idList));
			var kurzbezeichnung = BareNET.Bare.Decode_string(mandantenzugehoerigkeiten.Item2);
			var bezeichnung = BareNET.Bare.Decode_string(kurzbezeichnung.Item2);
			var gueltigAn = DienstGueltigkeitV1.Decode(bezeichnung.Item2);
			var geloescht = BareNET.Bare.Decode_bool(gueltigAn.Item2);
			return new ValueTuple<DienstV1, byte[]>(
				new DienstV1(id.Item1, mandantenzugehoerigkeiten.Item1.ToList(), kurzbezeichnung.Item1, bezeichnung.Item1, gueltigAn.Item1, geloescht.Item1),
				geloescht.Item2);
		}
	}

	public readonly struct DienstGueltigkeitV1
	{
		public readonly bool Montag;
		public readonly bool Dienstag;
		public readonly bool Mittwoch;
		public readonly bool Donnerstag;
		public readonly bool Freitag;
		public readonly bool Samstag;
		public readonly bool Sonntag;
		public readonly bool Feiertags;
	
		public DienstGueltigkeitV1(bool montag, bool dienstag, bool mittwoch, bool donnerstag, bool freitag, bool samstag, bool sonntag, bool feiertags)
		{
			Montag = montag;
			Dienstag = dienstag;
			Mittwoch = mittwoch;
			Donnerstag = donnerstag;
			Freitag = freitag;
			Samstag = samstag;
			Sonntag = sonntag;
			Feiertags = feiertags;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_bool(Montag)
				.Concat(BareNET.Bare.Encode_bool(Dienstag))
				.Concat(BareNET.Bare.Encode_bool(Mittwoch))
				.Concat(BareNET.Bare.Encode_bool(Donnerstag))
				.Concat(BareNET.Bare.Encode_bool(Freitag))
				.Concat(BareNET.Bare.Encode_bool(Samstag))
				.Concat(BareNET.Bare.Encode_bool(Sonntag))
				.Concat(BareNET.Bare.Encode_bool(Feiertags))
				.ToArray();
		}
	
		public static DienstGueltigkeitV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<DienstGueltigkeitV1, byte[]> Decode(byte[] data)
		{
			var montag = BareNET.Bare.Decode_bool(data);
			var dienstag = BareNET.Bare.Decode_bool(montag.Item2);
			var mittwoch = BareNET.Bare.Decode_bool(dienstag.Item2);
			var donnerstag = BareNET.Bare.Decode_bool(mittwoch.Item2);
			var freitag = BareNET.Bare.Decode_bool(donnerstag.Item2);
			var samstag = BareNET.Bare.Decode_bool(freitag.Item2);
			var sonntag = BareNET.Bare.Decode_bool(samstag.Item2);
			var feiertags = BareNET.Bare.Decode_bool(sonntag.Item2);
			return new ValueTuple<DienstGueltigkeitV1, byte[]>(
				new DienstGueltigkeitV1(montag.Item1, dienstag.Item1, mittwoch.Item1, donnerstag.Item1, freitag.Item1, samstag.Item1, sonntag.Item1, feiertags.Item1),
				feiertags.Item2);
		}
	}

	public readonly struct DienstbuchungV1
	{
		public readonly UUID Mitarbeiter;
		public readonly long Dienst;
		public readonly Uhrzeit BeginntUm;
	
		public DienstbuchungV1(UUID mitarbeiter, long dienst, Uhrzeit beginntUm)
		{
			Mitarbeiter = mitarbeiter;
			Dienst = dienst;
			BeginntUm = beginntUm;
		}
	
		public byte[] Encoded()
		{
			return Mitarbeiter.Encoded()
				.Concat(BareNET.Bare.Encode_int(Dienst))
				.Concat(BeginntUm.Encoded())
				.ToArray();
		}
	
		public static DienstbuchungV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<DienstbuchungV1, byte[]> Decode(byte[] data)
		{
			var mitarbeiter = UUID.Decode(data);
			var dienst = BareNET.Bare.Decode_int(mitarbeiter.Item2);
			var beginntUm = Uhrzeit.Decode(dienst.Item2);
			return new ValueTuple<DienstbuchungV1, byte[]>(
				new DienstbuchungV1(mitarbeiter.Item1, dienst.Item1, beginntUm.Item1),
				beginntUm.Item2);
		}
	}

	public readonly struct AbwesenheitV1
	{
		public readonly UUID Mitarbeiter;
		public readonly Zeitpunkt AbwesendAb;
		public readonly Zeitpunkt VorraussichtlichWiederVerfuegbarAb;
		public readonly string Grund;
		public readonly AbwesenheitsartV1 Art;
	
		public AbwesenheitV1(UUID mitarbeiter, Zeitpunkt abwesendAb, Zeitpunkt vorraussichtlichWiederVerfuegbarAb, string grund, AbwesenheitsartV1 art)
		{
			Mitarbeiter = mitarbeiter;
			AbwesendAb = abwesendAb;
			VorraussichtlichWiederVerfuegbarAb = vorraussichtlichWiederVerfuegbarAb;
			Grund = grund;
			Art = art;
		}
	
		public byte[] Encoded()
		{
			return Mitarbeiter.Encoded()
				.Concat(AbwesendAb.Encoded())
				.Concat(VorraussichtlichWiederVerfuegbarAb.Encoded())
				.Concat(BareNET.Bare.Encode_string(Grund))
				.Concat(Encoding.AbwesenheitsartV1_Encoded(Art))
				.ToArray();
		}
	
		public static AbwesenheitV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<AbwesenheitV1, byte[]> Decode(byte[] data)
		{
			var mitarbeiter = UUID.Decode(data);
			var abwesendAb = Zeitpunkt.Decode(mitarbeiter.Item2);
			var vorraussichtlichWiederVerfuegbarAb = Zeitpunkt.Decode(abwesendAb.Item2);
			var grund = BareNET.Bare.Decode_string(vorraussichtlichWiederVerfuegbarAb.Item2);
			var art = Encoding.Decode_AbwesenheitsartV1(grund.Item2);
			return new ValueTuple<AbwesenheitV1, byte[]>(
				new AbwesenheitV1(mitarbeiter.Item1, abwesendAb.Item1, vorraussichtlichWiederVerfuegbarAb.Item1, grund.Item1, art.Item1),
				art.Item2);
		}
	}

	public enum AbwesenheitsartV1
	{
		FEHLZEIT,
		ANDERSWEITIG_VERPLANT
	}

	public readonly struct SollIstAbgleichV1
	{
		public readonly Datum Datum;
		public readonly List<UngeplanteTourV1> UngeplanteTourenOhneTourenstamm;
		public readonly List<GeplanteTourV1> GeplanteTouren;
		public readonly List<NichtGefahreneTourV1> NichtGefahreneTouren;
	
		public SollIstAbgleichV1(Datum datum, List<UngeplanteTourV1> ungeplanteTourenOhneTourenstamm, List<GeplanteTourV1> geplanteTouren, List<NichtGefahreneTourV1> nichtGefahreneTouren)
		{
			Datum = datum;
			UngeplanteTourenOhneTourenstamm = ungeplanteTourenOhneTourenstamm;
			GeplanteTouren = geplanteTouren;
			NichtGefahreneTouren = nichtGefahreneTouren;
		}
	
		public byte[] Encoded()
		{
			return Datum.Encoded()
				.Concat(BareNET.Bare.Encode_list(UngeplanteTourenOhneTourenstamm, UngeplanteTourenOhneTourenstammList => UngeplanteTourenOhneTourenstammList.Encoded()))
				.Concat(BareNET.Bare.Encode_list(GeplanteTouren, GeplanteTourenList => GeplanteTourenList.Encoded()))
				.Concat(BareNET.Bare.Encode_list(NichtGefahreneTouren, NichtGefahreneTourenList => NichtGefahreneTourenList.Encoded()))
				.ToArray();
		}
	
		public static SollIstAbgleichV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<SollIstAbgleichV1, byte[]> Decode(byte[] data)
		{
			var datum = Datum.Decode(data);
			var ungeplanteTourenOhneTourenstamm = BareNET.Bare.Decode_list(datum.Item2, datumList => UngeplanteTourV1.Decode(datumList));
			var geplanteTouren = BareNET.Bare.Decode_list(ungeplanteTourenOhneTourenstamm.Item2, ungeplanteTourenOhneTourenstammList => GeplanteTourV1.Decode(ungeplanteTourenOhneTourenstammList));
			var nichtGefahreneTouren = BareNET.Bare.Decode_list(geplanteTouren.Item2, geplanteTourenList => NichtGefahreneTourV1.Decode(geplanteTourenList));
			return new ValueTuple<SollIstAbgleichV1, byte[]>(
				new SollIstAbgleichV1(datum.Item1, ungeplanteTourenOhneTourenstamm.Item1.ToList(), geplanteTouren.Item1.ToList(), nichtGefahreneTouren.Item1.ToList()),
				nichtGefahreneTouren.Item2);
		}
	}

	public readonly struct UngeplanteTourV1
	{
		public readonly UUID Mitarbeiter;
		public readonly UUID Mandant;
		public readonly List<EinsatzV1> Einsaetze;
	
		public UngeplanteTourV1(UUID mitarbeiter, UUID mandant, List<EinsatzV1> einsaetze)
		{
			Mitarbeiter = mitarbeiter;
			Mandant = mandant;
			Einsaetze = einsaetze;
		}
	
		public byte[] Encoded()
		{
			return Mitarbeiter.Encoded()
				.Concat(Mandant.Encoded())
				.Concat(BareNET.Bare.Encode_list(Einsaetze, EinsaetzeList => EinsaetzeList.Encoded()))
				.ToArray();
		}
	
		public static UngeplanteTourV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<UngeplanteTourV1, byte[]> Decode(byte[] data)
		{
			var mitarbeiter = UUID.Decode(data);
			var mandant = UUID.Decode(mitarbeiter.Item2);
			var einsaetze = BareNET.Bare.Decode_list(mandant.Item2, mandantList => EinsatzV1.Decode(mandantList));
			return new ValueTuple<UngeplanteTourV1, byte[]>(
				new UngeplanteTourV1(mitarbeiter.Item1, mandant.Item1, einsaetze.Item1.ToList()),
				einsaetze.Item2);
		}
	}

	public readonly struct GeplanteTourV1
	{
		public readonly UUID Mitarbeiter;
		public readonly UUID Mandant;
		public readonly long Dienst;
		public readonly List<EinsatzV1> Einsaetze;
	
		public GeplanteTourV1(UUID mitarbeiter, UUID mandant, long dienst, List<EinsatzV1> einsaetze)
		{
			Mitarbeiter = mitarbeiter;
			Mandant = mandant;
			Dienst = dienst;
			Einsaetze = einsaetze;
		}
	
		public byte[] Encoded()
		{
			return Mitarbeiter.Encoded()
				.Concat(Mandant.Encoded())
				.Concat(BareNET.Bare.Encode_int(Dienst))
				.Concat(BareNET.Bare.Encode_list(Einsaetze, EinsaetzeList => EinsaetzeList.Encoded()))
				.ToArray();
		}
	
		public static GeplanteTourV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<GeplanteTourV1, byte[]> Decode(byte[] data)
		{
			var mitarbeiter = UUID.Decode(data);
			var mandant = UUID.Decode(mitarbeiter.Item2);
			var dienst = BareNET.Bare.Decode_int(mandant.Item2);
			var einsaetze = BareNET.Bare.Decode_list(dienst.Item2, dienstList => EinsatzV1.Decode(dienstList));
			return new ValueTuple<GeplanteTourV1, byte[]>(
				new GeplanteTourV1(mitarbeiter.Item1, mandant.Item1, dienst.Item1, einsaetze.Item1.ToList()),
				einsaetze.Item2);
		}
	}

	public readonly struct NichtGefahreneTourV1
	{
		public readonly UUID Mitarbeiter;
		public readonly UUID Mandant;
		public readonly long Dienst;
	
		public NichtGefahreneTourV1(UUID mitarbeiter, UUID mandant, long dienst)
		{
			Mitarbeiter = mitarbeiter;
			Mandant = mandant;
			Dienst = dienst;
		}
	
		public byte[] Encoded()
		{
			return Mitarbeiter.Encoded()
				.Concat(Mandant.Encoded())
				.Concat(BareNET.Bare.Encode_int(Dienst))
				.ToArray();
		}
	
		public static NichtGefahreneTourV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<NichtGefahreneTourV1, byte[]> Decode(byte[] data)
		{
			var mitarbeiter = UUID.Decode(data);
			var mandant = UUID.Decode(mitarbeiter.Item2);
			var dienst = BareNET.Bare.Decode_int(mandant.Item2);
			return new ValueTuple<NichtGefahreneTourV1, byte[]>(
				new NichtGefahreneTourV1(mitarbeiter.Item1, mandant.Item1, dienst.Item1),
				dienst.Item2);
		}
	}

	public readonly struct EinsatzV1
	{
		public readonly RelativeZeit Beginn;
		public readonly ulong DauerInMinuten;
		public readonly ulong AnfahrtsdauerInMinuten;
		public readonly ulong AbfahrtsdauerInMinuten;
		public readonly EinsatzartV1 Art;
	
		public EinsatzV1(RelativeZeit beginn, ulong dauerInMinuten, ulong anfahrtsdauerInMinuten, ulong abfahrtsdauerInMinuten, EinsatzartV1 art)
		{
			Beginn = beginn;
			DauerInMinuten = dauerInMinuten;
			AnfahrtsdauerInMinuten = anfahrtsdauerInMinuten;
			AbfahrtsdauerInMinuten = abfahrtsdauerInMinuten;
			Art = art;
		}
	
		public byte[] Encoded()
		{
			return Beginn.Encoded()
				.Concat(BareNET.Bare.Encode_uint(DauerInMinuten))
				.Concat(BareNET.Bare.Encode_uint(AnfahrtsdauerInMinuten))
				.Concat(BareNET.Bare.Encode_uint(AbfahrtsdauerInMinuten))
				.Concat(Encoding.EinsatzartV1_Encoded(Art))
				.ToArray();
		}
	
		public static EinsatzV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<EinsatzV1, byte[]> Decode(byte[] data)
		{
			var beginn = RelativeZeit.Decode(data);
			var dauerInMinuten = BareNET.Bare.Decode_uint(beginn.Item2);
			var anfahrtsdauerInMinuten = BareNET.Bare.Decode_uint(dauerInMinuten.Item2);
			var abfahrtsdauerInMinuten = BareNET.Bare.Decode_uint(anfahrtsdauerInMinuten.Item2);
			var art = Encoding.Decode_EinsatzartV1(abfahrtsdauerInMinuten.Item2);
			return new ValueTuple<EinsatzV1, byte[]>(
				new EinsatzV1(beginn.Item1, dauerInMinuten.Item1, anfahrtsdauerInMinuten.Item1, abfahrtsdauerInMinuten.Item1, art.Item1),
				art.Item2);
		}
	}

	public interface EinsatzartV1 { /* Base type of union */ }

	public readonly struct KlientEinsatzV1 : EinsatzartV1
	{
		public byte[] Encoded() { return new byte[0]; }
		public static KlientEinsatzV1 Decoded(byte[] data) { return new KlientEinsatzV1(); }
		public static ValueTuple<KlientEinsatzV1, byte[]> Decode(byte[] data) { return new ValueTuple<KlientEinsatzV1, byte[]>(new KlientEinsatzV1(), data); }
	}

	public readonly struct SonstigeZeitV1 : EinsatzartV1
	{
		public readonly UUID Leistung;
	
		public SonstigeZeitV1(UUID leistung)
		{
			Leistung = leistung;
		}
	
		public byte[] Encoded()
		{
			return Leistung.Encoded();
		}
	
		public static SonstigeZeitV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<SonstigeZeitV1, byte[]> Decode(byte[] data)
		{
			var leistung = UUID.Decode(data);
			return new ValueTuple<SonstigeZeitV1, byte[]>(
				new SonstigeZeitV1(leistung.Item1),
				leistung.Item2);
		}
	}

	public readonly struct PauseV1 : EinsatzartV1
	{
		public byte[] Encoded() { return new byte[0]; }
		public static PauseV1 Decoded(byte[] data) { return new PauseV1(); }
		public static ValueTuple<PauseV1, byte[]> Decode(byte[] data) { return new ValueTuple<PauseV1, byte[]>(new PauseV1(), data); }
	}

	public readonly struct UnterbrechungV1 : EinsatzartV1
	{
		public byte[] Encoded() { return new byte[0]; }
		public static UnterbrechungV1 Decoded(byte[] data) { return new UnterbrechungV1(); }
		public static ValueTuple<UnterbrechungV1, byte[]> Decode(byte[] data) { return new ValueTuple<UnterbrechungV1, byte[]>(new UnterbrechungV1(), data); }
	}

	public interface SollIstAbgleichVerarbeitungsergebnisV1 : CommandResult { /* Base type of union */ }

	public readonly struct VerarbeitetV1 : SollIstAbgleichVerarbeitungsergebnisV1
	{
		public byte[] Encoded() { return new byte[0]; }
		public static VerarbeitetV1 Decoded(byte[] data) { return new VerarbeitetV1(); }
		public static ValueTuple<VerarbeitetV1, byte[]> Decode(byte[] data) { return new ValueTuple<VerarbeitetV1, byte[]>(new VerarbeitetV1(), data); }
	}

	public readonly struct DienstplanabschlussVerhindertVerarbeitungV1 : SollIstAbgleichVerarbeitungsergebnisV1
	{
		public readonly List<DienstplanabschlussV1> Value;
	
		public DienstplanabschlussVerhindertVerarbeitungV1(List<DienstplanabschlussV1> value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_list(Value, ValueList => ValueList.Encoded());
		}
	
		public static DienstplanabschlussVerhindertVerarbeitungV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<DienstplanabschlussVerhindertVerarbeitungV1, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_list(data, dataList => DienstplanabschlussV1.Decode(dataList));
			return new ValueTuple<DienstplanabschlussVerhindertVerarbeitungV1, byte[]>(
				new DienstplanabschlussVerhindertVerarbeitungV1(value.Item1.ToList()),
				value.Item2);
		}
	}

	public readonly struct DienstplanabschlussV1
	{
		public readonly UUID Mitarbeiter;
		public readonly UUID Mandant;
		public readonly Datum Datum;
	
		public DienstplanabschlussV1(UUID mitarbeiter, UUID mandant, Datum datum)
		{
			Mitarbeiter = mitarbeiter;
			Mandant = mandant;
			Datum = datum;
		}
	
		public byte[] Encoded()
		{
			return Mitarbeiter.Encoded()
				.Concat(Mandant.Encoded())
				.Concat(Datum.Encoded())
				.ToArray();
		}
	
		public static DienstplanabschlussV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<DienstplanabschlussV1, byte[]> Decode(byte[] data)
		{
			var mitarbeiter = UUID.Decode(data);
			var mandant = UUID.Decode(mitarbeiter.Item2);
			var datum = Datum.Decode(mandant.Item2);
			return new ValueTuple<DienstplanabschlussV1, byte[]>(
				new DienstplanabschlussV1(mitarbeiter.Item1, mandant.Item1, datum.Item1),
				datum.Item2);
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

	public readonly struct Uhrzeit
	{
		public readonly byte Stunden;
		public readonly byte Minuten;
	
		public Uhrzeit(byte stunden, byte minuten)
		{
			Stunden = stunden;
			Minuten = minuten;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_u8(Stunden)
				.Concat(BareNET.Bare.Encode_u8(Minuten))
				.ToArray();
		}
	
		public static Uhrzeit Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Uhrzeit, byte[]> Decode(byte[] data)
		{
			var stunden = BareNET.Bare.Decode_u8(data);
			var minuten = BareNET.Bare.Decode_u8(stunden.Item2);
			return new ValueTuple<Uhrzeit, byte[]>(
				new Uhrzeit(stunden.Item1, minuten.Item1),
				minuten.Item2);
		}
	}

	public readonly struct Zeitpunkt
	{
		public readonly Datum Datum;
		public readonly Uhrzeit Uhrzeit;
	
		public Zeitpunkt(Datum datum, Uhrzeit uhrzeit)
		{
			Datum = datum;
			Uhrzeit = uhrzeit;
		}
	
		public byte[] Encoded()
		{
			return Datum.Encoded()
				.Concat(Uhrzeit.Encoded())
				.ToArray();
		}
	
		public static Zeitpunkt Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Zeitpunkt, byte[]> Decode(byte[] data)
		{
			var datum = Datum.Decode(data);
			var uhrzeit = Uhrzeit.Decode(datum.Item2);
			return new ValueTuple<Zeitpunkt, byte[]>(
				new Zeitpunkt(datum.Item1, uhrzeit.Item1),
				uhrzeit.Item2);
		}
	}

	public readonly struct RelativeZeit
	{
		public readonly Uhrzeit Zeit;
		public readonly bool AmFolgetag;
	
		public RelativeZeit(Uhrzeit zeit, bool amFolgetag)
		{
			Zeit = zeit;
			AmFolgetag = amFolgetag;
		}
	
		public byte[] Encoded()
		{
			return Zeit.Encoded()
				.Concat(BareNET.Bare.Encode_bool(AmFolgetag))
				.ToArray();
		}
	
		public static RelativeZeit Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<RelativeZeit, byte[]> Decode(byte[] data)
		{
			var zeit = Uhrzeit.Decode(data);
			var amFolgetag = BareNET.Bare.Decode_bool(zeit.Item2);
			return new ValueTuple<RelativeZeit, byte[]>(
				new RelativeZeit(zeit.Item1, amFolgetag.Item1),
				amFolgetag.Item2);
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
		private static readonly BareNET.Union<AuthenticationResult> _AuthenticationResult =
			BareNET.Union<AuthenticationResult>.Register()
				.With_Case<AuthenticationSucceeded>(v => ((AuthenticationSucceeded) v).Encoded(), d => { var decoded = AuthenticationSucceeded.Decode(d); return new ValueTuple<AuthenticationResult, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<AuthenticationFailed>(v => ((AuthenticationFailed) v).Encoded(), d => { var decoded = AuthenticationFailed.Decode(d); return new ValueTuple<AuthenticationResult, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] AuthenticationResult_Encoded(AuthenticationResult value)
		{
			return BareNET.Bare.Encode_union(value, _AuthenticationResult);
		}
		
		public static AuthenticationResult AuthenticationResult_Decoded(byte[] data)
		{
			return Decode_AuthenticationResult(data).Item1;
		}
		
		public static ValueTuple<AuthenticationResult, byte[]> Decode_AuthenticationResult(byte[] data)
		{
			return BareNET.Bare.Decode_union<AuthenticationResult>(data, _AuthenticationResult);
		}


		private static readonly BareNET.Union<ResponseMessage> _ResponseMessage =
			BareNET.Union<ResponseMessage>.Register()
				.With_Case<QuerySucceeded>(v => ((QuerySucceeded) v).Encoded(), d => { var decoded = QuerySucceeded.Decode(d); return new ValueTuple<ResponseMessage, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<QueryFailed>(v => ((QueryFailed) v).Encoded(), d => { var decoded = QueryFailed.Decode(d); return new ValueTuple<ResponseMessage, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] ResponseMessage_Encoded(ResponseMessage value)
		{
			return BareNET.Bare.Encode_union(value, _ResponseMessage);
		}
		
		public static ResponseMessage ResponseMessage_Decoded(byte[] data)
		{
			return Decode_ResponseMessage(data).Item1;
		}
		
		public static ValueTuple<ResponseMessage, byte[]> Decode_ResponseMessage(byte[] data)
		{
			return BareNET.Bare.Decode_union<ResponseMessage>(data, _ResponseMessage);
		}


		private static readonly BareNET.Union<CommandResponseMessage> _CommandResponseMessage =
			BareNET.Union<CommandResponseMessage>.Register()
				.With_Case<CommandSucceeded>(v => ((CommandSucceeded) v).Encoded(), d => { var decoded = CommandSucceeded.Decode(d); return new ValueTuple<CommandResponseMessage, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<CommandFailed>(v => ((CommandFailed) v).Encoded(), d => { var decoded = CommandFailed.Decode(d); return new ValueTuple<CommandResponseMessage, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] CommandResponseMessage_Encoded(CommandResponseMessage value)
		{
			return BareNET.Bare.Encode_union(value, _CommandResponseMessage);
		}
		
		public static CommandResponseMessage CommandResponseMessage_Decoded(byte[] data)
		{
			return Decode_CommandResponseMessage(data).Item1;
		}
		
		public static ValueTuple<CommandResponseMessage, byte[]> Decode_CommandResponseMessage(byte[] data)
		{
			return BareNET.Bare.Decode_union<CommandResponseMessage>(data, _CommandResponseMessage);
		}


		public static byte[] Capability_Encoded(Capability value)
		{
			return BareNET.Bare.Encode_enum(value);
		}
		
		public static Capability Capability_Decoded(byte[] data)
		{
			return Decode_Capability(data).Item1;
		}
		
		public static ValueTuple<Capability, byte[]> Decode_Capability(byte[] data)
		{
			return BareNET.Bare.Decode_enum<Capability>(data);
		}


		private static readonly BareNET.Union<Query> _Query =
			BareNET.Union<Query>.Register()
				.With_Case<MitarbeiterAbrufenV1>(v => ((MitarbeiterAbrufenV1) v).Encoded(), d => { var decoded = MitarbeiterAbrufenV1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<MitarbeiterAbrufenAbV1>(v => ((MitarbeiterAbrufenAbV1) v).Encoded(), d => { var decoded = MitarbeiterAbrufenAbV1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<DiensteAbrufenV1>(v => ((DiensteAbrufenV1) v).Encoded(), d => { var decoded = DiensteAbrufenV1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<DiensteAbrufenAbV1>(v => ((DiensteAbrufenAbV1) v).Encoded(), d => { var decoded = DiensteAbrufenAbV1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<DienstbuchungenZumStichtagV1>(v => ((DienstbuchungenZumStichtagV1) v).Encoded(), d => { var decoded = DienstbuchungenZumStichtagV1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<AbwesenheitenZumStichtagV1>(v => ((AbwesenheitenZumStichtagV1) v).Encoded(), d => { var decoded = AbwesenheitenZumStichtagV1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); });
		
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


		private static readonly BareNET.Union<QueryResult> _QueryResult =
			BareNET.Union<QueryResult>.Register()
				.With_Case<IOFehler>(v => ((IOFehler) v).Encoded(), d => { var decoded = IOFehler.Decode(d); return new ValueTuple<QueryResult, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<MitarbeiterlisteV1>(v => ((MitarbeiterlisteV1) v).Encoded(), d => { var decoded = MitarbeiterlisteV1.Decode(d); return new ValueTuple<QueryResult, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<DiensteV1>(v => ((DiensteV1) v).Encoded(), d => { var decoded = DiensteV1.Decode(d); return new ValueTuple<QueryResult, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<DienstbuchungenV1>(v => ((DienstbuchungenV1) v).Encoded(), d => { var decoded = DienstbuchungenV1.Decode(d); return new ValueTuple<QueryResult, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<AbwesenheitenV1>(v => ((AbwesenheitenV1) v).Encoded(), d => { var decoded = AbwesenheitenV1.Decode(d); return new ValueTuple<QueryResult, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] QueryResult_Encoded(QueryResult value)
		{
			return BareNET.Bare.Encode_union(value, _QueryResult);
		}
		
		public static QueryResult QueryResult_Decoded(byte[] data)
		{
			return Decode_QueryResult(data).Item1;
		}
		
		public static ValueTuple<QueryResult, byte[]> Decode_QueryResult(byte[] data)
		{
			return BareNET.Bare.Decode_union<QueryResult>(data, _QueryResult);
		}


		private static readonly BareNET.Union<Command> _Command =
			BareNET.Union<Command>.Register()
				.With_Case<SollIstAbgleichFreigebenV1>(v => ((SollIstAbgleichFreigebenV1) v).Encoded(), d => { var decoded = SollIstAbgleichFreigebenV1.Decode(d); return new ValueTuple<Command, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] Command_Encoded(Command value)
		{
			return BareNET.Bare.Encode_union(value, _Command);
		}
		
		public static Command Command_Decoded(byte[] data)
		{
			return Decode_Command(data).Item1;
		}
		
		public static ValueTuple<Command, byte[]> Decode_Command(byte[] data)
		{
			return BareNET.Bare.Decode_union<Command>(data, _Command);
		}


		private static readonly BareNET.Union<CommandResult> _CommandResult =
			BareNET.Union<CommandResult>.Register()
				.With_Case<IOFehler>(v => ((IOFehler) v).Encoded(), d => { var decoded = IOFehler.Decode(d); return new ValueTuple<CommandResult, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<SollIstAbgleichVerarbeitungsergebnisV1>(v => Encoding.SollIstAbgleichVerarbeitungsergebnisV1_Encoded(((SollIstAbgleichVerarbeitungsergebnisV1) v)), d => { var decoded = Encoding.Decode_SollIstAbgleichVerarbeitungsergebnisV1(d); return new ValueTuple<CommandResult, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] CommandResult_Encoded(CommandResult value)
		{
			return BareNET.Bare.Encode_union(value, _CommandResult);
		}
		
		public static CommandResult CommandResult_Decoded(byte[] data)
		{
			return Decode_CommandResult(data).Item1;
		}
		
		public static ValueTuple<CommandResult, byte[]> Decode_CommandResult(byte[] data)
		{
			return BareNET.Bare.Decode_union<CommandResult>(data, _CommandResult);
		}


		public static byte[] AbwesenheitsartV1_Encoded(AbwesenheitsartV1 value)
		{
			return BareNET.Bare.Encode_enum(value);
		}
		
		public static AbwesenheitsartV1 AbwesenheitsartV1_Decoded(byte[] data)
		{
			return Decode_AbwesenheitsartV1(data).Item1;
		}
		
		public static ValueTuple<AbwesenheitsartV1, byte[]> Decode_AbwesenheitsartV1(byte[] data)
		{
			return BareNET.Bare.Decode_enum<AbwesenheitsartV1>(data);
		}


		private static readonly BareNET.Union<EinsatzartV1> _EinsatzartV1 =
			BareNET.Union<EinsatzartV1>.Register()
				.With_Case<KlientEinsatzV1>(v => ((KlientEinsatzV1) v).Encoded(), d => { var decoded = KlientEinsatzV1.Decode(d); return new ValueTuple<EinsatzartV1, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<SonstigeZeitV1>(v => ((SonstigeZeitV1) v).Encoded(), d => { var decoded = SonstigeZeitV1.Decode(d); return new ValueTuple<EinsatzartV1, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<PauseV1>(v => ((PauseV1) v).Encoded(), d => { var decoded = PauseV1.Decode(d); return new ValueTuple<EinsatzartV1, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<UnterbrechungV1>(v => ((UnterbrechungV1) v).Encoded(), d => { var decoded = UnterbrechungV1.Decode(d); return new ValueTuple<EinsatzartV1, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] EinsatzartV1_Encoded(EinsatzartV1 value)
		{
			return BareNET.Bare.Encode_union(value, _EinsatzartV1);
		}
		
		public static EinsatzartV1 EinsatzartV1_Decoded(byte[] data)
		{
			return Decode_EinsatzartV1(data).Item1;
		}
		
		public static ValueTuple<EinsatzartV1, byte[]> Decode_EinsatzartV1(byte[] data)
		{
			return BareNET.Bare.Decode_union<EinsatzartV1>(data, _EinsatzartV1);
		}


		private static readonly BareNET.Union<SollIstAbgleichVerarbeitungsergebnisV1> _SollIstAbgleichVerarbeitungsergebnisV1 =
			BareNET.Union<SollIstAbgleichVerarbeitungsergebnisV1>.Register()
				.With_Case<VerarbeitetV1>(v => ((VerarbeitetV1) v).Encoded(), d => { var decoded = VerarbeitetV1.Decode(d); return new ValueTuple<SollIstAbgleichVerarbeitungsergebnisV1, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<DienstplanabschlussVerhindertVerarbeitungV1>(v => ((DienstplanabschlussVerhindertVerarbeitungV1) v).Encoded(), d => { var decoded = DienstplanabschlussVerhindertVerarbeitungV1.Decode(d); return new ValueTuple<SollIstAbgleichVerarbeitungsergebnisV1, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] SollIstAbgleichVerarbeitungsergebnisV1_Encoded(SollIstAbgleichVerarbeitungsergebnisV1 value)
		{
			return BareNET.Bare.Encode_union(value, _SollIstAbgleichVerarbeitungsergebnisV1);
		}
		
		public static SollIstAbgleichVerarbeitungsergebnisV1 SollIstAbgleichVerarbeitungsergebnisV1_Decoded(byte[] data)
		{
			return Decode_SollIstAbgleichVerarbeitungsergebnisV1(data).Item1;
		}
		
		public static ValueTuple<SollIstAbgleichVerarbeitungsergebnisV1, byte[]> Decode_SollIstAbgleichVerarbeitungsergebnisV1(byte[] data)
		{
			return BareNET.Bare.Decode_union<SollIstAbgleichVerarbeitungsergebnisV1>(data, _SollIstAbgleichVerarbeitungsergebnisV1);
		}
	}
}
