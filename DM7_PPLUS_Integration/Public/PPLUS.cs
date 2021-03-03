using System;
using System.Collections.ObjectModel;
using System.Security.Authentication;
using System.Threading.Tasks;
using Bare.Msg;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung;
using Datum = DM7_PPLUS_Integration.Daten.Datum;
using System.Collections.Generic;
using System.Linq;

namespace DM7_PPLUS_Integration
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
        public static Task<PPLUS> Connect(string address, string user, string password, string encryptionKey, Log log)
        {
            if (!Uri.TryCreate(address, UriKind.Absolute, out var uri)) throw new ArgumentException($"'{address}' ist keine gültige Addresse", nameof(address));

            return Task.Run(() =>
            {
                log.Debug($"Verbinde mit Server {address}");
                switch (Authenticate(uri, user, password, encryptionKey, log))
                {
                    case Authenticated authenticated:
                    {
                        var (_, missing_capabilities) = Negotiate_capabilities(authenticated.Handler.Capabilities().Result.Value.ToList());
                        Guard_no_missing_capabilities(missing_capabilities);
                        return new PPLUS(authenticated.Handler, authenticated.Token);
                    }

                    case Not_Authenticated _:
                        throw new AuthenticationException("Nicht authentifiziert");

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        private static (List<Capability>, List<string>) Negotiate_capabilities(List<Capability> server_capabilities)
        {
            var best_fitting_capabilites = new List<Capability>();
            var missing_capabilities = new List<string>();

            void Evaluate(string nicht_zutreffend, params Capability[] one_of_this)
            {
                foreach (var capability in one_of_this)
                {
                    if (server_capabilities.Contains(capability))
                    {
                        best_fitting_capabilites.Add(capability);
                        return;
                    }
                }
                missing_capabilities.Add(nicht_zutreffend);
            }

            Evaluate("Mitarbeiter Version 1", Capability.MITARBEITER_V1);
            Evaluate("Dienste Version 1", Capability.DIENSTE_V1);
            Evaluate("Abwesenheiten Version 1", Capability.ABWESENHEITEN_V1);
            Evaluate("Dienstbuchungen Version 1", Capability.DIENSTBUCHUNGEN_V1);
            Evaluate("Soll/Ist Abgleich Version 1", Capability.SOLL_IST_ABGLEICH_V1);

            return (best_fitting_capabilites, missing_capabilities);
        }

        private static Authentication_Result Authenticate(Uri uri, string user, string password, string encryptionKey, Log log)
        {
            var pplusHandler = new Port($"{uri.Scheme}://{uri.Host}", uri.Port, encryptionKey, log);
            var token = pplusHandler.Authenticate(user, password).Result;

            return token.HasValue
                ? (Authentication_Result) new Authenticated(pplusHandler, token.Value)
                : new Not_Authenticated();
        }

        private static void Guard_no_missing_capabilities(List<string> missing_capabilities)
        {
            if (missing_capabilities.Any()) throw new NotSupportedException($"Folgende Capabilities nicht unterstützt: {string.Join(", ", missing_capabilities)}");
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
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities().Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.MITARBEITER_V1))
                return Handle_Query<Mitarbeiterliste_V1, Stammdaten<Mitarbeiter>>(
                    new Mitarbeiter_abrufen_V1(),
                    Message_mapper.Mitarbeiterlist_als_Stammdaten);
            
            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen_ab(Datenstand stand)
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities().Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.MITARBEITER_V1))
                return Handle_Query<Mitarbeiterliste_V1, Stammdaten<Mitarbeiter>>(
                    new Mitarbeiter_abrufen_ab_V1(Message_mapper.Stand_als_Message(stand)),
                    Message_mapper.Mitarbeiterlist_als_Stammdaten);
            
            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<Stammdaten<Dienst>> Dienste_abrufen()
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities().Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.DIENSTE_V1))
                return Handle_Query<Dienste_V1, Stammdaten<Dienst>>(
                    new Dienste_abrufen_V1(),
                    Message_mapper.Dienste_als_Stammdaten);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<Stammdaten<Dienst>> Dienste_abrufen_ab(Datenstand stand)
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities().Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.DIENSTE_V1))
                return Handle_Query<Dienste_V1, Stammdaten<Dienst>>(
                    new Dienste_abrufen_ab_V1(Message_mapper.Stand_als_Message(stand)),
                    Message_mapper.Dienste_als_Stammdaten);
            
            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<ReadOnlyCollection<Dienstbuchung>> Dienstbuchungen_zum_Stichtag(Datum stichtag, Guid mandantId)
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities().Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.DIENSTBUCHUNGEN_V1))
                return Handle_Query<Dienstbuchungen_V1, ReadOnlyCollection<Dienstbuchung>>(
                    new Dienstbuchungen_zum_Stichtag_V1(Message_mapper.Datum_als_Message(stichtag), Message_mapper.UUID_aus(mandantId)),
                    Message_mapper.Dienstbuchungen);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<ReadOnlyCollection<Abwesenheit>> Abwesenheiten_zum_Stichtag(Datum stichtag, Guid mandantId)
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities().Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.ABWESENHEITEN_V1))
                return Handle_Query<Abwesenheiten_V1, ReadOnlyCollection<Abwesenheit>>(
                new Abwesenheiten_zum_Stichtag_V1(Message_mapper.Datum_als_Message(stichtag), Message_mapper.UUID_aus(mandantId)),
                Message_mapper.Abwesenheiten);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<Soll_Ist_Abgleich_Verarbeitungsergebnis> Soll_Ist_Abgleich_freigeben(Soll_Ist_Abgleich abgleich)
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities().Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.SOLL_IST_ABGLEICH_V1))
                return Handle_Command<Soll_Ist_Abgleich_Verarbeitungsergebnis_V1, Soll_Ist_Abgleich_Verarbeitungsergebnis>(
                    new Soll_Ist_Abgleich_freigeben_V1(Message_mapper.Soll_Ist_Abgleich_als_Message(abgleich)),
                    Message_mapper.Soll_Ist_Abgleich_Verarbeitungsergebnis);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        private async Task<TResult> Handle_Query<TResponse, TResult>(Query query, Func<TResponse, TResult> handler)
        {
            var response = await _pplusHandler.HandleQuery(_token, query);
            switch (response)
            {
                case TResponse message:
                {
                    return handler(message);
                }

                case IO_Fehler error:
                {
                    throw new Exception(error.Reason);
                }

                default:
                    throw new Exception($"Unerwartetes Response '{response.GetType()}' erhalten");
            }
        }
        private async Task<TResult> Handle_Command<TResponse, TResult>(Command command, Func<TResponse, TResult> handler)
        {
            var response = await _pplusHandler.HandleCommand(_token, command);
            switch (response)
            {
                case TResponse message:
                {
                    return handler(message);
                }

                case IO_Fehler error:
                {
                    throw new Exception(error.Reason);
                }

                default:
                    throw new Exception($"Unerwartetes Response '{response.GetType()}' erhalten");
            }
        }
    }
}