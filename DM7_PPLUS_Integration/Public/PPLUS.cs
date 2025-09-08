using System;
using System.Collections.ObjectModel;
using System.Security.Authentication;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung;
using System.Collections.Generic;
using System.Linq;
using DM7_PPLUS_Integration.Implementierung.PPLUS;
using DM7_PPLUS_Integration.Messages.PPLUS;
using Datenstand = DM7_PPLUS_Integration.Daten.Datenstand;
using Datum = DM7_PPLUS_Integration.Daten.Datum;
using Dienstbuchung = DM7_PPLUS_Integration.Daten.Dienstbuchung;
using Uhrzeit = DM7_PPLUS_Integration.Daten.Uhrzeit;

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

    public class PPLUS : PPLUS_API
    {
        public static Task<PPLUS> Connect(string address, string user, string password, string encryptionKey, Log log, TimeSpan? timeout = null)
        {
            if (!Uri.TryCreate(address, UriKind.Absolute, out var uri)) throw new ArgumentException($"'{address}' ist keine gültige Addresse", nameof(address));

            return Task.Run(() =>
            {
                log.Debug($"Verbinde mit Server {address}");
                switch (Authenticate(uri, user, password, encryptionKey, log, timeout))
                {
                    case Authenticated authenticated:
                        {
                            try
                            {
                                var (_, missing_capabilities) = Negotiate_capabilities(authenticated.Handler.Capabilities(timeout).Result.Value.ToList());
                                Guard_no_missing_capabilities(missing_capabilities);
                                return new PPLUS(authenticated.Handler, authenticated.Token, timeout);
                            }
                            catch
                            {
                                authenticated.Handler.Dispose();
                                throw;
                            }
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

            Evaluate("Mitarbeiter - Version 1", Capability.MITARBEITER_V1);
            Evaluate("Dienste - Version 1", Capability.DIENSTE_V1);
            Evaluate("Beginn (und Ende) von Dienst", Capability.BEGINN_UND_ENDE_VON_DIENST_V1, Capability.BEGINN_VON_DIENST_V1);
            Evaluate("Abwesenheiten - Version 1", Capability.ABWESENHEITEN_V1);
            Evaluate("Dienstbuchungen", Capability.DIENSTBUCHUNGEN_V2, Capability.DIENSTBUCHUNGEN_V1);
            Evaluate("Soll/Ist Abgleich - Version 1", Capability.SOLL_IST_ABGLEICH_V1);
            Evaluate("Dienstbuchungsüberwachungszeitraum", Capability.DIENSTBUCHUNGSUEBERWACHUNGSZEITRAUM_V1);

            return (best_fitting_capabilites, missing_capabilities);
        }

        private static Authentication_Result Authenticate(Uri uri, string user, string password, string encryptionKey, Log log, TimeSpan? timeout)
        {
            Port pplusHandler = null;

            try
            {
                pplusHandler = new Port($"{uri.Scheme}://{uri.Host}", uri.Port, encryptionKey, log);
                var token = pplusHandler.Authenticate(user, password, timeout).Result;

                if (token.HasValue) return new Authenticated(pplusHandler, token.Value);
            }
            catch (Exception ex)
            {
                log.Info($"PPLUS - Authenticate - ErrorMessage: {ex}\r\nStackTrace:\r\n{ex.StackTrace}");

                pplusHandler?.Dispose();
                throw;
            }

            pplusHandler.Dispose();
            return new Not_Authenticated();
        }
        
        private static void Guard_no_missing_capabilities(List<string> missing_capabilities)
        {
            if (missing_capabilities.Any()) throw new NotSupportedException($"Folgende Capabilities nicht unterstützt: {string.Join(", ", missing_capabilities)}");
        }

        private readonly PPLUS_Handler _pplusHandler;
        private readonly Token _token;
        private readonly TimeSpan? _timeout;

        private PPLUS(PPLUS_Handler pplusHandler, Token token, TimeSpan? timeout)
        {
            _pplusHandler = pplusHandler;
            pplusHandler.Dienständerungen_liegen_bereit += () => Dienständerungen_liegen_bereit?.Invoke();
            pplusHandler.Mitarbeiteränderungen_liegen_bereit += () => Mitarbeiteränderungen_liegen_bereit?.Invoke();
            pplusHandler.Dienstbuchungsänderungen_liegen_bereit += () => Dienstbuchungsänderungen_liegen_bereit?.Invoke();
            _token = token;
            _timeout = timeout;
        }

        public int Auswahllisten_Version => 1;
        public event Action Mitarbeiteränderungen_liegen_bereit;
        public event Action Dienständerungen_liegen_bereit;
        public event Action Dienstbuchungsänderungen_liegen_bereit;

        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen()
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities(_timeout).Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.MITARBEITER_V1))
                return Handle_Query<MitarbeiterlisteV1, Stammdaten<Mitarbeiter>>(
                    new MitarbeiterAbrufenV1(),
                    Message_mapper.Mitarbeiterlist_als_Stammdaten);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen_ab(Datenstand stand)
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities(_timeout).Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.MITARBEITER_V1))
                return Handle_Query<MitarbeiterlisteV1, Stammdaten<Mitarbeiter>>(
                    new MitarbeiterAbrufenAbV1(Message_mapper.Stand_als_Message(stand)),
                    Message_mapper.Mitarbeiterlist_als_Stammdaten);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<Stammdaten<Dienst>> Dienste_abrufen()
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities(_timeout).Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.DIENSTE_V1))
                return Handle_Query<DiensteV1, Stammdaten<Dienst>>(
                    new DiensteAbrufenV1(),
                    Message_mapper.Dienste_als_Stammdaten);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<(Uhrzeit?,Uhrzeit?)> DienstBeginnUndEnde_am(Datum stichtag, int dienstId)
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities(_timeout).Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.BEGINN_UND_ENDE_VON_DIENST_V1))
                return Handle_Query<DienstBeginnUndEndeV1, (Uhrzeit?, Uhrzeit?)>(
                    new DienstBeginnUndEndeZumStichtagV1(Message_mapper.Datum_als_Message(stichtag), (ulong) dienstId),
                    Message_mapper.DienstBeginnUndEnde_aus_Message);

            if (best_fitting.Contains(Capability.BEGINN_VON_DIENST_V1))
                return Handle_Query<DienstbeginnV1, (Uhrzeit?, Uhrzeit?)>(
                    new DienstbeginnZumStichtagV1(Message_mapper.Datum_als_Message(stichtag), (ulong)dienstId),
                    Message_mapper.Dienstbeginn_aus_Message);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<Stammdaten<Dienst>> Dienste_abrufen_ab(Datenstand stand)
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities(_timeout).Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.DIENSTE_V1))
                return Handle_Query<DiensteV1, Stammdaten<Dienst>>(
                    new DiensteAbrufenAbV1(Message_mapper.Stand_als_Message(stand)),
                    Message_mapper.Dienste_als_Stammdaten);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<Dictionary<Datum, ReadOnlyCollection<Dienstbuchung>>> Dienstbuchungen_im_Zeitraum(Datum von, Datum bis, Guid mandantId)
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities(_timeout).Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.DIENSTBUCHUNGEN_V2))
                return Handle_Query<DienstbuchungenV2, Dictionary<Datum, ReadOnlyCollection<Dienstbuchung>>>(
                    new DienstbuchungenImZeitraumV1(Message_mapper.Datum_als_Message(von), Message_mapper.Datum_als_Message(bis), Message_mapper.UUID_aus(mandantId)),
                    Message_mapper.Dienstbuchungen);

            if (best_fitting.Contains(Capability.DIENSTBUCHUNGEN_V1))
                return Handle_Query<DienstbuchungenV1, Dictionary<Datum, ReadOnlyCollection<Dienstbuchung>>>(
                    new DienstbuchungenImZeitraumV1(Message_mapper.Datum_als_Message(von), Message_mapper.Datum_als_Message(bis), Message_mapper.UUID_aus(mandantId)),
                    Message_mapper.Dienstbuchungen);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<ReadOnlyCollection<Abwesenheit>> Abwesenheiten_im_Zeitraum(Datum von, Datum bis, Guid mandantId)
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities(_timeout).Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.ABWESENHEITEN_V1))
                return Handle_Query<AbwesenheitenV1, ReadOnlyCollection<Abwesenheit>>(
                    new AbwesenheitenImZeitraumV1(Message_mapper.Datum_als_Message(von), Message_mapper.Datum_als_Message(bis), Message_mapper.UUID_aus(mandantId)),
                    Message_mapper.Abwesenheiten);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<AnzahlTage> DienstbuchungsUeberwachungszeitraum_abrufen()
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities(_timeout).Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.DIENSTBUCHUNGSUEBERWACHUNGSZEITRAUM_V1)) 
                return Handle_Query<AnzahlTageV1, AnzahlTage>(
                    new DienstbuchungsUeberwachungszeitraumV1(),
                    Message_mapper.AnzahlTage_aus);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<Soll_Ist_Abgleich_Verarbeitungsergebnis> Soll_Ist_Abgleich_freigeben(Soll_Ist_Abgleich abgleich)
        {
            var (best_fitting, missing) = Negotiate_capabilities(_pplusHandler.Capabilities(_timeout).Result.Value.ToList());
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.SOLL_IST_ABGLEICH_V1))
                return Handle_Command<SollIstAbgleichVerarbeitungsergebnisV1, Soll_Ist_Abgleich_Verarbeitungsergebnis>(
                    new SollIstAbgleichFreigebenV1(Message_mapper.Soll_Ist_Abgleich_als_Message(abgleich)),
                    Message_mapper.Soll_Ist_Abgleich_Verarbeitungsergebnis);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        private async Task<TResult> Handle_Query<TResponse, TResult>(Query query, Func<TResponse, TResult> handler)
        {
            var response = await _pplusHandler.HandleQuery(_token, query, _timeout);
            switch (response)
            {
                case TResponse message:
                {
                    return handler(message);
                }

                case IOFehler error:
                {
                    throw new Exception(error.Reason);
                }

                default:
                    throw new Exception($"Unerwartetes Response '{response.GetType()}' erhalten");
            }
        }
        private async Task<TResult> Handle_Command<TResponse, TResult>(Command command, Func<TResponse, TResult> handler)
        {
            var response = await _pplusHandler.HandleCommand(_token, command, _timeout);
            switch (response)
            {
                case TResponse message:
                {
                    return handler(message);
                }

                case IOFehler error:
                {
                    throw new Exception(error.Reason);
                }

                default:
                    throw new Exception($"Unerwartetes Response '{response.GetType()}' erhalten");
            }
        }

        public void Dispose()
        {
            _pplusHandler?.Dispose();
        }
    }
}