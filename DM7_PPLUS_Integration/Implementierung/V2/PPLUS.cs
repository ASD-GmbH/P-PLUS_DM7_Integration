using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using Bare.Msg;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration.Implementierung.V2
{
    internal interface Authentication_Result { }

    internal readonly struct Authenticated : Authentication_Result
    {
        public readonly PPLUS_Handler Handler;
        public readonly Token Token;

        public Authenticated(PPLUS_Handler handler, Token token)
        {
            Handler = handler;
            Token = token;
        }
    }

    internal readonly struct Not_Authenticated : Authentication_Result { }

    public class PPLUS : DM7_PPLUS_API
    {
        public static Task<PPLUS> Connect(string address, string user, string password, Log log)
        {
            if (!Uri.TryCreate(address, UriKind.Absolute, out var uri)) throw new ArgumentException($"'{address}' ist keine gültige Addresse", nameof(address));

            return Task.Run(() =>
            {
                if (uri.Scheme == "demo")
                {
                    return new PPLUS(new Demo_PPLUS_Handler(), Token.Demo());
                }

                log.Debug($"Verbinde mit Server {address}");
                switch (Authenticate(uri, user, password, log))
                {
                    case Authenticated authenticated:
                        return new PPLUS(authenticated.Handler, authenticated.Token);

                    case Not_Authenticated _:
                        throw new AuthenticationException("Nicht authentifiziert");

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        private static Authentication_Result Authenticate(Uri uri, string user, string password, Log log)
        {
            var pplusHandler = new Port($"{uri.Scheme}://{uri.Host}", uri.Port, log);
            var token = pplusHandler.Authenticate(user, password).Result;

            return token.HasValue
                ? (Authentication_Result) new Authenticated(pplusHandler, token.Value)
                : new Not_Authenticated();
        }

        private readonly PPLUS_Handler _pplusHandler;
        private readonly Token _token;

        private PPLUS(PPLUS_Handler pplusHandler, Token token)
        {
            _pplusHandler = pplusHandler;
            _token = token;
        }

        public int Auswahllisten_Version => 1;

        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen()
        {
            return Handle_Query<Mitarbeiterliste_V1, Stammdaten<Mitarbeiter>>(
                new Mitarbeiter_abrufen_V1(),
                Message_mapper.Mitarbeiterlist_als_Stammdaten);
        }

        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen_ab(Datenstand stand)
        {
            return Handle_Query<Mitarbeiterliste_V1, Stammdaten<Mitarbeiter>>(
                new Mitarbeiter_abrufen_ab_V1(Message_mapper.Stand_als_Message(stand)),
                Message_mapper.Mitarbeiterlist_als_Stammdaten);
        }

        public Task<Stammdaten<Mitarbeiterfoto>> Mitarbeiterfotos_abrufen()
        {
            return Handle_Query<Mitarbeiterfotos_V1, Stammdaten<Mitarbeiterfoto>>(
                new Mitarbeiterfotos_abrufen_V1(),
                Message_mapper.Mitarbeiterfotos_als_Stammdaten);
        }

        public Task<Stammdaten<Mitarbeiterfoto>> Mitarbeiterfotos_abrufen_ab(Datenstand stand)
        {
            return Handle_Query<Mitarbeiterfotos_V1, Stammdaten<Mitarbeiterfoto>>(
                new Mitarbeiterfotos_abrufen_ab_V1(Message_mapper.Stand_als_Message(stand)),
                Message_mapper.Mitarbeiterfotos_als_Stammdaten);
        }

        public Task<Stammdaten<Dienst>> Dienste_abrufen()
        {
            return Handle_Query<Dienste_V1, Stammdaten<Dienst>>(
                new Dienste_abrufen_V1(),
                Message_mapper.Dienste_als_Stammdaten);
        }

        public Task<Stammdaten<Dienst>> Dienste_abrufen_ab(Datenstand stand)
        {
            return Handle_Query<Dienste_V1, Stammdaten<Dienst>>(
                new Dienste_abrufen_ab_V1(Message_mapper.Stand_als_Message(stand)),
                Message_mapper.Dienste_als_Stammdaten);
        }

        private async Task<TResult> Handle_Query<TResponse, TResult>(Query query, Func<TResponse, TResult> handler)
        {
            var response = await _pplusHandler.HandleQuery(new Query_Message(_token.Value, query));
            switch (response)
            {
                case TResponse message:
                {
                    return handler(message);
                }

                case Query_Failed error:
                {
                    throw new Exception(error.Reason);
                }

                default:
                    throw new Exception($"Unerwartetes Response '{response.GetType()}' erhalten");
            }
        }

        public void Dispose()
        {
        }
    }
}