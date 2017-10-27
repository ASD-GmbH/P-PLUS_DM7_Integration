using System;
using System.IO;
using System.Linq;

namespace DM7_PPLUS_Integration.Implementierung.Shared
{
    public static class StreamReaderExtension
    {
        public static void WriteStringWithLengthPrefix(this Stream stream, string @string)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(@string);
            stream.Write(BitConverter.GetBytes(buffer.Length), 0, 4);
            stream.Write(buffer, 0, buffer.Length);
        }

        public static string ReadStringWithPrefixedLength(this Stream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            var length = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return System.Text.Encoding.UTF8.GetString(buffer, 0, length);
        }

        public static void WriteGuid(this Stream stream, Guid guid)
        {
            stream.Write(guid.ToByteArray(), 0, 16);
        }

        public static Guid ReadGuid(this Stream stream)
        {
            var buffer = new byte[16];
            stream.Read(buffer, 0, 16);
            return new Guid(buffer);
        }
    }
}