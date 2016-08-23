﻿using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using log4net;
using massive_moose.contracts;
using massive_moose.contracts.drawing;
using massive_moose.data;
using massive_moose.drawing;
using massive_moose.storage.azure;
using Newtonsoft.Json;
using NHibernate.Criterion;

namespace massive_moose.api.Controllers
{
    
    public class LiterallyController : ApiController
    {
        private static ILog Log = LogManager.GetLogger(typeof(LiterallyController));
        private static AzureFileStorage _fileStorage = new AzureFileStorage(ConfigurationManager.ConnectionStrings["azure-storage"].ConnectionString);

        [HttpPost]
        [Route("literally/receive/{token}")]
        [EnableCors(origins: "http://local.massivemoose.com", headers: "*", methods: "*")]
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
                    var drawing = JsonConvert.DeserializeObject<massive_moose.contracts.literally.Drawing>(inputString);
                    var canvas = new LiterallyMapper().ToCanvas(drawing);
                    ExportCanvasToImage(canvas, drawingSession);
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
                        .UniqueResult<Brick>();

                    if (brick == null)
                    {
                        brick = new Brick()
                        {
                            AddressX = drawingSession.AddressX,
                            AddressY = drawingSession.AddressY
                        };
                    }
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

        private static void ExportCanvasToImage(Canvas canvas, DrawingSession drawingSession)
        {
            var imageData = new BrickRenderer().Render(canvas);

            string outputPath = string.Format("{0}/brick_{1}-{2}.png",
                ConfigurationManager.AppSettings["storageContainer"],
                drawingSession.AddressX, drawingSession.AddressY);

            _fileStorage.Store(outputPath, imageData, true);

            System.IO.MemoryStream myMemStream = new System.IO.MemoryStream(imageData);
            System.Drawing.Image fullsizeImage = System.Drawing.Image.FromStream(myMemStream);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(200, 100, null, IntPtr.Zero);
            System.IO.MemoryStream myResult = new System.IO.MemoryStream();
            newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Png);

            outputPath = string.Format("{0}/brick_{1}-{2}_1.png",
                ConfigurationManager.AppSettings["storageContainer"],
                drawingSession.AddressX, drawingSession.AddressY);
            _fileStorage.Store(outputPath, myResult.ToArray(), true);
        }
    }
}
