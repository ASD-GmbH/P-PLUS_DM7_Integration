using System;
using System.Threading.Tasks;
using Bare.Msg;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration.Implementierung.V2
{
    public class PPLUS : DM7_PPLUS_API
    {
        public static Task<PPLUS> Connect(string address)
        {
            var uri = new Uri(address);

            return Task.Run(() =>
            {
                var backend =
                    uri.Scheme == "demo"
                        ? new Demo_Backend()
                        : (Backend)new Port($"{uri.Scheme}://{uri.Host}", uri.Port);
                return new PPLUS(backend);
            });
        }

        private readonly Backend _backend;

        private PPLUS(Backend backend)
        {
            _backend = backend;
        }

        public int Auswahllisten_Version { get; }
        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen()
        {
            return Task.Run(() =>
            {
                var response = _backend.HandleQuery(new Mitarbeiter_abrufen_V1()).Result;
                return Handle_Response_Query_Result<Mitarbeiterliste_V1, Stammdaten<Mitarbeiter>>(response, Message_mapper.Mitarbeiterlist_als_Stammdaten);
            });
        }

        public Task<Stammdaten<Mitarbeiter>> Mitarbeiter_abrufen_ab(Datenstand stand)
        {
            return Task.Run(() =>
            {
                var response = _backend.HandleQuery(new Mitarbeiter_abrufen_ab_V1(Message_mapper.Stand_als_Message(stand))).Result;
                return Handle_Response_Query_Result<Mitarbeiterliste_V1, Stammdaten<Mitarbeiter>>(response, Message_mapper.Mitarbeiterlist_als_Stammdaten);
            });
        }

        private static TOut Handle_Response_Query_Result<T, TOut>(Response response, Func<T, TOut> handler)
        {
            switch (response)
            {
                case T message:
                {
                    return handler(message);
                }

                case Query_Failed error:
                {
                    throw new Exception(error.Reason);
                }

                default:
                    throw new Exception($"Unerwartetes Response '{response}' erhalten");
            }
        }

        public void Dispose()
        {
        }
    }
}