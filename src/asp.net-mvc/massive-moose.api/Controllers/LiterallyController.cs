using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using log4net;
using massive_moose.storage.azure;
using Newtonsoft.Json;
using NHibernate.Criterion;
using massive_moose.services.models.drawing;
using massive_moose.services.models;
using massive_moose.services;
using massive_moose.services.models.literally;
using System.Net.Http;

namespace massive_moose.api.Controllers
{
    
    public class LiterallyController : ApiController
    {
        private readonly WallOperations _wallOperations;
        private readonly IFileStorage _fileStorage;
        private static ILog Log = LogManager.GetLogger(typeof(LiterallyController));

        public LiterallyController()
        {
            _fileStorage = new AzureFileStorage(new AzureFileStorageConfiguration() { ConnectionString = ConfigurationManager.ConnectionStrings["azure-storage"].ConnectionString });
            _wallOperations = new WallOperations(_fileStorage);
        }

        [HttpPost]
        [Route("literally/draw/{token}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
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
                try
                {
                    var drawing = JsonConvert.DeserializeObject<Drawing>(inputString);
                    var canvas = new LiterallyMapper().ToCanvas(drawing);
                    _wallOperations.Contribute(inputString, canvas, drawingSession, session, Request.GetOwinContext().Request.RemoteIpAddress);
                }
                catch (Exception ex)
                {
                    Log.Error("failure", ex);
                    return InternalServerError();
                }

                drawingSession.Closed = true;

                Log.DebugFormat("Updated brick ({0},{1}) for session {2}", drawingSession.AddressX,
                    drawingSession.AddressY, drawingSession.SessionToken);

                try
                {
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
                    brick.SnapshotJson = inputString;
                    session.Save(brick);

                    session.Flush();
                }
                catch (Exception ex)
                {
                    Log.Error("Failed saving brick", ex);
                    return InternalServerError();
                }
            }

            return Ok();
        }

        private void ExportCanvasToImage(Canvas canvas, DrawingSession drawingSession)
        {
            var imageData = new BrickRenderer().Render(canvas);

            string outputPath = string.Format("{0}/b_{1}-{2}-{3}.png",
                ConfigurationManager.AppSettings["storageContainer"],
                drawingSession.Wall.InviteCode, drawingSession.AddressX, drawingSession.AddressY);

            _fileStorage.Store(outputPath, imageData, true);

            
        }
    }
}
