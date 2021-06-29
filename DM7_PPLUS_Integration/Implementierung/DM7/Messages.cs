//////////////////////////////////////////////////
// Generated code by BareNET - 29.06.2021 11:03 //
//////////////////////////////////////////////////
using System;
using System.Linq;
using System.Collections.Generic;
namespace DM7_PPLUS_Integration.Messages.DM7
{
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
		ALLE_LEISTUNGEN_V1,
		ALLE_MANDANTEN_V1
	}

	public interface Query { /* Base type of union */ }

	public readonly struct AlleLeistungenV1 : Query
	{
		public byte[] Encoded() { return new byte[0]; }
		public static AlleLeistungenV1 Decoded(byte[] data) { return new AlleLeistungenV1(); }
		public static ValueTuple<AlleLeistungenV1, byte[]> Decode(byte[] data) { return new ValueTuple<AlleLeistungenV1, byte[]>(new AlleLeistungenV1(), data); }
	}

	public readonly struct AlleMandantenV1 : Query
	{
		public byte[] Encoded() { return new byte[0]; }
		public static AlleMandantenV1 Decoded(byte[] data) { return new AlleMandantenV1(); }
		public static ValueTuple<AlleMandantenV1, byte[]> Decode(byte[] data) { return new ValueTuple<AlleMandantenV1, byte[]>(new AlleMandantenV1(), data); }
	}

	public interface QueryResult { /* Base type of union */ }

	public readonly struct IOFehler : QueryResult
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

	public readonly struct LeistungenV1 : QueryResult
	{
		public readonly List<LeistungV1> Value;
	
		public LeistungenV1(List<LeistungV1> value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_list(Value, ValueList => ValueList.Encoded());
		}
	
		public static LeistungenV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<LeistungenV1, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_list(data, dataList => LeistungV1.Decode(dataList));
			return new ValueTuple<LeistungenV1, byte[]>(
				new LeistungenV1(value.Item1.ToList()),
				value.Item2);
		}
	}

	public readonly struct MandantenV1 : QueryResult
	{
		public readonly List<MandantV1> Value;
	
		public MandantenV1(List<MandantV1> value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_list(Value, ValueList => ValueList.Encoded());
		}
	
		public static MandantenV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<MandantenV1, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_list(data, dataList => MandantV1.Decode(dataList));
			return new ValueTuple<MandantenV1, byte[]>(
				new MandantenV1(value.Item1.ToList()),
				value.Item2);
		}
	}

	public readonly struct LeistungV1
	{
		public readonly UUID Id;
		public readonly string Bezeichnung;
	
		public LeistungV1(UUID id, string bezeichnung)
		{
			Id = id;
			Bezeichnung = bezeichnung;
		}
	
		public byte[] Encoded()
		{
			return Id.Encoded()
				.Concat(BareNET.Bare.Encode_string(Bezeichnung))
				.ToArray();
		}
	
		public static LeistungV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<LeistungV1, byte[]> Decode(byte[] data)
		{
			var id = UUID.Decode(data);
			var bezeichnung = BareNET.Bare.Decode_string(id.Item2);
			return new ValueTuple<LeistungV1, byte[]>(
				new LeistungV1(id.Item1, bezeichnung.Item1),
				bezeichnung.Item2);
		}
	}

	public readonly struct MandantV1
	{
		public readonly UUID Id;
		public readonly string Bezeichnung;
	
		public MandantV1(UUID id, string bezeichnung)
		{
			Id = id;
			Bezeichnung = bezeichnung;
		}
	
		public byte[] Encoded()
		{
			return Id.Encoded()
				.Concat(BareNET.Bare.Encode_string(Bezeichnung))
				.ToArray();
		}
	
		public static MandantV1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<MandantV1, byte[]> Decode(byte[] data)
		{
			var id = UUID.Decode(data);
			var bezeichnung = BareNET.Bare.Decode_string(id.Item2);
			return new ValueTuple<MandantV1, byte[]>(
				new MandantV1(id.Item1, bezeichnung.Item1),
				bezeichnung.Item2);
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

	public static class Encoding
	{
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
				.With_Case<AlleLeistungenV1>(v => ((AlleLeistungenV1) v).Encoded(), d => { var decoded = AlleLeistungenV1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<AlleMandantenV1>(v => ((AlleMandantenV1) v).Encoded(), d => { var decoded = AlleMandantenV1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); });
		
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
				.With_Case<LeistungenV1>(v => ((LeistungenV1) v).Encoded(), d => { var decoded = LeistungenV1.Decode(d); return new ValueTuple<QueryResult, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<MandantenV1>(v => ((MandantenV1) v).Encoded(), d => { var decoded = MandantenV1.Decode(d); return new ValueTuple<QueryResult, byte[]>(decoded.Item1, decoded.Item2); });
		
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
	}
}
