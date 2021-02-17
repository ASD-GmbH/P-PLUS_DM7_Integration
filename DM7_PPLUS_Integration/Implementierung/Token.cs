namespace DM7_PPLUS_Integration.Implementierung
{
    public readonly struct Token
    {
        public static Token Demo() => new Token(0);

        public readonly int Value;

        public Token(int value)
        {
            Value = value;
        }
        
        public bool Equals(Token other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is Token other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
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