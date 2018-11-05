namespace DM7_PPLUS_Integration.Implementierung.Server
{
    public interface PPLUS_Authentifizierung
    {
        Autorisierung Authentifizieren(string credentials);
    }

    public interface Autorisierung
    {
    }

    public interface StammdatenZugriff : Autorisierung
    {
        
    }

    public class Vollzugriff : StammdatenZugriff, Autorisierung { }

    public class StaticAuthentication : PPLUS_Authentifizierung
    {
        private readonly string _credentials;

        public StaticAuthentication(string credentials)
        {
            _credentials = credentials;
        }

        public Autorisierung Authentifizieren(string credentials)
        {
            return (credentials == _credentials) ? (Autorisierung) new Vollzugriff() : null;
        }
    }

}