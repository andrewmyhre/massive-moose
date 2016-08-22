using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using log4net;
using massive_moose.data;

namespace massive_moose.api.Controllers
{
    public class ConfigController : ApiController
    {
        private static ILog Log = LogManager.GetLogger(typeof(ConfigController));
        [HttpGet]
        [Route("config/create-database")]
        public IHttpActionResult CreateDatabase()
        {
            try
            {
                SessionFactory.RebuildDatabase();
                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to create database", ex);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("log")]
        public object GetLog()
        {
            var logFilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/app_data/log.txt");
            if (System.IO.File.Exists(logFilePath))
            {
                using (var file = System.IO.File.Open(logFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.Read))
                using (System.IO.StreamReader reader = new StreamReader(file))
                {
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(reader.ReadToEnd(), Encoding.UTF8, "text/plain")
                    };
                }
            }
            return NotFound();
        }
    }
}
