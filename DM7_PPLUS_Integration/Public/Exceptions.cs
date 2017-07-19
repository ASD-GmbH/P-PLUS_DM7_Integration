using System;

namespace DM7_PPLUS_Integration
{
    [Serializable]
    public sealed class UnsupportedVersionException : Exception {
        public UnsupportedVersionException()
        {
        }

        public UnsupportedVersionException(string message) : base(message)
        {
        }
    }

    [Serializable]
    public sealed class ConnectionErrorException : Exception
    {
        public ConnectionErrorException(){}

        public ConnectionErrorException(string message, Exception exception) : base(message, exception) {}

        public ConnectionErrorException(string message) : base(message) { }
    }

}