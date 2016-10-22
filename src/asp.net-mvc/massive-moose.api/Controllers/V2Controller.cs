using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using log4net;
using massive_moose.services.models;
using massive_moose.services;
using System.Net.Http.Headers;
using massive_moose.services.viewmodels;
using NHibernate;
using System.Text;
using System.Web.Mvc;
using System.Configuration;
using System.Drawing.Imaging;

namespace massive_moose.api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class V2Controller : ApiController
    {
        private readonly IWallOperations _wallOperations;
        private readonly ILog _log;
        private readonly ISessionFactory _sessionFactory;

        public V2Controller(IWallOperations wallOperations, ILog log, ISessionFactory sessionFactory)
        {
            _wallOperations = wallOperations;
            _log = log;
            _sessionFactory = sessionFactory;
        }

        [System.Web.Http.HttpGet] 
        [System.Web.Http.Route("v2/wall/{wallKey}/{originX}/{originY}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public BrickViewModel[,] Wall(int originX, int originY, string wallKey = null)
        {
            using (var session = _sessionFactory.OpenStatelessSession())
            {
                return _wallOperations.GetWallSnapshot(originX, originY, wallKey, session);
            }
        }

        [System.Web.Http.AcceptVerbs("HEAD")]
        [System.Web.Http.Route("v2/wall/{wallKey}/{originX}/{originY}/{etag?}")]
        [EnableCors(origins: "*", headers: "*", methods: "*", exposedHeaders:"ETag")]
        public HttpResponseMessage WallETag(int originX, int originY, string etag=null, string wallKey = null)
        {
            var result = new HttpResponseMessage();
            if (string.IsNullOrWhiteSpace(etag))
                return result; // always return 200 (has content)

            try
            {
                using (var session = _sessionFactory.OpenStatelessSession())
                {
                    Wall wallRecord = _wallOperations.GetWallByKeyOrDefault(wallKey, session);
                    if (wallRecord == null)
                    {
                        result.StatusCode = HttpStatusCode.NotFound;
                    }
                    else
                    {
                        var drawingSessions = _wallOperations.GetActiveDrawingSessions(wallRecord.Id, session);
                        var sb = new StringBuilder();
                        var bytes = drawingSessions.Select(ds=>ds.Id).ToArray();
                        foreach (var b in bytes)
                        {
                            sb.Append(string.Format("{0:X}", b));
                        }

                        var actualETag = wallRecord.ETag+ sb.ToString();
                        result.Headers.ETag = new EntityTagHeaderValue("\""+actualETag+"\"");
                        if (etag == actualETag)
                            result.StatusCode = HttpStatusCode.NotModified;
                        return result;
                    }
                    return result;
                }
            } catch (Exception ex) {
                _log.Error("error generating etag", ex);
                throw;
            }
        }

        [System.Web.Http.Route("v2/wall/history/image/{historyItemId}")]
        public HttpResponseMessage GetHistoricImage(int historyItemId)
        {
            using (var session = _sessionFactory.OpenSession())
            {
                var historyItem = session.Get<WallHistoryItem>(historyItemId);
                HttpResponseMessage result = new HttpResponseMessage();
                result.Content = new ByteArrayContent(historyItem.SnapshotImage);
                result.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                return result;
            }
        }

        [System.Web.Http.Route("v2/wall/history/image/t/{historyItemId}")]
        public HttpResponseMessage GetHistoricThumbnailImage(int historyItemId)
        {
            using (var session = _sessionFactory.OpenSession())
            {
                var historyItem = session.Get<WallHistoryItem>(historyItemId);
                HttpResponseMessage result = new HttpResponseMessage();
                result.Content = new ByteArrayContent(historyItem.SnapshotImageThumbnail);
                result.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                return result;
            }
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("v2/wall/{wallKey}/img")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage WallImage(string wallKey = null)
        {
            HttpResponseMessage result = new HttpResponseMessage();

            var image = _wallOperations.GetFullWallImage(wallKey);
            if (image == null)
            {
                result.StatusCode = HttpStatusCode.NotFound;
            }
            else
            {
                result.Content = new ByteArrayContent(image);
                result.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
            }
            return result;
        }
        [System.Web.Http.HttpHead]
        [System.Web.Http.Route("v2/wall/{wallKey}/img/{etag?}")]
        [EnableCors(origins: "*", headers: "*", methods: "*", exposedHeaders: "ETag")]
        public HttpResponseMessage WallImageSize(string etag = null, string wallKey = null)
        {
            var result = new HttpResponseMessage();

            if (string.IsNullOrWhiteSpace(etag))
                return result;

            var image = _wallOperations.GetFullWallImage(wallKey);
            if (image == null)
            {
                result.StatusCode = HttpStatusCode.NotFound;
            }
            else
            {
                result.Headers.ETag = new EntityTagHeaderValue("\"" + image.LongLength + "\""); ;
                var sentEtag = long.Parse(etag);
                if (sentEtag == image.LongLength)
                {
                    result.StatusCode = HttpStatusCode.NotModified;
                }
            }
            return result;
        }

    }
}