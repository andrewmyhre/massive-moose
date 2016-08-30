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
    }
}
