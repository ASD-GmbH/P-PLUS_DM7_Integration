using System;
using System.IO;
using System.Security.Cryptography;

namespace DM7_PPLUS_Integration.Implementierung.V2
{
    public class Encryption : IDisposable
    {
        private readonly Aes _aes;

        public static string Generate_encoded_Key()
        {
            using (var aes = Aes.Create()) return Convert.ToBase64String(aes.Key);
        }

        public static Encryption From_encoded_Key(string key)
        {
            return new Encryption(Convert.FromBase64String(key));
        }

        public Encryption(byte[] key)
        {
            _aes = Aes.Create();
            _aes.Key = key;
            _aes.IV = new byte[_aes.BlockSize/8];
        }

        public byte[] Encrypt(byte[] data)
        {
            using (var encryptor = _aes.CreateEncryptor())
            using (var memoryStream = new MemoryStream())
            using (var stream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                stream.Write(data, 0, data.Length);
                stream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            using (var decryptor = _aes.CreateDecryptor())
            using (var memoryStream = new MemoryStream())
            using (var stream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
            {
                stream.Write(data, 0, data.Length);
                stream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
        }

        public void Dispose()
        {
            _aes.Dispose();
        }
    }
}