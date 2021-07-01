using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Implementierung.DM7;
using DM7_PPLUS_Integration.Messages.DM7;
using DM7_PPLUS_Integration.Daten;

namespace DM7_PPLUS_Integration
{
    public class DM7
    {
        private readonly Port _port;

        private DM7(Port port)
        {
            _port = port;
        }

        public static DM7 Connect(string address, string encryptionKey, Log log, TimeSpan? timeout = null)
        {
            if (!Uri.TryCreate(address, UriKind.Absolute, out var uri)) throw new ArgumentException($"'{address}' ist keine gültige Addresse", nameof(address));

            var port = new Port($"{uri.Scheme}://{uri.Host}", uri.Port, encryptionKey, log, timeout);
            var (_, missing_capabilities) = Negotiate_capabilities(port.Capabilities().Result.Value);
            Guard_no_missing_capabilities(missing_capabilities);
            return new DM7(port);
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

            Evaluate("Leistungen Version 1", Capability.ALLE_LEISTUNGEN_V1);
            Evaluate("Mandanten Version 1", Capability.ALLE_MANDANTEN_V1);

            return (best_fitting_capabilites, missing_capabilities);
        }

        private static void Guard_no_missing_capabilities(List<string> missing_capabilities)
        {
            if (missing_capabilities.Any()) throw new NotSupportedException($"Folgende Capabilities nicht unterstützt: {string.Join(", ", missing_capabilities)}");
        }

        public Task<List<Leistung>> Alle_Leistungen()
        {
            var (best_fitting, missing) = Negotiate_capabilities(_port.Capabilities().Result.Value);
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.ALLE_LEISTUNGEN_V1))
                return Handle_Query<Leistungen_V1, List<Leistung>>(
                    new Alle_leistungen_V1(),
                    Message_mapper.Als_Leistungen);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        public Task<List<DM7_Mandant>> Alle_Mandanten()
        {
            var (best_fitting, missing) = Negotiate_capabilities(_port.Capabilities().Result.Value);
            Guard_no_missing_capabilities(missing);

            if (best_fitting.Contains(Capability.ALLE_MANDANTEN_V1))
                return Handle_Query<Mandanten_V1, List<DM7_Mandant>>(
                    new Alle_mandanten_V1(),
                    Message_mapper.Als_Mandanten);

            throw new ArgumentOutOfRangeException(nameof(best_fitting), best_fitting, null);
        }

        private async Task<TResult> Handle_Query<TResponse, TResult>(Query query, Func<TResponse, TResult> handler)
        {
            var response = await _port.Handle_Query(query);
            switch (response)
            {
                case TResponse message:
                {
                    return handler(message);
                }

                case IO_fehler error:
                {
                    throw new Exception(error.Reason);
                }

                default:
                    throw new Exception($"Unerwartetes Response '{response.GetType()}' erhalten");
            }
        }
    }
}