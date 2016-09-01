using System.Web.Http;
using log4net;
using massive_moose.services;

namespace massive_moose.api.Controllers
{

    public class LiterallyController : ApiController
    {
        private readonly IWallOperations _wallOperations;
        private readonly IFileStorage _fileStorage;
        private readonly ILog _log;

        public LiterallyController(IWallOperations wallOperations, IFileStorage fileStorage, ILog log)
        {
            _fileStorage = fileStorage;
            _log = log;
            _wallOperations = wallOperations;
        }
    }
}
