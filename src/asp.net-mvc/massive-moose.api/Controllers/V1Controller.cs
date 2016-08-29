using System;
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
using massive_moose.api.Models;

namespace massive_moose.api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class V1Controller : ApiController
    {
        private readonly WallOperations _wallOperations;
        private static ILog Log = LogManager.GetLogger(typeof(V1Controller));
        private IFileStorage _fileStorage;
        private static ThumbnailCache ThumbnailCache = new ThumbnailCache();
        public V1Controller()
        {

            _fileStorage = new AzureFileStorage(new AzureFileStorageConfiguration() { ConnectionString = ConfigurationManager.ConnectionStrings["azure-storage"].ConnectionString });
            _wallOperations = new WallOperations(_fileStorage);
        }
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
                var wall = _wallOperations.GetWallByKeyOrDefault(wallKey, session);

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
        [Route("v1/save/{token}")]
        public async Task<IHttpActionResult> SaveImage(Guid token, [FromBody] BrickUpdate update)
        {
            if (!ModelState.IsValid || update == null)
            {
                return BadRequest("ModelState.IsValid:" + ModelState.IsValid + ", update:" + update);
            }
            using (var session = SessionFactory.Instance.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                try
                {
                    Log.DebugFormat("Receiving data for session {0}", token);
                    var drawingSession = session.CreateCriteria<DrawingSession>()
                        .Add(Restrictions.Eq("SessionToken", token))
                        .UniqueResult<DrawingSession>();

                    if (drawingSession == null)
                        return NotFound();


                        string outputPath = string.Format("{0}/b_{1}-{2}-{3}.png",
                            ConfigurationManager.AppSettings["storageContainer"],
                            drawingSession.Wall.InviteCode, drawingSession.AddressX, drawingSession.AddressY);

                    string imageDataBase64 = update.ImageData.Replace("data:image/png;base64,", "");
                    byte[] imageDataBytes = Convert.FromBase64String(imageDataBase64);
                    _fileStorage.Store(outputPath, imageDataBytes, true);

                    System.IO.MemoryStream myMemStream = new System.IO.MemoryStream(imageDataBytes);
                    System.Drawing.Image fullsizeImage = System.Drawing.Image.FromStream(myMemStream);
                    int thumbnailWidth = 0, thumbnailHeight = 0;
                    int.TryParse(ConfigurationManager.AppSettings["thumbnailWidth"], out thumbnailWidth);
                    int.TryParse(ConfigurationManager.AppSettings["thumbnailHeight"], out thumbnailHeight);
                    System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(thumbnailWidth, thumbnailHeight, null, IntPtr.Zero);
                    System.IO.MemoryStream myResult = new System.IO.MemoryStream();
                    newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Png);


                    outputPath = string.Format("{0}/b_{1}-{2}-{3}_1.png",
                        ConfigurationManager.AppSettings["storageContainer"],
                        drawingSession.Wall.InviteCode, drawingSession.AddressX, drawingSession.AddressY);
                    byte[] thumbnailImageData = myResult.ToArray();
                    _fileStorage.Store(outputPath, thumbnailImageData, true);
                    ThumbnailCache.Set(outputPath, thumbnailImageData);

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

                    var unencodedSnapshotJson = System.Web.HttpUtility.UrlDecode(update.SnapshotJson);

                    brick.LastUpdated = DateTime.Now;
                    brick.SnapshotJson = unencodedSnapshotJson;
                    session.SaveOrUpdate(brick);

                    tx.Commit();

                    return Ok();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    Log.Error("Failed to save image", ex);
                    return InternalServerError();
                }
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

        private void ExportImageToFile(DrawingSession drawingSession, Canvas canvas)
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

            int thumbnailWidth = 0, thumbnailHeight = 0;
            int.TryParse(ConfigurationManager.AppSettings["thumbnailWidth"], out thumbnailWidth);
            int.TryParse(ConfigurationManager.AppSettings["thumbnailHeight"], out thumbnailHeight);
            System.IO.MemoryStream myMemStream = new System.IO.MemoryStream(imageData);
            System.Drawing.Image fullsizeImage = System.Drawing.Image.FromStream(myMemStream);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(thumbnailWidth, thumbnailHeight, null, IntPtr.Zero);
            System.IO.MemoryStream myResult = new System.IO.MemoryStream();
            newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Gif);

            outputPath = string.Format("{0}/b_{1}-{2}-{3}_1.png",
                ConfigurationManager.AppSettings["storageContainer"],
                drawingSession.Wall.InviteCode, drawingSession.AddressX, drawingSession.AddressY);
            var thumbnailImageData = myResult.ToArray();
            _fileStorage.Store(outputPath, thumbnailImageData, true);
            ThumbnailCache.Set(outputPath, thumbnailImageData);
        }

        [HttpGet]
        [Route("v1/image/{wallKey}/{addressX}/{addressY}")]
        public HttpResponseMessage ViewImage(string wallKey, int addressX, int addressY)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            Wall wall = null;
            using (var session = SessionFactory.Instance.OpenStatelessSession())
            {
                wall = _wallOperations.GetWallByKeyOrDefault(wallKey, session);
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

            if (string.IsNullOrWhiteSpace(wallKey))
            {
                using (var session = SessionFactory.Instance.OpenStatelessSession())
                {
                    wall = _wallOperations.GetWallByKeyOrDefault(wallKey, session);
                    if (wall == null)
                    {
                        result.StatusCode = HttpStatusCode.NotFound;
                        return result;
                    }

                    wallKey = wall.InviteCode;
                }
            }
            string filePath = string.Format("{0}/b_{1}-{2}-{3}_1.png",
                    ConfigurationManager.AppSettings["storageContainer"],
                    wallKey, addressX, addressY);

            var fromCache = ThumbnailCache.Get(filePath);
            if (fromCache != null)
            {
                result.Content = new ByteArrayContent(fromCache);
                result.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                return result;
            }

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
        [Route("v1/background/{filename}")]
        public HttpResponseMessage GetBackgroundImage(string filename)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            var filePath = string.Concat(ConfigurationManager.AppSettings["backgroundsContainer"],
                "/",
                filename);
            if (_fileStorage.Exists(filePath))
            {
                var data = _fileStorage.Get(filePath);
                result.Content = new ByteArrayContent(data);
                result.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpg");
                return result;
            }
            Log.WarnFormat("can't find background image {0}", filePath);
            result.StatusCode = HttpStatusCode.NotFound;
            return result;
        }
    }
}
