using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DM7_PPLUS_Integration.Daten;
using DM7_PPLUS_Integration.Implementierung.Protokoll;
using DM7_PPLUS_Integration.Implementierung.Shared;
using NetMQ;
using NetMQ.Sockets;

namespace DM7_PPLUS_Integration.Implementierung.V2.Protokoll
{
    public interface Query {}

    public readonly struct Mitarbeiterdatensätze_V1 : Query {}

    public readonly struct Mitarbeiterdatensätze_für_Stand_V1 : Query
    {
        public readonly Stand Von;
        public readonly Stand Bis;

        public Mitarbeiterdatensätze_für_Stand_V1(Stand von, Stand bis)
        {
            Von = von;
            Bis = bis;
        }
    }

    public readonly struct Mitarbeiterdatensätze_V3 : Query { }

    public readonly struct Mitarbeiterdatensätze_für_Stand_V3 : Query
    {
        public readonly Stand Von;
        public readonly Stand Bis;

        public Mitarbeiterdatensätze_für_Stand_V3(Stand von, Stand bis)
        {
            Von = von;
            Bis = bis;
        }
    }

    public interface Server : IDisposable
    {
        Task<List<Capability>> Capabilities();
        Task<QueryResponse> HandleQuery(Query query);
        IObservable<Notification> Notifications { get; }
    }

    public class DemoServer : Server
    {
        public Task<List<Capability>> Capabilities()
        {
            return Task.FromResult(new List<Capability>
            {
                Protokoll.Capabilities.Legacy_V3
            });
        }

        public Task<QueryResponse> HandleQuery(Query query)
        {
            return Task.FromResult<QueryResponse>(new QueryFailed(QueryFailure.API_Version_no_longer_available, "nicht unterstützt"));
        }

        public IObservable<Notification> Notifications => new Subject<Notification>();
        public void Dispose()
        {
            // does nothing
        }
    }

    public interface Client_Question_Message {}
    public readonly struct What_are_your_Capabilities : Client_Question_Message {}
    public readonly struct Server_Query : Client_Question_Message
    {
        public readonly Query Query;

        public Server_Query(Query query)
        {
            Query = query;
        }
    }

    public interface Server_Answer_Message {}
    public readonly struct These_are_my_Capabilities : Server_Answer_Message
    {
        public readonly List<Capability> Capabilities;

        public These_are_my_Capabilities(List<Capability> capabilities)
        {
            Capabilities = capabilities;
        }
    }
    public readonly struct Query_Response : Server_Answer_Message
    {
        public readonly QueryResponse Response;

        public Query_Response(QueryResponse response)
        {
            Response = response;
        }
    }
    public readonly struct Sorry_I_could_not_understand_you : Server_Answer_Message {}

    public class ServerHost : IDisposable
    {
        private readonly Server _server;
        private readonly NetMQPoller _poller;
        private readonly Thread _thread;

        public ServerHost(Server server, string bindAdress, int portRangeStart)
        {
            _server = server;
            _poller = new NetMQPoller();
            _thread = new Thread(() =>
            {
                using (var publisherSocket = new PublisherSocket())
                using (var socket = new RouterSocket())
                {
                    socket.Bind($"tcp://{bindAdress}:{portRangeStart}");
                    socket.ReceiveReady += (_, args) => Handle_Query(args.Socket);

                    publisherSocket.Bind($"tcp://{bindAdress}:{portRangeStart + 1}");
                    var notificationSubscription =
                        server.Notifications
                            // ReSharper disable once AccessToDisposedClosure
                            .Subscribe(new Observer<Notification>(notification => Send_Notification(notification, publisherSocket), _ => { }));

                    using (notificationSubscription)
                    using (_poller)
                    {
                        _poller.Add(socket);
                        _poller.Run();
                    }
                }
            });
        }

        private static void Send_Notification(Notification notification, NetMQSocket socket)
        {
            socket.SendFrame("notifications");
            // TODO: Serializing + Encryption
            // socket.Send(notification)
        }

        private void Handle_Query(NetMQSocket socket)
        {
            var caller = socket.ReceiveFrameBytes();
            socket.SkipFrame();
            var answer = Answer_Message(Decode_Message(socket), _server);
            Send_Message(caller, answer, socket);
        }

        private static Server_Answer_Message Answer_Message(Client_Question_Message message, Server server)
        {
            switch (message)
            {
                case What_are_your_Capabilities _:
                    return new These_are_my_Capabilities(server.Capabilities().Result);

                case Server_Query msg:
                    return new Query_Response(server.HandleQuery(msg.Query).Result);

                default:
                    return new Sorry_I_could_not_understand_you();
            }
        }

        private static Client_Question_Message Decode_Message(NetMQSocket socket)
        {
            // TODO: Deserializing + Decryption
            return new What_are_your_Capabilities();
        }

        private static void Send_Message(byte[] caller, Server_Answer_Message msg, NetMQSocket socket)
        {
            socket.SendFrame(caller);
            socket.SendFrameEmpty();
            // TODO: Serializing + Encryption
            // socket.send(msg)
        }

        public void Dispose()
        {
            _poller.Stop();
            var gracefullyStopped = _thread.Join(TimeSpan.FromSeconds(10));
            if (!gracefullyStopped) _thread.Abort();
        }
    }

    public class Server_Proxy : Server
    {
        private readonly string _address;
        private readonly int _portRangeStart;
        private readonly NetMQPoller _poller;
        private readonly Thread _subscriptionThread;

        public Server_Proxy(string address, int portRangeStart)
        {
            _address = address;
            _portRangeStart = portRangeStart;
            _poller = new NetMQPoller();

            var subject = new Subject<Notification>();
            Notifications = subject;
            _subscriptionThread = new Thread(() =>
            {
                using (var subscriberSocket = new SubscriberSocket($"tcp://{address}:{portRangeStart+1}"))
                {
                    subscriberSocket.Subscribe("notifications");
                    subscriberSocket.ReceiveReady += (_,args) => Receive_Notification(subject, args.Socket);

                    using (_poller)
                    {
                        _poller.Add(subscriberSocket);
                        _poller.Run();
                    }
                }
            });
        }

        private static void Receive_Notification(Subject<Notification> subject, NetMQSocket socket)
        {
            // TODO: Deserializing + Decryption
            // var notification = socket.ReceiveFrame()
        }

        public Task<List<Capability>> Capabilities()
        {
            return Task.Run(async () =>
            {
                var answer = await Send_Message(new What_are_your_Capabilities()); // TODO: Seperater Kanal für Server Verbindung Msgs
                switch (answer)
                {
                    case These_are_my_Capabilities msg:
                        return msg.Capabilities;

                    case Sorry_I_could_not_understand_you _:
                        throw new IOException("Server kennt die Frage nach Capabilities nicht");

                    default:
                        throw new IOException($"Unerwartete Antwort {answer.GetType()} bei der Frage nach Capabilities erhalten");
                }
            });
        }

        public Task<QueryResponse> HandleQuery(Query query)
        {
            return Task.Run(async () =>
            {
                var answer = await Send_Message(new Server_Query(query));
                switch (answer)
                {
                    case Query_Response msg:
                        return msg.Response;

                    case Sorry_I_could_not_understand_you _:
                        throw new IOException("Server kann keine Queries entgegen nehmen");

                    default:
                        throw new IOException($"Unerwartete Antwort {answer.GetType()} beim Stellen einer Query erhalten");
                }
            });
        }

        private Task<Server_Answer_Message> Send_Message(Client_Question_Message message)
        {
            return Task.Run(() =>
            {
                using (var socket = new RequestSocket($"tcp://{_address}:{_portRangeStart}"))
                {
                    Encode_Message(message, socket);
                    return Decode_Answer(socket);
                }
            });
        }

        private static void Encode_Message(Client_Question_Message message, NetMQSocket socket)
        {
            // TODO: Serializing + Encryption
            // socket.Send(message)
        }

        private static Server_Answer_Message Decode_Answer(NetMQSocket socket)
        {
            // TODO: Deserializing + Decryption
            // socket.ReceiveFrameBytes()
            return new Sorry_I_could_not_understand_you();
        }

        public IObservable<Notification> Notifications { get; }
        public void Dispose()
        {
            _poller.Stop();
            var gracefullyStopped = _subscriptionThread.Join(TimeSpan.FromSeconds(10));
            if (!gracefullyStopped) _subscriptionThread.Abort();
        }
    }

    public readonly struct ConnectionEstablished : ConnectionResult
    {
        public readonly Server Server;

        public ConnectionEstablished(Server server)
        {
            Server = server;
        }
    }

    public static class Connector
    {
        public static Client Connect(string address, string credentials)
        {
            var uri = new Uri(address);
            if (uri.Scheme == "demo") return Initialize_DemoClient(credentials);
            else if (uri.Scheme == "tcp") 

            var result = Authenticated(uri, credentials);
            if (result is ConnectionEstablished s)
            {
                Benötigte_Capabilities_auswerten(s.Server.Capabilities());
                return new Client(s.Server);
            }
            return null;
        }

        private static void Benötigte_Capabilities_auswerten(Task<List<Capability>> capabilities)
        {
            
        }

        private static ConnectionResult Authenticated(Uri uri, string credentials)
        {
            return null;
        }

        private static Client Initialize_DemoClient(string credentials)
        {
            throw new NotImplementedException();
        }
    }

    public class Client : DM7_PPLUS_API
    {
        private readonly Server _server;

        public Client(Server server)
        {
            _server = server;
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        public int Auswahllisten_Version { get; }
        public IObservable<Stand> Stand_Mitarbeiterdaten { get; }
        public async Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen(Stand von, Stand bis)
        {
            var capabilities = await _server.Capabilities();
            if (capabilities.Contains(Capabilities.Legacy_V3))
            {
                var response = await _server.HandleQuery(new Mitarbeiterdatensätze_für_Stand_V3(von, bis));
                // TODO: Deserializing + Encryption
                return new Mitarbeiterdatensaetze();
            }

            if (capabilities.Contains(Capabilities.Legacy_V1))
            {
                var response = await _server.HandleQuery(new Mitarbeiterdatensätze_für_Stand_V1(von, bis));
                // TODO: Deserializing + Encryption
                return new Mitarbeiterdatensaetze();
            }

            throw new NotSupportedException();
        }

        public async Task<Mitarbeiterdatensaetze> Mitarbeiterdaten_abrufen()
        {
            var capabilities = await _server.Capabilities();
            if (capabilities.Contains(Capabilities.Legacy_V3))
            {
                var response = await _server.HandleQuery(new Mitarbeiterdatensätze_V3());
                // TODO: Deserializing + Encryption
                return new Mitarbeiterdatensaetze();
            }

            if (capabilities.Contains(Capabilities.Legacy_V1))
            {
                var response = await _server.HandleQuery(new Mitarbeiterdatensätze_V1());
                // TODO: Deserializing + Encryption
                return new Mitarbeiterdatensaetze();
            }

            throw new NotSupportedException();
        }
    }

    public static class Capabilities
    {
        public static readonly Capability Legacy_V1 = Capability.Repräsentiert_als(1);
        public static readonly Capability Legacy_V3 = Capability.Repräsentiert_als(2);
        public static readonly Capability Dienste_Abfrage_V1 = Capability.Repräsentiert_als(3);
        public static readonly Capability Dienste_Abfrage_V2 = Capability.Repräsentiert_als(4);
    }

    public readonly struct Capability
    {
        public readonly int Nummer;

        private Capability(int nummer)
        {
            Nummer = nummer;
        }

        public static Capability Repräsentiert_als(int nummer) => new Capability(nummer);
    }
}