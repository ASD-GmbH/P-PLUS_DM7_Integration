using System;
using System.IO;
using System.Security.Cryptography;

namespace DM7_PPLUS_Integration.Implementierung.Server
{
    public class CryptoService : IDisposable
    {
        private readonly AesCryptoServiceProvider _aes;
        private readonly byte[] _iv;

        public CryptoService(byte[] symmetrickey, byte[] iv = null)
        {
            _aes = new AesCryptoServiceProvider() { Key = symmetrickey };
            if (iv != null)
            {
                _aes.IV = iv;
            }
            else
            {
                _aes.GenerateIV();
            }

            _iv = _aes.IV;
        }

        public void Dispose()
        {
            _aes?.Dispose();
        }

        public byte[] IV => _iv;

        public byte[] Encrypt(byte[] data)
        {
            using (var encryptor = _aes.CreateEncryptor())
            using (var source = new MemoryStream(data))
            using (var stream = new CryptoStream(source, encryptor, CryptoStreamMode.Read))
            using (var result = new MemoryStream())
            {
                stream.CopyTo(result);
                return result.ToArray();
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            using (var decryptor = _aes.CreateDecryptor())
            using (var source = new MemoryStream(data))
            using (var stream = new CryptoStream(source, decryptor, CryptoStreamMode.Read))
            using (var result = new MemoryStream())
            {
                stream.CopyTo(result);
                return result.ToArray();
            }
        }
    }
}