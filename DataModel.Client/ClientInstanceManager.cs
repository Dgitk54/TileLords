using DataModel.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Client
{
    /// <summary>
    /// Class for managing a clientinstance with autoreconnect functionality
    /// </summary>
    public class ClientInstanceManager
    {
        const int SENDLOGINMESSAGEDELAY = 2;
        const int RECONNECTWAITTIME = 5;
        const int RECONNECTSTATERESET = 5;
        readonly Subject<UnityMapMessage> mapForwarding = new Subject<UnityMapMessage>();
        readonly Subject<IMessage> outboundTraffic = new Subject<IMessage>();
        readonly Subject<IMessage> inboundTraffic = new Subject<IMessage>();
        readonly Subject<bool> connectionState = new Subject<bool>();
        readonly BehaviorSubject<bool> reconnectionState = new BehaviorSubject<bool>(false);
        readonly string ip;
        readonly int port;
        readonly List<IDisposable> forwardDisposables = new List<IDisposable>();
        readonly List<IDisposable> reconnectDisposables = new List<IDisposable>();
        
        ClientInstance instance;
        Task runningClient;
        bool hasBeenRunningFlag = false;
        bool shouldLogConsole;

        public ClientInstanceManager(string ipAdress = "127.0.0.1", int port = 8080, bool shouldLogConsole = false)
        {
            ip = ipAdress;
            this.port = port;
            this.shouldLogConsole = shouldLogConsole;

            var autoRelog = connectionState.Where(v => v).WithLatestFrom(reconnectionState, (connection, reconnect) => new { connection, reconnect })
                                                         .Where(v => v.reconnect)
                                                         .WithLatestFrom(GetWorkingAccountDetails(inboundTraffic, outboundTraffic), (states, account) => new { states, account })
                                                         .Subscribe(v =>
                                                         {
                                                             outboundTraffic.OnNext(v.account);
                                                         });

            var activateReconnectionState = GetWorkingAccountDetails(InboundTraffic, outboundTraffic).CombineLatest(connectionState, (account, connection) => new { account, connection })
                                                                                                     .Where(v => !v.connection)
                                                                                                     .Subscribe(v => reconnectionState.OnNext(true));

            var autoReconnect = Observable.Interval(TimeSpan.FromSeconds(RECONNECTWAITTIME)).WithLatestFrom(connectionState, (timer, state) => new { timer, state })
                                                                                            .Where(v => !v.state)
                                                                                            .Where(v => hasBeenRunningFlag)
                                                                                            .Do(v => Console.WriteLine("CreateNewInstanceAndSubscribe"))
                                                                                            .Subscribe(v => 
                                                                                            {
                                                                                                try
                                                                                                {
                                                                                                CreateNewInstanceAndSubscribe();
                                                                                                }
                                                                                                catch (Exception e)
                                                                                                {
                                                                                                    Console.WriteLine("Could not connect with the client instance");
                                                                                                }
                                                                                            });


            var reconnectionResetDisposable = connectionState.Where(v => v)
                                                             .Delay(TimeSpan.FromSeconds(RECONNECTSTATERESET))
                                                             .Subscribe(v => reconnectionState.OnNext(false));

            var outboundTrafficForward = outboundTraffic.Subscribe(v =>
            {
                
                if (instance != null)
                    instance.SendMessage(v);
            });

            var consoleContentLogger = inboundTraffic.OfType<BatchContentMessage>().Select(v=> v.ContentList).Subscribe(v => 
            {
                if (shouldLogConsole)
                {
                    string visibleContentString = "\n \n";
                    visibleContentString += "Visible Contentamount:" + v.Count + " \n";
                    var players = v.Where(v2 => v2.Type == ContentType.PLAYER).Select(v2 => "[Player:" + v2.Name + " id:" + v2.Id + " on location " + v2.Location + "] \n").ToList();
                    var resources = v.Where(v2 => v2.Type == ContentType.RESOURCE).Select(v2 => "[Resource:" + v2.Name + " id:" + v2.Id + " on location " + v2.Location + "] \n").ToList();
                    players.ForEach(v2 => visibleContentString += v2);
                    resources.ForEach(v2 => visibleContentString += v2);
                    Console.WriteLine(visibleContentString);
                }
            } );

            reconnectDisposables.Add(reconnectionResetDisposable);
            reconnectDisposables.Add(autoRelog);
            reconnectDisposables.Add(autoReconnect);
            reconnectDisposables.Add(outboundTrafficForward);
            reconnectDisposables.Add(activateReconnectionState);
            reconnectDisposables.Add(consoleContentLogger);


        }

        public IObservable<UnityMapMessage> ClientMapStream => mapForwarding.AsObservable();

        public IObservable<IMessage> InboundTraffic => inboundTraffic.AsObservable();

        public IObservable<bool> ClientConnectionState => connectionState.AsObservable();

        public IObservable<bool> ReconnectionState => reconnectionState.AsObservable();

        public void SendMessage(IMessage msg)
        {
            outboundTraffic.OnNext(msg);
        }
        public void StartClient()
        {
            if (hasBeenRunningFlag)
                throw new Exception("Can not start a ClientInstanceManager once it has been shut down!");
            CreateNewInstanceAndSubscribe();
            hasBeenRunningFlag = true;
        }

        public void ShutDown()
        {
            reconnectDisposables.ForEach(v => v.Dispose());
            StopRunningClientInstance();
        }

        void CreateNewInstanceAndSubscribe()
        {
            StopRunningClientInstance();
            instance = new ClientInstance();

            
            var disp1 = instance.InboundTraffic.Subscribe(v => inboundTraffic.OnNext(v));

            var disp2 = instance.ClientConnectionState.Subscribe(v => connectionState.OnNext(v));

            var disp3 = instance.ClientMapStream.Subscribe(v => mapForwarding.OnNext(v));
           
            forwardDisposables.Add(disp1);
            forwardDisposables.Add(disp2);
            forwardDisposables.Add(disp3);
          
            runningClient = ClientFunctions.StartClient(instance,ip,port);
        }
        void StopRunningClientInstance()
        {
            forwardDisposables.ForEach(v => v.Dispose());
            if(instance != null)
            {
                instance.CloseClient();
            }
            if(runningClient != null)
            {
                runningClient.Wait();
            }
            instance = null;
            runningClient = null;
            forwardDisposables.Clear();
        }
        

        IObservable<AccountMessage> GetWorkingAccountDetails(IObservable<IMessage> inbound, IObservable<IMessage> outbound) 
        {
            var inboundLoginSuccess = inbound.Where(v => v is UserActionMessage)
                          .Select(v => v as UserActionMessage)
                          .Where(v => v.MessageContext == MessageContext.LOGIN && v.MessageState == MessageState.SUCCESS);

            var outboundLoginRequests = outbound.Where(v => v is AccountMessage)
                                                .Select(v => v as AccountMessage);

            return inboundLoginSuccess.WithLatestFrom(outboundLoginRequests, (successMessage, loginMessage) => new { successMessage, loginMessage })
                                      .Select(v=> v.loginMessage)
                                      .Take(1);
        }
    }
}
