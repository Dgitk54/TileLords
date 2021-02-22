using DataModel.Common.Messages;
using System;
using System.Collections.Generic;
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

        readonly Subject<UnityMapMessage> mapForwarding = new Subject<UnityMapMessage>();
        readonly Subject<IMessage> outboundTraffic = new Subject<IMessage>();
        readonly Subject<IMessage> inboundTraffic = new Subject<IMessage>();
        readonly Subject<bool> connectionState = new Subject<bool>();
        readonly BehaviorSubject<bool> reconnectinonState = new BehaviorSubject<bool>(false);
        readonly string ip;
        readonly int port;
        readonly List<IDisposable> forwardDisposables = new List<IDisposable>();
        readonly List<IDisposable> reconnectDisposables = new List<IDisposable>();
        
        ClientInstance instance;
        Task runningClient;
        bool hasBeenRunningFlag = false;

        public ClientInstanceManager(string ipAdress = "127.0.0.1", int port = 8080)
        {
            ip = ipAdress;
            this.port = port;
            var reconnectionDisposable = connectionState.Where(v => !v)
                                                    .WithLatestFrom(GetWorkingAccountDetails(inboundTraffic, outboundTraffic), (state, account) => new { state, account })
                                                    .Do(v => reconnectinonState.OnNext(true))
                                                    .Do(v => CreateNewInstanceAndSubscribe())
                                                    .Delay(TimeSpan.FromSeconds(SENDLOGINMESSAGEDELAY))
                                                    .Subscribe(v => outboundTraffic.OnNext(v.account));

            var reconnectionResetDisposable = connectionState.Where(v => v).Subscribe(v => reconnectinonState.OnNext(false));

            var outboundTrafficForward = outboundTraffic.Subscribe(v =>
            {
                
                if (instance != null)
                    instance.SendMessage(v);
            });

            reconnectDisposables.Add(reconnectionResetDisposable);
            reconnectDisposables.Add(reconnectionDisposable);
            reconnectDisposables.Add(outboundTrafficForward);


        }

        public IObservable<UnityMapMessage> ClientMapStream => mapForwarding.AsObservable();

        public IObservable<IMessage> InboundTraffic => inboundTraffic.AsObservable();

        public IObservable<bool> ClientConnectionState => connectionState.AsObservable();

        public IObservable<bool> ReconnectionState => reconnectinonState.AsObservable();

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
