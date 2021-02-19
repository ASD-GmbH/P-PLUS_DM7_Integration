//////////////////////////////////////////////////
// Generated code by BareNET - 19.02.2021 16:49 //
//////////////////////////////////////////////////
using System;
using System.Linq;
using System.Collections.Generic;
namespace Bare.Msg
{
	public readonly struct Authentication_Request
	{
		public readonly string User;
		public readonly string Password;
	
		public Authentication_Request(string user, string password)
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
	
		public static Authentication_Request Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Authentication_Request, byte[]> Decode(byte[] data)
		{
			var user = BareNET.Bare.Decode_string(data);
			var password = BareNET.Bare.Decode_string(user.Item2);
			return new ValueTuple<Authentication_Request, byte[]>(
				new Authentication_Request(user.Item1, password.Item1),
				password.Item2);
		}
	}

	public interface Authentication_Result { /* Base type of union */ }

	public readonly struct Authentication_Succeeded : Authentication_Result
	{
		public readonly int Token;
	
		public Authentication_Succeeded(int token)
		{
			Token = token;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_i32(Token);
		}
	
		public static Authentication_Succeeded Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Authentication_Succeeded, byte[]> Decode(byte[] data)
		{
			var token = BareNET.Bare.Decode_i32(data);
			return new ValueTuple<Authentication_Succeeded, byte[]>(
				new Authentication_Succeeded(token.Item1),
				token.Item2);
		}
	}

	public readonly struct Authentication_Failed : Authentication_Result
	{
		public readonly string Reason;
	
		public Authentication_Failed(string reason)
		{
			Reason = reason;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_string(Reason);
		}
	
		public static Authentication_Failed Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Authentication_Failed, byte[]> Decode(byte[] data)
		{
			var reason = BareNET.Bare.Decode_string(data);
			return new ValueTuple<Authentication_Failed, byte[]>(
				new Authentication_Failed(reason.Item1),
				reason.Item2);
		}
	}

	public readonly struct Query_Message
	{
		public readonly int Token;
		public readonly byte[] Query;
	
		public Query_Message(int token, byte[] query)
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
	
		public static Query_Message Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Query_Message, byte[]> Decode(byte[] data)
		{
			var token = BareNET.Bare.Decode_i32(data);
			var query = BareNET.Bare.Decode_data(token.Item2);
			return new ValueTuple<Query_Message, byte[]>(
				new Query_Message(token.Item1, query.Item1),
				query.Item2);
		}
	}

	public interface Response_Message { /* Base type of union */ }

	public readonly struct Query_Succeeded : Response_Message
	{
		public readonly byte[] Value;
	
		public Query_Succeeded(byte[] value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_data(Value);
		}
	
		public static Query_Succeeded Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Query_Succeeded, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_data(data);
			return new ValueTuple<Query_Succeeded, byte[]>(
				new Query_Succeeded(value.Item1),
				value.Item2);
		}
	}

	public readonly struct Query_Failed : Response_Message
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

	public readonly struct Dienste_abrufen_V1 : Query
	{
		public byte[] Encoded() { return new byte[0]; }
		public static Dienste_abrufen_V1 Decoded(byte[] data) { return new Dienste_abrufen_V1(); }
		public static ValueTuple<Dienste_abrufen_V1, byte[]> Decode(byte[] data) { return new ValueTuple<Dienste_abrufen_V1, byte[]>(new Dienste_abrufen_V1(), data); }
	}

	public readonly struct Dienste_abrufen_ab_V1 : Query
	{
		public readonly Datenstand Value;
	
		public Dienste_abrufen_ab_V1(Datenstand value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return Value.Encoded();
		}
	
		public static Dienste_abrufen_ab_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Dienste_abrufen_ab_V1, byte[]> Decode(byte[] data)
		{
			var value = Datenstand.Decode(data);
			return new ValueTuple<Dienste_abrufen_ab_V1, byte[]>(
				new Dienste_abrufen_ab_V1(value.Item1),
				value.Item2);
		}
	}

	public readonly struct Abwesenheiten_zum_Stichtag_V1 : Query
	{
		public readonly Datum Stichtag;
		public readonly UUID Mandant;
	
		public Abwesenheiten_zum_Stichtag_V1(Datum stichtag, UUID mandant)
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
	
		public static Abwesenheiten_zum_Stichtag_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Abwesenheiten_zum_Stichtag_V1, byte[]> Decode(byte[] data)
		{
			var stichtag = Datum.Decode(data);
			var mandant = UUID.Decode(stichtag.Item2);
			return new ValueTuple<Abwesenheiten_zum_Stichtag_V1, byte[]>(
				new Abwesenheiten_zum_Stichtag_V1(stichtag.Item1, mandant.Item1),
				mandant.Item2);
		}
	}

	public readonly struct Dienstbuchungen_zum_Stichtag_V1 : Query
	{
		public readonly Datum Stichtag;
		public readonly UUID Mandant;
	
		public Dienstbuchungen_zum_Stichtag_V1(Datum stichtag, UUID mandant)
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
	
		public static Dienstbuchungen_zum_Stichtag_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Dienstbuchungen_zum_Stichtag_V1, byte[]> Decode(byte[] data)
		{
			var stichtag = Datum.Decode(data);
			var mandant = UUID.Decode(stichtag.Item2);
			return new ValueTuple<Dienstbuchungen_zum_Stichtag_V1, byte[]>(
				new Dienstbuchungen_zum_Stichtag_V1(stichtag.Item1, mandant.Item1),
				mandant.Item2);
		}
	}

	public interface Query_Result { /* Base type of union */ }

	public readonly struct IO_Fehler : Query_Result
	{
		public readonly string Reason;
	
		public IO_Fehler(string reason)
		{
			Reason = reason;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_string(Reason);
		}
	
		public static IO_Fehler Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<IO_Fehler, byte[]> Decode(byte[] data)
		{
			var reason = BareNET.Bare.Decode_string(data);
			return new ValueTuple<IO_Fehler, byte[]>(
				new IO_Fehler(reason.Item1),
				reason.Item2);
		}
	}

	public readonly struct Mitarbeiterliste_V1 : Query_Result
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

	public readonly struct Dienste_V1 : Query_Result
	{
		public readonly Dienst_V1[] Dienste;
		public readonly Datenstand Stand;
	
		public Dienste_V1(Dienst_V1[] dienste, Datenstand stand)
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
	
		public static Dienste_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Dienste_V1, byte[]> Decode(byte[] data)
		{
			var dienste = BareNET.Bare.Decode_list(data, dataList => Dienst_V1.Decode(dataList));
			var stand = Datenstand.Decode(dienste.Item2);
			return new ValueTuple<Dienste_V1, byte[]>(
				new Dienste_V1(dienste.Item1.ToArray(), stand.Item1),
				stand.Item2);
		}
	}

	public readonly struct Dienstbuchungen_V1 : Query_Result
	{
		public readonly Dienstbuchung_V1[] Value;
	
		public Dienstbuchungen_V1(Dienstbuchung_V1[] value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_list(Value, ValueList => ValueList.Encoded());
		}
	
		public static Dienstbuchungen_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Dienstbuchungen_V1, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_list(data, dataList => Dienstbuchung_V1.Decode(dataList));
			return new ValueTuple<Dienstbuchungen_V1, byte[]>(
				new Dienstbuchungen_V1(value.Item1.ToArray()),
				value.Item2);
		}
	}

	public readonly struct Abwesenheiten_V1 : Query_Result
	{
		public readonly Abwesenheit_V1[] Value;
	
		public Abwesenheiten_V1(Abwesenheit_V1[] value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_list(Value, ValueList => ValueList.Encoded());
		}
	
		public static Abwesenheiten_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Abwesenheiten_V1, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_list(data, dataList => Abwesenheit_V1.Decode(dataList));
			return new ValueTuple<Abwesenheiten_V1, byte[]>(
				new Abwesenheiten_V1(value.Item1.ToArray()),
				value.Item2);
		}
	}

	public readonly struct Mitarbeiter_V1
	{
		public readonly UUID Id;
		public readonly DM7_Mandantenzugehoerigkeit_V1[] Mandantenzugehoerigkeiten;
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
	
		public Mitarbeiter_V1(UUID id, DM7_Mandantenzugehoerigkeit_V1[] mandantenzugehoerigkeiten, UUID titel, string vorname, string nachname, Postanschrift_V1? postanschrift, Datum? geburtstag, UUID familienstand, UUID konfession, Qualifikation_V1[] qualifikationen, string handzeichen, string personalnummer, UUID geschlecht, Kontakt_V1[] kontakte)
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
			var mandantenzugehoerigkeiten = BareNET.Bare.Decode_list(id.Item2, idList => DM7_Mandantenzugehoerigkeit_V1.Decode(idList));
			var titel = UUID.Decode(mandantenzugehoerigkeiten.Item2);
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
				new Mitarbeiter_V1(id.Item1, mandantenzugehoerigkeiten.Item1.ToArray(), titel.Item1, vorname.Item1, nachname.Item1, postanschrift.Item1, geburtstag.Item1, familienstand.Item1, konfession.Item1, qualifikationen.Item1.ToArray(), handzeichen.Item1, personalnummer.Item1, geschlecht.Item1, kontakte.Item1.ToArray()),
				kontakte.Item2);
		}
	}

	public readonly struct DM7_Mandantenzugehoerigkeit_V1
	{
		public readonly UUID Mandantid;
		public readonly Datum Gueltigab;
		public readonly Datum? Gueltigbis;
	
		public DM7_Mandantenzugehoerigkeit_V1(UUID mandantId, Datum gueltigAb, Datum? gueltigBis)
		{
			Mandantid = mandantId;
			Gueltigab = gueltigAb;
			Gueltigbis = gueltigBis;
		}
	
		public byte[] Encoded()
		{
			return Mandantid.Encoded()
				.Concat(Gueltigab.Encoded())
				.Concat(BareNET.Bare.Encode_optional<Datum>(Gueltigbis, GueltigbisOpt => GueltigbisOpt.Encoded()))
				.ToArray();
		}
	
		public static DM7_Mandantenzugehoerigkeit_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<DM7_Mandantenzugehoerigkeit_V1, byte[]> Decode(byte[] data)
		{
			var mandantId = UUID.Decode(data);
			var gueltigAb = Datum.Decode(mandantId.Item2);
			var gueltigBis = BareNET.Bare.Decode_optional(gueltigAb.Item2, gueltigAbOpt => Datum.Decode(gueltigAbOpt));
			return new ValueTuple<DM7_Mandantenzugehoerigkeit_V1, byte[]>(
				new DM7_Mandantenzugehoerigkeit_V1(mandantId.Item1, gueltigAb.Item1, gueltigBis.Item1),
				gueltigBis.Item2);
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
		public readonly Datum Gueltigab;
		public readonly Datum? Gueltigbis;
	
		public Qualifikation_V1(byte stufe, string bezeichnung, Datum gueltigAb, Datum? gueltigBis)
		{
			Stufe = stufe;
			Bezeichnung = bezeichnung;
			Gueltigab = gueltigAb;
			Gueltigbis = gueltigBis;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_u8(Stufe)
				.Concat(BareNET.Bare.Encode_string(Bezeichnung))
				.Concat(Gueltigab.Encoded())
				.Concat(BareNET.Bare.Encode_optional<Datum>(Gueltigbis, GueltigbisOpt => GueltigbisOpt.Encoded()))
				.ToArray();
		}
	
		public static Qualifikation_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Qualifikation_V1, byte[]> Decode(byte[] data)
		{
			var stufe = BareNET.Bare.Decode_u8(data);
			var bezeichnung = BareNET.Bare.Decode_string(stufe.Item2);
			var gueltigAb = Datum.Decode(bezeichnung.Item2);
			var gueltigBis = BareNET.Bare.Decode_optional(gueltigAb.Item2, gueltigAbOpt => Datum.Decode(gueltigAbOpt));
			return new ValueTuple<Qualifikation_V1, byte[]>(
				new Qualifikation_V1(stufe.Item1, bezeichnung.Item1, gueltigAb.Item1, gueltigBis.Item1),
				gueltigBis.Item2);
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

	public readonly struct Dienst_V1
	{
		public readonly ulong Id;
		public readonly DM7_Mandantenzugehoerigkeit_V1[] Mandantenzugehoerigkeiten;
		public readonly string Kurzbezeichnung;
		public readonly string Bezeichnung;
		public readonly Datum Gueltigab;
		public readonly Datum? Gueltibbis;
		public readonly Dienst_Gueltigkeit_V1 Gueltigan;
		public readonly bool Geloescht;
	
		public Dienst_V1(ulong id, DM7_Mandantenzugehoerigkeit_V1[] mandantenzugehoerigkeiten, string kurzbezeichnung, string bezeichnung, Datum gueltigAb, Datum? gueltibBis, Dienst_Gueltigkeit_V1 gueltigAn, bool geloescht)
		{
			Id = id;
			Mandantenzugehoerigkeiten = mandantenzugehoerigkeiten;
			Kurzbezeichnung = kurzbezeichnung;
			Bezeichnung = bezeichnung;
			Gueltigab = gueltigAb;
			Gueltibbis = gueltibBis;
			Gueltigan = gueltigAn;
			Geloescht = geloescht;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_uint(Id)
				.Concat(BareNET.Bare.Encode_list(Mandantenzugehoerigkeiten, MandantenzugehoerigkeitenList => MandantenzugehoerigkeitenList.Encoded()))
				.Concat(BareNET.Bare.Encode_string(Kurzbezeichnung))
				.Concat(BareNET.Bare.Encode_string(Bezeichnung))
				.Concat(Gueltigab.Encoded())
				.Concat(BareNET.Bare.Encode_optional<Datum>(Gueltibbis, GueltibbisOpt => GueltibbisOpt.Encoded()))
				.Concat(Gueltigan.Encoded())
				.Concat(BareNET.Bare.Encode_bool(Geloescht))
				.ToArray();
		}
	
		public static Dienst_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Dienst_V1, byte[]> Decode(byte[] data)
		{
			var id = BareNET.Bare.Decode_uint(data);
			var mandantenzugehoerigkeiten = BareNET.Bare.Decode_list(id.Item2, idList => DM7_Mandantenzugehoerigkeit_V1.Decode(idList));
			var kurzbezeichnung = BareNET.Bare.Decode_string(mandantenzugehoerigkeiten.Item2);
			var bezeichnung = BareNET.Bare.Decode_string(kurzbezeichnung.Item2);
			var gueltigAb = Datum.Decode(bezeichnung.Item2);
			var gueltibBis = BareNET.Bare.Decode_optional(gueltigAb.Item2, gueltigAbOpt => Datum.Decode(gueltigAbOpt));
			var gueltigAn = Dienst_Gueltigkeit_V1.Decode(gueltibBis.Item2);
			var geloescht = BareNET.Bare.Decode_bool(gueltigAn.Item2);
			return new ValueTuple<Dienst_V1, byte[]>(
				new Dienst_V1(id.Item1, mandantenzugehoerigkeiten.Item1.ToArray(), kurzbezeichnung.Item1, bezeichnung.Item1, gueltigAb.Item1, gueltibBis.Item1, gueltigAn.Item1, geloescht.Item1),
				geloescht.Item2);
		}
	}

	public readonly struct Dienst_Gueltigkeit_V1
	{
		public readonly bool Montag;
		public readonly bool Dienstag;
		public readonly bool Mittwoch;
		public readonly bool Donnerstag;
		public readonly bool Freitag;
		public readonly bool Samstag;
		public readonly bool Sonntag;
		public readonly bool Feiertags;
	
		public Dienst_Gueltigkeit_V1(bool montag, bool dienstag, bool mittwoch, bool donnerstag, bool freitag, bool samstag, bool sonntag, bool feiertags)
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
	
		public static Dienst_Gueltigkeit_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Dienst_Gueltigkeit_V1, byte[]> Decode(byte[] data)
		{
			var montag = BareNET.Bare.Decode_bool(data);
			var dienstag = BareNET.Bare.Decode_bool(montag.Item2);
			var mittwoch = BareNET.Bare.Decode_bool(dienstag.Item2);
			var donnerstag = BareNET.Bare.Decode_bool(mittwoch.Item2);
			var freitag = BareNET.Bare.Decode_bool(donnerstag.Item2);
			var samstag = BareNET.Bare.Decode_bool(freitag.Item2);
			var sonntag = BareNET.Bare.Decode_bool(samstag.Item2);
			var feiertags = BareNET.Bare.Decode_bool(sonntag.Item2);
			return new ValueTuple<Dienst_Gueltigkeit_V1, byte[]>(
				new Dienst_Gueltigkeit_V1(montag.Item1, dienstag.Item1, mittwoch.Item1, donnerstag.Item1, freitag.Item1, samstag.Item1, sonntag.Item1, feiertags.Item1),
				feiertags.Item2);
		}
	}

	public readonly struct Dienstbuchung_V1
	{
		public readonly UUID Mitarbeiter;
		public readonly long Dienst;
		public readonly Uhrzeit Beginntum;
	
		public Dienstbuchung_V1(UUID mitarbeiter, long dienst, Uhrzeit beginntUm)
		{
			Mitarbeiter = mitarbeiter;
			Dienst = dienst;
			Beginntum = beginntUm;
		}
	
		public byte[] Encoded()
		{
			return Mitarbeiter.Encoded()
				.Concat(BareNET.Bare.Encode_int(Dienst))
				.Concat(Beginntum.Encoded())
				.ToArray();
		}
	
		public static Dienstbuchung_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Dienstbuchung_V1, byte[]> Decode(byte[] data)
		{
			var mitarbeiter = UUID.Decode(data);
			var dienst = BareNET.Bare.Decode_int(mitarbeiter.Item2);
			var beginntUm = Uhrzeit.Decode(dienst.Item2);
			return new ValueTuple<Dienstbuchung_V1, byte[]>(
				new Dienstbuchung_V1(mitarbeiter.Item1, dienst.Item1, beginntUm.Item1),
				beginntUm.Item2);
		}
	}

	public readonly struct Abwesenheit_V1
	{
		public readonly UUID Mitarbeiter;
		public readonly Zeitpunkt Abwesendab;
		public readonly Zeitpunkt Vorraussichtlichwiederverfuegbarab;
		public readonly string Grund;
		public readonly Abwesenheitsart_V1 Art;
	
		public Abwesenheit_V1(UUID mitarbeiter, Zeitpunkt abwesendAb, Zeitpunkt vorraussichtlichWiederVerfuegbarAb, string grund, Abwesenheitsart_V1 art)
		{
			Mitarbeiter = mitarbeiter;
			Abwesendab = abwesendAb;
			Vorraussichtlichwiederverfuegbarab = vorraussichtlichWiederVerfuegbarAb;
			Grund = grund;
			Art = art;
		}
	
		public byte[] Encoded()
		{
			return Mitarbeiter.Encoded()
				.Concat(Abwesendab.Encoded())
				.Concat(Vorraussichtlichwiederverfuegbarab.Encoded())
				.Concat(BareNET.Bare.Encode_string(Grund))
				.Concat(Encoding.Abwesenheitsart_V1_Encoded(Art))
				.ToArray();
		}
	
		public static Abwesenheit_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Abwesenheit_V1, byte[]> Decode(byte[] data)
		{
			var mitarbeiter = UUID.Decode(data);
			var abwesendAb = Zeitpunkt.Decode(mitarbeiter.Item2);
			var vorraussichtlichWiederVerfuegbarAb = Zeitpunkt.Decode(abwesendAb.Item2);
			var grund = BareNET.Bare.Decode_string(vorraussichtlichWiederVerfuegbarAb.Item2);
			var art = Encoding.Decode_Abwesenheitsart_V1(grund.Item2);
			return new ValueTuple<Abwesenheit_V1, byte[]>(
				new Abwesenheit_V1(mitarbeiter.Item1, abwesendAb.Item1, vorraussichtlichWiederVerfuegbarAb.Item1, grund.Item1, art.Item1),
				art.Item2);
		}
	}

	public enum Abwesenheitsart_V1
	{
		FEHLZEIT,
		ANDERSWEITIG_VERPLANT
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
		private static readonly BareNET.Union<Authentication_Result> _Authentication_Result =
			BareNET.Union<Authentication_Result>.Register()
				.With_Case<Authentication_Succeeded>(v => ((Authentication_Succeeded) v).Encoded(), d => { var decoded = Authentication_Succeeded.Decode(d); return new ValueTuple<Authentication_Result, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Authentication_Failed>(v => ((Authentication_Failed) v).Encoded(), d => { var decoded = Authentication_Failed.Decode(d); return new ValueTuple<Authentication_Result, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] Authentication_Result_Encoded(Authentication_Result value)
		{
			return BareNET.Bare.Encode_union(value, _Authentication_Result);
		}
		
		public static Authentication_Result Authentication_Result_Decoded(byte[] data)
		{
			return Decode_Authentication_Result(data).Item1;
		}
		
		public static ValueTuple<Authentication_Result, byte[]> Decode_Authentication_Result(byte[] data)
		{
			return BareNET.Bare.Decode_union<Authentication_Result>(data, _Authentication_Result);
		}


		private static readonly BareNET.Union<Response_Message> _Response_Message =
			BareNET.Union<Response_Message>.Register()
				.With_Case<Query_Succeeded>(v => ((Query_Succeeded) v).Encoded(), d => { var decoded = Query_Succeeded.Decode(d); return new ValueTuple<Response_Message, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Query_Failed>(v => ((Query_Failed) v).Encoded(), d => { var decoded = Query_Failed.Decode(d); return new ValueTuple<Response_Message, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] Response_Message_Encoded(Response_Message value)
		{
			return BareNET.Bare.Encode_union(value, _Response_Message);
		}
		
		public static Response_Message Response_Message_Decoded(byte[] data)
		{
			return Decode_Response_Message(data).Item1;
		}
		
		public static ValueTuple<Response_Message, byte[]> Decode_Response_Message(byte[] data)
		{
			return BareNET.Bare.Decode_union<Response_Message>(data, _Response_Message);
		}


		private static readonly BareNET.Union<Query> _Query =
			BareNET.Union<Query>.Register()
				.With_Case<Mitarbeiter_abrufen_V1>(v => ((Mitarbeiter_abrufen_V1) v).Encoded(), d => { var decoded = Mitarbeiter_abrufen_V1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Mitarbeiter_abrufen_ab_V1>(v => ((Mitarbeiter_abrufen_ab_V1) v).Encoded(), d => { var decoded = Mitarbeiter_abrufen_ab_V1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Dienste_abrufen_V1>(v => ((Dienste_abrufen_V1) v).Encoded(), d => { var decoded = Dienste_abrufen_V1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Dienste_abrufen_ab_V1>(v => ((Dienste_abrufen_ab_V1) v).Encoded(), d => { var decoded = Dienste_abrufen_ab_V1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Dienstbuchungen_zum_Stichtag_V1>(v => ((Dienstbuchungen_zum_Stichtag_V1) v).Encoded(), d => { var decoded = Dienstbuchungen_zum_Stichtag_V1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Abwesenheiten_zum_Stichtag_V1>(v => ((Abwesenheiten_zum_Stichtag_V1) v).Encoded(), d => { var decoded = Abwesenheiten_zum_Stichtag_V1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); });
		
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


		private static readonly BareNET.Union<Query_Result> _Query_Result =
			BareNET.Union<Query_Result>.Register()
				.With_Case<IO_Fehler>(v => ((IO_Fehler) v).Encoded(), d => { var decoded = IO_Fehler.Decode(d); return new ValueTuple<Query_Result, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Mitarbeiterliste_V1>(v => ((Mitarbeiterliste_V1) v).Encoded(), d => { var decoded = Mitarbeiterliste_V1.Decode(d); return new ValueTuple<Query_Result, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Dienste_V1>(v => ((Dienste_V1) v).Encoded(), d => { var decoded = Dienste_V1.Decode(d); return new ValueTuple<Query_Result, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Dienstbuchungen_V1>(v => ((Dienstbuchungen_V1) v).Encoded(), d => { var decoded = Dienstbuchungen_V1.Decode(d); return new ValueTuple<Query_Result, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Abwesenheiten_V1>(v => ((Abwesenheiten_V1) v).Encoded(), d => { var decoded = Abwesenheiten_V1.Decode(d); return new ValueTuple<Query_Result, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] Query_Result_Encoded(Query_Result value)
		{
			return BareNET.Bare.Encode_union(value, _Query_Result);
		}
		
		public static Query_Result Query_Result_Decoded(byte[] data)
		{
			return Decode_Query_Result(data).Item1;
		}
		
		public static ValueTuple<Query_Result, byte[]> Decode_Query_Result(byte[] data)
		{
			return BareNET.Bare.Decode_union<Query_Result>(data, _Query_Result);
		}


		public static byte[] Abwesenheitsart_V1_Encoded(Abwesenheitsart_V1 value)
		{
			return BareNET.Bare.Encode_enum(value);
		}
		
		public static Abwesenheitsart_V1 Abwesenheitsart_V1_Decoded(byte[] data)
		{
			return Decode_Abwesenheitsart_V1(data).Item1;
		}
		
		public static ValueTuple<Abwesenheitsart_V1, byte[]> Decode_Abwesenheitsart_V1(byte[] data)
		{
			return BareNET.Bare.Decode_enum<Abwesenheitsart_V1>(data);
		}
	}
}
