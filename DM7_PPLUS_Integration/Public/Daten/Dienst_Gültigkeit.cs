namespace DM7_PPLUS_Integration.Daten
{
    public readonly struct Dienst_Gültigkeit
    {
        public readonly bool Montag;
        public readonly bool Dienstag;
        public readonly bool Mittwoch;
        public readonly bool Donnerstag;
        public readonly bool Freitag;
        public readonly bool Samstag;
        public readonly bool Sonntag;
        public readonly bool Feiertags;

        public Dienst_Gültigkeit(bool montag, bool dienstag, bool mittwoch, bool donnerstag, bool freitag, bool samstag, bool sonntag, bool feiertags)
        {
            Montag = montag;
            Dienstag = dienstag;
            Mittwoch = mittwoch;
            Donnerstag = donnerstag;
            Freitag = freitag;
            Samstag = samstag;
            Sonntag = sonntag;
            Feiertags = feiertags;
        }
    }
}