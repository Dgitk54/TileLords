using DataModel.Common;
using DataModel.Common.Messages;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace DataModel.Server.Services
{
    public class MapContentService
    {

        readonly Func<string, BatchContentMessage> areaLookup;
        readonly Action<MapContent, string> userContentStorage;
        public  MapContentService(Func<string, BatchContentMessage> areaLookup, Action<MapContent, string> userContentStorage)
        {
            this.areaLookup = areaLookup;
            this.userContentStorage = userContentStorage;
        }
        
        public IDisposable AddMapContent(MapContent content, IObservable<PlusCode> userLocation)
        {
            return userLocation.Finally(()=> userContentStorage(content, null))
                               .Subscribe(v => 
                               {
                                   userContentStorage(content, v.Code);
                               }, 
                                   () => { userContentStorage(content, null);
                               });
        }

        public IObservable<BatchContentMessage> GetMapUpdate(string userLocation)
        {
            return Observable.Create<BatchContentMessage>(v =>
            {
                BatchContentMessage result = null;
                try
                {
                    result = areaLookup.Invoke(userLocation);
                } catch (Exception e)
                {
                    v.OnError(e);
                    return Disposable.Empty;
                }

                if(result != null)
                {
                    v.OnNext(result);
                    v.OnCompleted();
                    return Disposable.Empty;
                } else
                {
                    v.OnCompleted();
                    return Disposable.Empty;
                }
            });
        }

        
    }
}
