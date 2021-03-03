namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Relative_Zeit
    {
        public readonly Uhrzeit Zeit;
        public readonly bool Am_Folgetag;

        public Relative_Zeit(Uhrzeit zeit, bool am_Folgetag)
        {
            Zeit = zeit;
            Am_Folgetag = am_Folgetag;
        }
    }
}