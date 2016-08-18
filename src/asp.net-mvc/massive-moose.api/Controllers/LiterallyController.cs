using log4net;
using massive_moose.drawing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using massive_moose.storage.azure;
using Newtonsoft.Json;
using massive_moose.data;
using NHibernate.Criterion;
using massive_moose.contracts;

namespace massive_moose.server.api.Controllers
{
    
    public class LiterallyController : ApiController
    {
        private static ILog Log = LogManager.GetLogger(typeof(LiterallyController));
        private static AzureFileStorage _fileStorage = new AzureFileStorage(ConfigurationManager.ConnectionStrings["azure-storage"].ConnectionString);
        [HttpPost]
        [Route("literally/receive/{addressX}/{addressY}")]
        public async Task<IHttpActionResult> Receive(int addressX, int addressY)
        {
            using (var session = SessionFactory.Instance.OpenSession())
            {
                Log.DebugFormat("Receiving literallycanvas data for {0},{1}", addressX, addressY);
                string outputPath = string.Format("{0}/brick_{1}-{2}.png",
                    ConfigurationManager.AppSettings["storageContainer"],
                    addressX, addressY);

                try
                {
                    var inputString = await Request.Content.ReadAsStringAsync();
                    var drawing = JsonConvert.DeserializeObject<massive_moose.contracts.literally.Drawing>(inputString);
                    var canvas = new LiterallyMapper().ToCanvas(drawing);
                    var imageData = new BrickRenderer().Render(canvas, null);
                    _fileStorage.Store(outputPath, imageData, true);
                }
                catch (Exception ex)
                {
                    Log.Error("failure", ex);
                    return InternalServerError();
                }

                var brick = session.CreateCriteria<Brick>()
                    .Add(Restrictions.Eq("AddressX", addressX))
                    .Add(Restrictions.Eq("AddressY", addressY))
                    .UniqueResult<Brick>();

                if (brick == null)
                {
                    brick = new Brick()
                    {
                        AddressX = addressX,
                        AddressY = addressY
                    };
                    session.Save(brick);
                }

                session.Flush();
            }

            return Ok();
        }
    }
}
