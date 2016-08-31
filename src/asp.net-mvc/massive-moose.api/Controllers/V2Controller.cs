using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using log4net;
using massive_moose.services.models;
using massive_moose.services;
using NHibernate.Criterion;
using massive_moose.api.Models;
using massive_moose.storage.azure;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using massive_moose.services.viewmodels;
using Newtonsoft.Json;
using System.Text;

namespace massive_moose.api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class V2Controller : ApiController
    {
        private readonly WallOperations _wallOperations;

        public V2Controller()
        {
            _wallOperations = new WallOperations(new AzureFileStorage(new AzureFileStorageConfiguration() {ConnectionString = ConfigurationManager.ConnectionStrings["azure-storage"].ConnectionString}));
        }
        private static ILog Log = LogManager.GetLogger(typeof(V2Controller));

        [HttpGet]
        [Route("v2/wall/{wallKey}/{originX}/{originY}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public BrickViewModel[,] Wall(int originX, int originY, string wallKey = null)
        {
            using (var session = SessionFactory.Instance.OpenStatelessSession())
            {
                return _wallOperations.GetBricksForWall(originX, originY, wallKey, session);
            }
        }

        [AcceptVerbs("HEAD")]
        [Route("v2/wall/{wallKey}/{originX}/{originY}/{etag}")]
        [EnableCors(origins: "*", headers: "*", methods: "*", exposedHeaders:"ETag")]
        public HttpResponseMessage WallETag(int originX, int originY, string etag=null, string wallKey = null)
        {
            var result = new HttpResponseMessage();
            try {
                using (var session = SessionFactory.Instance.OpenStatelessSession())
                {
                    Wall wallRecord = _wallOperations.GetWallByKeyOrDefault(wallKey, session);
                    if (wallRecord == null)
                    {
                        result.StatusCode = HttpStatusCode.NotFound;
                    }
                    else
                    {
                        var actualETag = wallRecord.ETag;
                        result.Headers.ETag = new EntityTagHeaderValue("\""+actualETag+"\"");
                        if (etag == actualETag)
                            result.StatusCode = HttpStatusCode.NotModified;
                        return result;
                    }
                    return result;
                }
            } catch (Exception ex) {
                Log.Error("error generating etag", ex);
                throw;
            }
        }

        [Route("v2/wall/history/image/{historyItemId}")]
        public HttpResponseMessage GetHistoricImage(int historyItemId)
        {
            using (var session = SessionFactory.Instance.OpenSession())
            {
                var historyItem = session.Get<WallHistoryItem>(historyItemId);
                HttpResponseMessage result = new HttpResponseMessage();
                result.Content = new ByteArrayContent(historyItem.SnapshotImage);
                result.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                return result;
            }
        }

        [Route("v2/wall/history/image/t/{historyItemId}")]
        public HttpResponseMessage GetHistoricThumbnailImage(int historyItemId)
        {
            using (var session = SessionFactory.Instance.OpenSession())
            {
                var historyItem = session.Get<WallHistoryItem>(historyItemId);
                HttpResponseMessage result = new HttpResponseMessage();
                result.Content = new ByteArrayContent(historyItem.SnapshotImageThumbnail);
                result.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                return result;
            }
        }
    }
}