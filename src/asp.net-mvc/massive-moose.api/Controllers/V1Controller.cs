﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml;
using log4net;
using massive_moose.storage.azure;
using NHibernate.Criterion;
using massive_moose.services.models;
using massive_moose.services;
using massive_moose.services.models.drawing;
using NHibernate;

namespace massive_moose.api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class V1Controller : ApiController
    {
        private static ILog Log = LogManager.GetLogger(typeof(V1Controller));
        private static AzureFileStorage _fileStorage = new AzureFileStorage(ConfigurationManager.ConnectionStrings["azure-storage"].ConnectionString);

        [HttpGet]
        [Route("v1/image/sessions")]
        public IEnumerable<DrawingSession> ActiveSessions()
        {
            using (var session = SessionFactory.Instance.OpenSession())
            {
                return session.CreateCriteria<DrawingSession>()
                    .Add(Restrictions.Eq("Closed", false))
                    .Add(Restrictions.Gt("Opened", DateTime.Now.Subtract(TimeSpan.FromMinutes(5))))
                    .List<DrawingSession>();
            }
        }

        [HttpGet]
        [Route("v1/wall")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IEnumerable<Brick> Wall()
        {
            using (var session = SessionFactory.Instance.OpenSession())
            {
                var wall = session.CreateCriteria<Brick>()
                    .List<Brick>();
                return wall;
            }
        }

        [HttpPost]
        [Route("v1/release/{token}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult Release(Guid token)
        {
            using (var session = SessionFactory.Instance.OpenSession())
            {
                Log.DebugFormat("Receiving data for session {0}", token);
                var drawingSession = session.CreateCriteria<DrawingSession>()
                    .Add(Restrictions.Eq("SessionToken", token))
                    .UniqueResult<DrawingSession>();

                if (drawingSession == null)
                    return NotFound();

                drawingSession.Closed = true;
                session.Flush();

                return Ok();
            }
        }

        [HttpPost]
        [Route("v1/{wallKey}/draw/{addressX}/{addressY}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult Begin(int addressX, int addressY, string wallKey=null)
        {
            using (var session = SessionFactory.Instance.OpenSession())
            {
                var wall = new WallOperations().GetWallByKeyOrDefault(wallKey, session);

                if (wall == null)
                    return NotFound();

                if (session.CreateCriteria<DrawingSession>()
                    .Add(Restrictions.Eq("Closed", false))
                    .Add(Restrictions.Gt("Opened", DateTime.Now.Subtract(TimeSpan.FromMinutes(5))))
                    .Add(Restrictions.Eq("AddressX", addressX))
                    .Add(Restrictions.Eq("AddressY",addressY))
                    .Add(Restrictions.Eq("Wall.Id", wall.Id))
                    .SetProjection(Projections.Count("Id"))
                    .UniqueResult<int>() > 0)
                    return Conflict();

                var drawingSession = new DrawingSession(wall, addressX, addressY);
                session.Save(drawingSession);
                session.Flush();

                Log.DebugFormat("Opened session {0}", drawingSession.SessionToken);
                var brick = session.CreateCriteria<Brick>()
                    .Add(Restrictions.Eq("AddressX", addressX))
                    .Add(Restrictions.Eq("AddressY", addressY))
                    .Add(Restrictions.Eq("Wall.Id", wall.Id))
                    .UniqueResult<Brick>();

                return Ok(new
                {
                    sessionToken= drawingSession.SessionToken,
                    snapshotJson=brick != null ? brick.SnapshotJson : null
                });
            }
        }

        [HttpPost]
        [Route("v1/draw/{token}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<IHttpActionResult> Receive(Guid token)
        {
            using (var session = SessionFactory.Instance.OpenSession())
                using (var tx = session.BeginTransaction())
            {

                Log.DebugFormat("Receiving data for session {0}", token);
                var drawingSession = session.CreateCriteria<DrawingSession>()
                    .Add(Restrictions.Eq("SessionToken", token))
                    .UniqueResult<DrawingSession>();

                if (drawingSession == null)
                    return NotFound();

                var inputString = await Request.Content.ReadAsStringAsync();
                Canvas canvas = null;
                try
                {
                    StringBuilder sb = new StringBuilder();

                    using (var textReader = new System.IO.StringReader(inputString))
                    using (var xmlReader = XmlReader.Create(textReader))
                    {
                        canvas =
                            new DataContractSerializer(typeof(Canvas)).ReadObject(xmlReader) as
                                Canvas;
                        if (canvas == null)
                        {
                            throw new Exception("Deserializing data produced a null object");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to deserialise canvas", ex);
                    throw;
                }

                try
                {
                    ExportImageToFile(drawingSession, canvas);
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to render canvas", ex);
                    throw;
                }

                drawingSession.Closed = true;

                Log.DebugFormat("Updated brick ({0},{1}) for session {2}", drawingSession.AddressX,
                    drawingSession.AddressY, drawingSession.SessionToken);

                var brick = session.CreateCriteria<Brick>()
                    .Add(Restrictions.Eq("AddressX", drawingSession.AddressX))
                    .Add(Restrictions.Eq("AddressY", drawingSession.AddressY))
                    .Add(Restrictions.Eq("Wall.Id", drawingSession.Wall.Id))
                    .UniqueResult<Brick>();

                if (brick == null)
                {
                    brick = new Brick()
                    {
                        AddressX = drawingSession.AddressX,
                        AddressY = drawingSession.AddressY,
                        Wall = drawingSession.Wall
                    };
                    
                }
                brick.LastUpdated = DateTime.Now;
                session.SaveOrUpdate(brick);

                tx.Commit();

                return Ok();
            }
        }

        private static void ExportImageToFile(DrawingSession drawingSession, Canvas canvas)
        {
            string outputPath = string.Format("{0}/b_{1}-{2}-{3}.png",
                ConfigurationManager.AppSettings["storageContainer"],
                        drawingSession.Wall.InviteCode, drawingSession.AddressX, drawingSession.AddressY);

            System.IO.Stream inputStream = null;
            if (_fileStorage.Exists(outputPath))
            {
                inputStream = new System.IO.MemoryStream(_fileStorage.Get(outputPath));
            }
            var imageData = new BrickRenderer().Render(canvas, inputStream);

            _fileStorage.Store(outputPath, imageData, true);

            System.IO.MemoryStream myMemStream = new System.IO.MemoryStream(imageData);
            System.Drawing.Image fullsizeImage = System.Drawing.Image.FromStream(myMemStream);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(200, 100, null, IntPtr.Zero);
            System.IO.MemoryStream myResult = new System.IO.MemoryStream();
            newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Gif);

            outputPath = string.Format("{0}/b_{1}-{2}-{3}_1.png",
                ConfigurationManager.AppSettings["storageContainer"],
                drawingSession.Wall.InviteCode, drawingSession.AddressX, drawingSession.AddressY);
            _fileStorage.Store(outputPath, myResult.ToArray(), true);
        }

        [HttpGet]
        [Route("v1/image/{wallKey}/{addressX}/{addressY}")]
        public HttpResponseMessage ViewImage(string wallKey, int addressX, int addressY)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            Wall wall = null;
            using (var session = SessionFactory.Instance.OpenStatelessSession())
            {
                wall = new WallOperations().GetWallByKeyOrDefault(wallKey, session);
            }

            string filename = string.Format("b_{0}-{1}-{2}", wall.InviteCode, addressX, addressY);
            string filePath = string.Format("{0}/{1}.png",
                    ConfigurationManager.AppSettings["storageContainer"],
                    filename);

            if (!_fileStorage.Exists(filePath))
            {
                result.StatusCode = HttpStatusCode.NotFound;
                return result;
            }

            var data = _fileStorage.Get(filePath);
            result.Content = new ByteArrayContent(data);
            result.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
            return result;
        }

        

        [HttpGet]
        [Route("v1/image/t/{wallKey}/{addressX}/{addressY}")]
        public HttpResponseMessage ViewThumbnail(int addressX, int addressY, string wallKey=null)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            Wall wall = null;
            using (var session = SessionFactory.Instance.OpenStatelessSession())
            {
                wall = new WallOperations().GetWallByKeyOrDefault(wallKey, session);
            }
            string filePath = string.Format("{0}/b_{1}-{2}-{3}_1.png",
                    ConfigurationManager.AppSettings["storageContainer"],
                    wall.InviteCode, addressX, addressY);

            if (!_fileStorage.Exists(filePath))
            {
                result.StatusCode = HttpStatusCode.NotFound;
                return result;
            }

            var data = _fileStorage.Get(filePath);
            result.Content = new ByteArrayContent(data);
            result.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
            return result;
        }
    }
}
