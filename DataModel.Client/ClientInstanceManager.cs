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
        IDisposable reconnectionDisposable;
        IDisposable reconnectionResetDisposable;
        ClientInstance instance;
        Task runningClient;
        public ClientInstanceManager(string ipAdress = "127.0.0.1", int port = 8080)
        {

            CreateNewInstanceAndSubscribe();

            reconnectionDisposable = connectionState.Where(v => !v)
                                                    .WithLatestFrom(GetWorkingAccountDetails(inboundTraffic, outboundTraffic), (state, account) => new { state, account })
                                                    .Do(v => reconnectinonState.OnNext(true))
                                                    .Do(v => CreateNewInstanceAndSubscribe())
                                                    .Delay(TimeSpan.FromSeconds(SENDLOGINMESSAGEDELAY))
                                                    .Subscribe(v => outboundTraffic.OnNext(v.account));

            reconnectionResetDisposable = connectionState.Where(v => v).Subscribe(v => reconnectinonState.OnNext(false));
        }

        public IObservable<UnityMapMessage> ClientMapStream => mapForwarding.AsObservable();

        public IObservable<IMessage> OutboundTraffic => outboundTraffic.AsObservable();

        public IObservable<IMessage> InboundTraffic => inboundTraffic.AsObservable();

        public IObservable<bool> ClientConnectionState => connectionState.AsObservable();

        public IObservable<bool> ReconnectionState => reconnectinonState.AsObservable();

        public void ShutDown()
        {
            reconnectionDisposable.Dispose();
            reconnectionResetDisposable.Dispose();
            StopRunningClientInstance();
        }
        void CreateNewInstanceAndSubscribe()
        {
            StopRunningClientInstance();

            //Create new Instance
            instance = new ClientInstance();
            var disp1 = instance.OutboundTraffic.Subscribe(v => outboundTraffic.OnNext(v));
            var disp2 = instance.InboundTraffic.Subscribe(v => inboundTraffic.OnNext(v));
            var disp3 = instance.ClientConnectionState.Subscribe(v => connectionState.OnNext(v));
            var disp4 = instance.ClientMapStream.Subscribe(v => mapForwarding.OnNext(v));

            

            forwardDisposables.Add(disp1);
            forwardDisposables.Add(disp2);
            forwardDisposables.Add(disp3);
            forwardDisposables.Add(disp4);

            runningClient = Task.Run(() => instance.RunClientAsyncWithIP());

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
