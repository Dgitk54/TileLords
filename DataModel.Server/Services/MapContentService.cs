using DataModel.Common;
using DataModel.Common.Messages;
using DataModel.Server.Model;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DataModel.Server.Services
{
    public class MapContentService
    {
        const int CONTENTSAMPLE = 3;

        public MapContentService()
        {
        }

        /// <summary>
        /// Adds content on the map with a given currentLocationStream. Content is removed on dispose of this stream.
        /// </summary>
        /// <param name="content">The content to add on the map</param>
        /// <param name="contentLocation">The stream of the location</param>
        /// <returns>Disposable to remove the content.</returns>
        public IDisposable AddMapContent(MapContent content, IObservable<PlusCode> contentLocation)
        {
            return contentLocation.Sample(TimeSpan.FromSeconds(CONTENTSAMPLE))
                                  .Finally(() => RedisDatabaseFunctions.UpsertOrDeleteContent(content, null))
                                  .Subscribe(v =>
                                  {
                                      RedisDatabaseFunctions.UpsertOrDeleteContent(content, v.Code);
                                  },() => RedisDatabaseFunctions.UpsertOrDeleteContent(content, null));
        }


        public IObservable<List<MapContent>> GetListMapUpdate(string userLocation)
        {
            return Observable.Create<List<MapContent>>(v =>
            {
                List<MapContent> result = null;
                try
                {
                    result = RedisDatabaseFunctions.RequestVisibleContent(userLocation);
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
                List<MapContent> result = null;
                try
                {
                    result = RedisDatabaseFunctions.RequestVisibleContent(userLocation);
                    
                }
                catch (Exception e)
                {
                    v.OnError(e);
                    return Disposable.Empty;
                }

                if (result != null)
                {
                    var contentList = result.ConvertAll(e => e.AsMessage());
                    var batchMessage = new BatchContentMessage() { ContentList = contentList };
                    v.OnNext(batchMessage);
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


    }
}
