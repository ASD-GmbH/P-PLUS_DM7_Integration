namespace DM7_PPLUS_Integration.Implementierung.V2
{
    public readonly struct Token
    {
        public static Token Demo() => new Token(0);

        public readonly int Value;

        public Token(int value)
        {
            Value = value;
        }

        public static bool operator !=(Token a, Token b)
        {
            return a.Value != b.Value;
        }

        public static bool operator ==(Token a, Token b)
        {
            return a.Value == b.Value;
        }
    }
}