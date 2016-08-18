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
using System.Xml;
using log4net;
using massive_moose.contracts;
using massive_moose.data;
using massive_moose.drawing;
using massive_moose.storage.azure;
using NHibernate.Criterion;

namespace massive_moose.server.api.Controllers
{
    public class ImageController : ApiController
    {
        private static ILog Log = LogManager.GetLogger(typeof(ImageController));
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
        [Route("v1/image/begin/{addressX}/{addressY}")]
        public IHttpActionResult Begin(int addressX, int addressY)
        {
            using (var session = SessionFactory.Instance.OpenSession())
            {
                if (session.CreateCriteria<DrawingSession>()
                    .Add(Restrictions.Eq("Closed", false))
                    .Add(Restrictions.Gt("Opened", DateTime.Now.Subtract(TimeSpan.FromMinutes(5))))
                    .SetProjection(Projections.Count("Id"))
                    .UniqueResult<int>() > 0)
                    return Conflict();

                var drawingSession = new DrawingSession(addressX, addressY);
                session.Save(drawingSession);
                session.Flush();
                Log.DebugFormat("Opened session {0}", drawingSession.SessionToken);
                return Ok(drawingSession.SessionToken);
            }
        }

        [HttpPost]
        [Route("v1/image/receive/{token}")]
        public async Task<IHttpActionResult> Receive(Guid token)
        {
            using (var session = SessionFactory.Instance.OpenSession())
            {
                Log.DebugFormat("Receiving data for session {0}", token);
                var drawingSession = session.CreateCriteria<DrawingSession>()
                    .Add(Restrictions.Eq("SessionToken", token))
                    .UniqueResult<DrawingSession>();

                if (drawingSession == null)
                    return NotFound();

                var inputString = await Request.Content.ReadAsStringAsync();
                massive_moose.contracts.Canvas canvas = null;
                try
                {
                    StringBuilder sb = new StringBuilder();

                    using (var textReader = new System.IO.StringReader(inputString))
                    using (var xmlReader = XmlReader.Create(textReader))
                    {
                        canvas =
                            new DataContractSerializer(typeof(massive_moose.contracts.Canvas)).ReadObject(xmlReader) as
                                massive_moose.contracts.Canvas;
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
                    string outputPath = string.Format("{0}/brick_{1}-{2}.png",
                        ConfigurationManager.AppSettings["storageContainer"],
                        drawingSession.AddressX, drawingSession.AddressY);

                    System.IO.Stream inputStream = null;
                    if (_fileStorage.Exists(outputPath))
                    {
                        inputStream = new System.IO.MemoryStream(_fileStorage.Get(outputPath));
                    }
                    var imageData = new BrickRenderer().Render(canvas, inputStream);

                    _fileStorage.Store(outputPath, imageData, true);
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
                    .UniqueResult<Brick>();

                if (brick == null)
                {
                    brick = new Brick()
                    {
                        AddressX = drawingSession.AddressX,
                        AddressY = drawingSession.AddressY
                    };
                    session.Save(brick);
                }

                session.Flush();

                return Ok();
            }
        }

        [HttpGet]
        [Route("v1/image/{addressX}/{addressY}")]
        public HttpResponseMessage ViewImage(int addressX, int addressY)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            string filePath = string.Format("{0}/brick_{1}-{2}.png",
                    ConfigurationManager.AppSettings["storageContainer"],
                    addressX, addressY);

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
