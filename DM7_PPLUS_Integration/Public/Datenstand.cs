namespace DM7_PPLUS_Integration
{
    public readonly struct Datenstand
    {
        public readonly ulong Value;

        public Datenstand(ulong value)
        {
            Value = value;
        }

        public string Serialisiert()
        {
            return Value.ToString();
        }

        public static Datenstand Aus_serialisiertem_Wert(string serialisiert)
        {
            return new Datenstand(ulong.Parse(serialisiert));
        }
    }
}