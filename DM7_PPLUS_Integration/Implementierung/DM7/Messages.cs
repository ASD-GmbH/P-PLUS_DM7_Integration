//////////////////////////////////////////////////
// Generated code by BareNET - 08.10.2021 14:58 //
//////////////////////////////////////////////////
using System;
using System.Linq;
using System.Collections.Generic;
namespace DM7_PPLUS_Integration.Messages.DM7
{
	public readonly struct Query_message
	{
		public readonly byte[] Value;
	
		public Query_message(byte[] value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_data(Value);
		}
	
		public static Query_message Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Query_message, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_data(data);
			return new ValueTuple<Query_message, byte[]>(
				new Query_message(value.Item1),
				value.Item2);
		}
	}

	public interface Response_message { /* Base type of union */ }

	public readonly struct Query_succeeded : Response_message
	{
		public readonly byte[] Value;
	
		public Query_succeeded(byte[] value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_data(Value);
		}
	
		public static Query_succeeded Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Query_succeeded, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_data(data);
			return new ValueTuple<Query_succeeded, byte[]>(
				new Query_succeeded(value.Item1),
				value.Item2);
		}
	}

	public readonly struct Query_failed : Response_message
	{
		public readonly string Reason;
	
		public Query_failed(string reason)
		{
			Reason = reason;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_string(Reason);
		}
	
		public static Query_failed Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Query_failed, byte[]> Decode(byte[] data)
		{
			var reason = BareNET.Bare.Decode_string(data);
			return new ValueTuple<Query_failed, byte[]>(
				new Query_failed(reason.Item1),
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

	public readonly struct Alle_leistungen_V1 : Query
	{
		public byte[] Encoded() { return new byte[0]; }
		public static Alle_leistungen_V1 Decoded(byte[] data) { return new Alle_leistungen_V1(); }
		public static ValueTuple<Alle_leistungen_V1, byte[]> Decode(byte[] data) { return new ValueTuple<Alle_leistungen_V1, byte[]>(new Alle_leistungen_V1(), data); }
	}

	public readonly struct Alle_mandanten_V1 : Query
	{
		public byte[] Encoded() { return new byte[0]; }
		public static Alle_mandanten_V1 Decoded(byte[] data) { return new Alle_mandanten_V1(); }
		public static ValueTuple<Alle_mandanten_V1, byte[]> Decode(byte[] data) { return new ValueTuple<Alle_mandanten_V1, byte[]>(new Alle_mandanten_V1(), data); }
	}

	public interface Query_result { /* Base type of union */ }

	public readonly struct IO_fehler : Query_result
	{
		public readonly string Reason;
	
		public IO_fehler(string reason)
		{
			Reason = reason;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_string(Reason);
		}
	
		public static IO_fehler Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<IO_fehler, byte[]> Decode(byte[] data)
		{
			var reason = BareNET.Bare.Decode_string(data);
			return new ValueTuple<IO_fehler, byte[]>(
				new IO_fehler(reason.Item1),
				reason.Item2);
		}
	}

	public readonly struct Leistungen_V1 : Query_result
	{
		public readonly List<Leistung_V1> Value;
	
		public Leistungen_V1(List<Leistung_V1> value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_list(Value, ValueList => ValueList.Encoded());
		}
	
		public static Leistungen_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Leistungen_V1, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_list(data, dataList => Leistung_V1.Decode(dataList));
			return new ValueTuple<Leistungen_V1, byte[]>(
				new Leistungen_V1(value.Item1.ToList()),
				value.Item2);
		}
	}

	public readonly struct Mandanten_V1 : Query_result
	{
		public readonly List<Mandant_V1> Value;
	
		public Mandanten_V1(List<Mandant_V1> value)
		{
			Value = value;
		}
	
		public byte[] Encoded()
		{
			return BareNET.Bare.Encode_list(Value, ValueList => ValueList.Encoded());
		}
	
		public static Mandanten_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Mandanten_V1, byte[]> Decode(byte[] data)
		{
			var value = BareNET.Bare.Decode_list(data, dataList => Mandant_V1.Decode(dataList));
			return new ValueTuple<Mandanten_V1, byte[]>(
				new Mandanten_V1(value.Item1.ToList()),
				value.Item2);
		}
	}

	public readonly struct Leistung_V1
	{
		public readonly UUID Id;
		public readonly string Bezeichnung;
	
		public Leistung_V1(UUID id, string bezeichnung)
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
	
		public static Leistung_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Leistung_V1, byte[]> Decode(byte[] data)
		{
			var id = UUID.Decode(data);
			var bezeichnung = BareNET.Bare.Decode_string(id.Item2);
			return new ValueTuple<Leistung_V1, byte[]>(
				new Leistung_V1(id.Item1, bezeichnung.Item1),
				bezeichnung.Item2);
		}
	}

	public readonly struct Mandant_V1
	{
		public readonly UUID Id;
		public readonly string Bezeichnung;
	
		public Mandant_V1(UUID id, string bezeichnung)
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
	
		public static Mandant_V1 Decoded(byte[] data) { return Decode(data).Item1; }
	
		public static ValueTuple<Mandant_V1, byte[]> Decode(byte[] data)
		{
			var id = UUID.Decode(data);
			var bezeichnung = BareNET.Bare.Decode_string(id.Item2);
			return new ValueTuple<Mandant_V1, byte[]>(
				new Mandant_V1(id.Item1, bezeichnung.Item1),
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
		private static readonly BareNET.Union<Response_message> _Response_message =
			BareNET.Union<Response_message>.Register()
				.With_Case<Query_succeeded>(v => ((Query_succeeded) v).Encoded(), d => { var decoded = Query_succeeded.Decode(d); return new ValueTuple<Response_message, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Query_failed>(v => ((Query_failed) v).Encoded(), d => { var decoded = Query_failed.Decode(d); return new ValueTuple<Response_message, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] Response_message_Encoded(Response_message value)
		{
			return BareNET.Bare.Encode_union(value, _Response_message);
		}
		
		public static Response_message Response_message_Decoded(byte[] data)
		{
			return Decode_Response_message(data).Item1;
		}
		
		public static ValueTuple<Response_message, byte[]> Decode_Response_message(byte[] data)
		{
			return BareNET.Bare.Decode_union<Response_message>(data, _Response_message);
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
				.With_Case<Alle_leistungen_V1>(v => ((Alle_leistungen_V1) v).Encoded(), d => { var decoded = Alle_leistungen_V1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Alle_mandanten_V1>(v => ((Alle_mandanten_V1) v).Encoded(), d => { var decoded = Alle_mandanten_V1.Decode(d); return new ValueTuple<Query, byte[]>(decoded.Item1, decoded.Item2); });
		
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


		private static readonly BareNET.Union<Query_result> _Query_result =
			BareNET.Union<Query_result>.Register()
				.With_Case<IO_fehler>(v => ((IO_fehler) v).Encoded(), d => { var decoded = IO_fehler.Decode(d); return new ValueTuple<Query_result, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Leistungen_V1>(v => ((Leistungen_V1) v).Encoded(), d => { var decoded = Leistungen_V1.Decode(d); return new ValueTuple<Query_result, byte[]>(decoded.Item1, decoded.Item2); })
				.With_Case<Mandanten_V1>(v => ((Mandanten_V1) v).Encoded(), d => { var decoded = Mandanten_V1.Decode(d); return new ValueTuple<Query_result, byte[]>(decoded.Item1, decoded.Item2); });
		
		public static byte[] Query_result_Encoded(Query_result value)
		{
			return BareNET.Bare.Encode_union(value, _Query_result);
		}
		
		public static Query_result Query_result_Decoded(byte[] data)
		{
			return Decode_Query_result(data).Item1;
		}
		
		public static ValueTuple<Query_result, byte[]> Decode_Query_result(byte[] data)
		{
			return BareNET.Bare.Decode_union<Query_result>(data, _Query_result);
		}
	}
}
