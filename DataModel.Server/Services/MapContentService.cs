using DataModel.Common;
using DataModel.Common.Messages;
using DataModel.Server.Model;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace DataModel.Server.Services
{
    public class MapContentService
    {
        const int CONTENTTHROTTLEINSECONDS = 3;
        readonly Func<string, BatchContentMessage> areaLookup;
        readonly Func<string, List<MapContent>> areaLookupAsContent;
        readonly Action<MapContent, string> userContentStorage;
        
        public  MapContentService(Func<string, BatchContentMessage> areaLookup, Action<MapContent, string> userContentStorage, Func<string, List<MapContent>> areaLookupAsContent)
        {
            this.areaLookup = areaLookup;
            this.userContentStorage = userContentStorage;
            this.areaLookupAsContent = areaLookupAsContent;
        }

        /// <summary>
        /// Adds content on the map with a given currentLocationStream. Content is removed on dispose of this stream.
        /// </summary>
        /// <param name="content">The content to add on the map</param>
        /// <param name="contentLocation">The stream of the location</param>
        /// <returns>Disposable to remove the content.</returns>
        public IDisposable AddMapContent(MapContent content, IObservable<PlusCode> contentLocation)
        {
            return contentLocation.Throttle(TimeSpan.FromSeconds(CONTENTTHROTTLEINSECONDS))
                                  .Finally(()=> userContentStorage(content, null))
                                  .Subscribe(v => 
                                  {
                                      userContentStorage(content, v.Code);
                                  }, 
                                      () => { userContentStorage(content, null);
                                  });
        }

        
        public IObservable<List<MapContent>> GetListMapUpdate(string userLocation)
        {
            return Observable.Create<List<MapContent>>(v =>
            {
                
                List<MapContent> result = null;
                try
                {
                    result = areaLookupAsContent.Invoke(userLocation);
                }
                catch (Exception e)
                {
                    v.OnError(e);
                    return Disposable.Empty;
                }
                ;
                if (result != null)
                {
                    v.OnNext(result);
                    v.OnCompleted();
                    return Disposable.Empty;
                }
                else
                {
                    v.OnCompleted();
                    return Disposable.Empty;
                }
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
