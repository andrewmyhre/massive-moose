using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Utils;
using massive_moose.services.models;
using NHibernate;
using NHibernate.Criterion;
using massive_moose.services.models.drawing;
using massive_moose.services.viewmodels;
using massive_moose.services.caching;
using massive_moose.caching;
using System.Drawing.Imaging;
using System.Drawing;
using log4net;

namespace massive_moose.services
{
    public class WallOperations : IWallOperations
    {
        private readonly IFileStorage _fileStorage;
        private readonly ISessionFactory _sessionFactory;
        private readonly IObjectCache<Wall> _wallCache;
        private readonly IObjectCache<BrickWallSet> _wallAtLocationCache;
        private readonly ILog _log;

        public WallOperations(IFileStorage fileStorage, 
            ISessionFactory sessionFactory,
            IObjectCache<Wall> wallCache,
            IObjectCache<BrickWallSet> wallAtLocationCache,
            ILog log)
        {
            _fileStorage = fileStorage;
            _sessionFactory = sessionFactory;
            _wallCache = wallCache;
            _wallAtLocationCache = wallAtLocationCache;
            _log = log;
        }
        public Wall GetWallByKeyOrDefault(string wallKey, IStatelessSession session)
        {
            
            if (!string.IsNullOrWhiteSpace(wallKey))
            {
                var wall = session.CreateCriteria<Wall>()
                    .Add(Restrictions.Eq("InviteCode", wallKey))
                    .UniqueResult<Wall>();

                return wall;
            }
            else
            {
                return session.Get<Wall>(1);
            }
        }
        public Wall GetWallByKeyOrDefault(string wallKey, ISession session)
        {
            if (!string.IsNullOrWhiteSpace(wallKey))
            {
                return session.CreateCriteria<Wall>()
                    .Add(Restrictions.Eq("InviteCode", wallKey))
                    .UniqueResult<Wall>();
            }
            else
            {
                return session.Get<Wall>(1);
            }
        }

        public void Contribute(string inputJson, Guid sessionToken,
            byte[] imageData, string snapshotJson, ISessionFactory sessionFactory, string clientIp)
        {
            string wallInviteCode = null;
            using (var session = _sessionFactory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                try
                {
                    var drawingSession = session.CreateCriteria<DrawingSession>()
                        .Add(Restrictions.Eq("SessionToken", sessionToken))
                        .UniqueResult<DrawingSession>();
                    wallInviteCode = drawingSession.Wall.InviteCode;

                    string fullSizeImagePath = string.Format("{0}/b_{1}-{2}-{3}.png",
                            ConfigurationManager.AppSettings["storageContainer"],
                            drawingSession.Wall.InviteCode, drawingSession.AddressX, drawingSession.AddressY);
                    string thumbnailImagePath = string.Format("{0}/b_{1}-{2}-{3}_1.png",
                        ConfigurationManager.AppSettings["storageContainer"],
                        drawingSession.Wall.InviteCode, drawingSession.AddressX, drawingSession.AddressY);

                    _fileStorage.Store(fullSizeImagePath, imageData, true);
                    System.IO.MemoryStream myMemStream = new System.IO.MemoryStream(imageData);
                    System.Drawing.Image fullsizeImage = System.Drawing.Image.FromStream(myMemStream);
                    int thumbnailWidth = 0, thumbnailHeight = 0;
                    int.TryParse(ConfigurationManager.AppSettings["thumbnailWidth"], out thumbnailWidth);
                    int.TryParse(ConfigurationManager.AppSettings["thumbnailHeight"], out thumbnailHeight);
                    System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(thumbnailWidth, thumbnailHeight,
                        null, IntPtr.Zero);
                    System.IO.MemoryStream myResult = new System.IO.MemoryStream();
                    newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Png);

                    using (
                        var fout =
                            System.IO.File.Open(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                "app_data","image.png"), System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
                    {
                        fout.Write(imageData, 0, imageData.Length);
                        fout.Flush();
                    }

                        drawingSession.Closed = true;

                    byte[] thumbnailImageData = myResult.ToArray();
                    _fileStorage.Store(thumbnailImagePath, thumbnailImageData, true);

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

                    var unencodedSnapshotJson = System.Web.HttpUtility.UrlDecode(snapshotJson);

                    brick.LastUpdated = DateTime.Now.ToUniversalTime();
                    brick.SnapshotJson = unencodedSnapshotJson;
                    var sb = new StringBuilder();
                    var bytes = Guid.NewGuid().ToByteArray();
                    foreach (var b in bytes)
                    {
                        sb.Append(string.Format("{0:X}", b));
                    }
                    brick.Wall.ETag = sb.ToString();

                    session.SaveOrUpdate(brick);

                    // store a history entry for this contribution
                    var wallHistoryItem = new WallHistoryItem();
                    wallHistoryItem.Wall = drawingSession.Wall;
                    wallHistoryItem.SnapshotImage = imageData;
                    wallHistoryItem.SnapshotImageThumbnail = thumbnailImageData;
                    wallHistoryItem.SnapshotJson = inputJson;
                    wallHistoryItem.Timestamp = DateTime.Now.ToUniversalTime();
                    wallHistoryItem.ClientIp = clientIp;
                    wallHistoryItem.DrawingSession = drawingSession;

                    GetLatestWallSnapshotAndUpdateCache(0, 0, drawingSession.Wall.InviteCode, session);


                    session.Save(wallHistoryItem);
                    tx.Commit();


                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw;
                }
            }

            var wallImageFilename = string.Format("{0}/w_{1}.png",
                ConfigurationManager.AppSettings["storageContainer"],
                wallInviteCode);
            Task.Factory.StartNew(() => UpdateFullWallImage(wallInviteCode, wallImageFilename));
        }

        public IList<DrawingSession> GetActiveDrawingSessions(int wallId, IStatelessSession session)
        {
            return session.CreateCriteria<DrawingSession>()
                .Add(Restrictions.Eq("Wall.Id", wallId))
                .Add(Restrictions.Eq("Closed", false))
                .Add(Restrictions.Gt("Opened", DateTime.Now.ToUniversalTime().Subtract(TimeSpan.FromMinutes(5))))
                .List<DrawingSession>();
        }

        public string GetImageUrl(Brick brick)
        {
            return GetImageUrl(
                brick.Wall.InviteCode,
                brick.AddressX,
                brick.AddressY);
        }
        public string GetThumbnailImageUrl(Brick brick)
        {
            return GetThumbnailImageUrl(brick.Wall.InviteCode,
                brick.AddressX,
                brick.AddressY);
        }

        public byte[] GetThumbnailImage(Brick brick)
        {
            string thumbnailImagePath = string.Format("{0}/b_{1}-{2}-{3}_1.png",
                ConfigurationManager.AppSettings["storageContainer"],
                brick.Wall.InviteCode, brick.AddressX, brick.AddressY);
            return _fileStorage.Get(thumbnailImagePath);
        }
        public byte[] GetImage(Brick brick)
        {
            string fullSizeImagePath = string.Format("{0}/b_{1}-{2}-{3}.png",
                    ConfigurationManager.AppSettings["storageContainer"],
                    brick.Wall.InviteCode, brick.AddressX, brick.AddressY);
            return _fileStorage.Get(fullSizeImagePath);
        }

        public string GetImageUrl(string inviteCode, int addressX, int addressY)
        {
            return string.Format("{0}/v1/image/{1}/{2}/{3}",
                ConfigurationManager.AppSettings["MMApi"],
                inviteCode,
                addressX,
                addressY);
        }
        public string GetThumbnailImageUrl(string inviteCode, int addressX, int addressY)
        {
            return string.Format("{0}/v1/image/t/{1}/{2}/{3}",
                ConfigurationManager.AppSettings["MMApi"],
                inviteCode,
                addressX,
                addressY);
        }

        public string GetCacheKey(string wallKey, int originX, int originY)
        {
            return string.Format("{0}_{1}_{2}", wallKey, originX, originY);
        }

        public BrickViewModel[,] GetWallSnapshot(int originX, int originY, string wallKey, IStatelessSession session,bool mustBeUpToDate=false, bool updateCache=true)
        {
            if (!mustBeUpToDate)
            {
                var cachedBricks = _wallAtLocationCache.Get(GetCacheKey(wallKey, originX, originY));
                if (cachedBricks != null && cachedBricks.Set != null)
                    return cachedBricks.Set;
            }

            return GetLatestWallSnapshotAndUpdateCache(originX, originY, wallKey, session, updateCache);
        }

        public BrickViewModel[,] GetLatestWallSnapshotAndUpdateCache(int originX, int originY, string wallKey, IStatelessSession session, bool updateCache= true)
        {
            var wall = GetLatestWallSnapshot(originX, originY, wallKey, session);

            if (updateCache)
            {
                _wallAtLocationCache.Set(GetCacheKey(wallKey, originX, originY), new BrickWallSet(wall) {Hello = "test"});
            }

            return wall;
        }
        public BrickViewModel[,] GetLatestWallSnapshotAndUpdateCache(int originX, int originY, string wallKey, ISession session, bool updateCache = true)
        {
            var wall = GetLatestWallSnapshot(originX, originY, wallKey, session);

            if (updateCache)
            {
                _wallAtLocationCache.Set(GetCacheKey(wallKey, originX, originY), new BrickWallSet(wall) {Hello = "test"});
            }

            return wall;
        }

        public byte[] GetFullWallImage(string wallKey)
        {
            var wallImageFilename = string.Format("{0}/w_{1}.png",
                ConfigurationManager.AppSettings["storageContainer"],
                wallKey);
            if (_fileStorage.Exists(wallImageFilename))
            {
                return _fileStorage.Get(wallImageFilename);
            }
            else
            {
                return UpdateFullWallImage(wallKey, wallImageFilename);
            }
        }

        private byte[] UpdateFullWallImage(string wallKey, string wallImageFilename)
        {
            _log.Debug("Updating wall image " + wallKey);
            int thumbnailWidth = 0, thumbnailHeight = 0;
            int.TryParse(ConfigurationManager.AppSettings["thumbnailWidth"], out thumbnailWidth);
            int.TryParse(ConfigurationManager.AppSettings["thumbnailHeight"], out thumbnailHeight);

            using (System.Drawing.Image newImage = new Bitmap(thumbnailWidth*12, thumbnailHeight*12))
            {
                using (var session = SessionFactory.Instance.OpenSession())
                {
                    var wall = GetWallByKeyOrDefault(wallKey, session);
                    if (wall == null)
                    {
                        return null;
                    }

                    int width = 12;
                    int height = 12;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var brick =
                                wall.Bricks.SingleOrDefault(b => b.AddressX == (x - 6) && b.AddressY == (y - 6));
                            if (brick == null)
                                continue;

                            Image img = Image.FromStream(new System.IO.MemoryStream(GetThumbnailImage(brick)));
                            using (Graphics g = Graphics.FromImage(newImage))
                            {
                                g.DrawImage(img, new System.Drawing.Point((thumbnailWidth*x), (thumbnailHeight*y)));
                            }
                        }
                    }

                    var outstream = new System.IO.MemoryStream();
                    newImage.Save(outstream, ImageFormat.Png);
                    _fileStorage.Store(wallImageFilename, outstream.ToArray(), true);
                    _log.Debug("Updated wall image " + wallKey);
                    return outstream.ToArray();
                }
            }
        }

        private BrickViewModel[,] GetLatestWallSnapshot(int originX, int originY, string wallKey, ISession session)
        {
            Wall wallRecord = GetWallByKeyOrDefault(wallKey, session);

            IList<object[]> bricks = session.QueryOver<Brick>()
                .And(b => b.Wall.Id == wallRecord.Id)
                .Select(
                    b => b.AddressX,
                    b => b.AddressY,
                    b => b.LastUpdated,
                    b => b.Guid).List<object[]>();

            var openDrawingSessionsForThisWall = session.QueryOver<DrawingSession>()
                .And(s => !s.Closed)
                .And(s => s.Wall.Id == wallRecord.Id)
                .Select(
                    s => s.AddressX,
                    s => s.AddressY,
                    s=>s.Opened)
                    .List<object[]>();

            openDrawingSessionsForThisWall =
                openDrawingSessionsForThisWall.Where(
                    s =>
                        (DateTime) s[2] >
                        DateTime.Now.ToUniversalTime().Subtract(TimeSpan.FromMinutes(5))).ToList();

            var wall = new BrickViewModel[12, 12];
            for (int y = 0; y < 12; y++)
            {
                for (int x = 0; x < 12; x++)
                {
                    var addressX = originX - 6 + x;
                    var addressY = originY - 6 + y;
                    var o = bricks.SingleOrDefault(b => (int)b[0] == addressX && (int)b[1] == addressY);
                    if (o != null)
                    {
                        wall[x, y] = new BrickViewModel()
                        {
                            X = addressX,
                            Y = addressY,
                            C = 1,
                            D = ((DateTime)o[2]).Ticks,
                            U = openDrawingSessionsForThisWall.Any(s=>(int)s[0]== addressX && (int)s[1]== addressY) ? 1 : 0
                        };
                    }
                    else
                    {
                        wall[x, y] = new BrickViewModel()
                        {
                            X = addressX,
                            Y = addressY,
                            C = 0,
                            U = openDrawingSessionsForThisWall.Any(s => (int)s[0] == addressX && (int)s[1] == addressY) ? 1 : 0
                        };
                    }
                }
            }
            return wall;
        }

        private BrickViewModel[,] GetLatestWallSnapshot(int originX, int originY, string wallKey, IStatelessSession session)
        {
            Wall wallRecord = GetWallByKeyOrDefault(wallKey, session);

            IList<object[]> bricks = session.QueryOver<Brick>()
                .And(b => b.Wall.Id == wallRecord.Id)
                .Select(
                    b => b.AddressX,
                    b => b.AddressY,
                    b => b.LastUpdated,
                    b => b.Guid).List<object[]>();

            var openDrawingSessionsForThisWall = session.QueryOver<DrawingSession>()
                .And(s => !s.Closed)
                .And(s => s.Wall.Id == wallRecord.Id)
                .Select(
                    s=>s.AddressX,
                    s=>s.AddressY,
                    s => s.Opened)
                    .List<object[]>();

            openDrawingSessionsForThisWall =
                openDrawingSessionsForThisWall.Where(
                    s =>
                        (DateTime)s[2] >
                        DateTime.Now.ToUniversalTime().Subtract(TimeSpan.FromMinutes(5))).ToList();


            var wall = new BrickViewModel[12, 12];
            for (int y = 0; y < 12; y++)
            {
                for (int x = 0; x < 12; x++)
                {
                    var addressX = originX - 6 + x;
                    var addressY = originY - 6 + y;
                    var o = bricks.SingleOrDefault(b => (int) b[0] == addressX && (int) b[1] == addressY);
                    if (o != null)
                    {
                        wall[x, y] = new BrickViewModel()
                        {
                            X = addressX,
                            Y = addressY,
                            C = 1,
                            D = 100,
                            U = openDrawingSessionsForThisWall.Any(s=>(int)s[0]== addressX && (int)s[1]== addressY) ?1:0
                        };
                    }
                    else
                    {
                        wall[x, y] = new BrickViewModel()
                        {
                            X = addressX,
                            Y = addressY,
                            C = 0,
                            U = openDrawingSessionsForThisWall.Any(s => (int)s[0] == addressX && (int)s[1] == addressY) ? 1 : 0
                        };
                    }
                }
            }
            return wall;
        }

        private bool BrickInUse(IStatelessSession session, int addressX, int addressY, int wallId)
        {
            return session.CreateCriteria<DrawingSession>()
                .Add(Restrictions.Eq("Closed", false))
                .Add(Restrictions.Gt("Opened", DateTime.Now.Subtract(TimeSpan.FromMinutes(5))))
                .Add(Restrictions.Eq("AddressX", addressX))
                .Add(Restrictions.Eq("AddressY", addressY))
                .Add(Restrictions.Eq("Wall.Id", wallId))
                .SetProjection(Projections.Count("Id"))
                .UniqueResult<int>() > 0;
        }
        private bool BrickInUse(ISession session, int addressX, int addressY, int wallId)
        {
            return session.CreateCriteria<DrawingSession>()
                .Add(Restrictions.Eq("Closed", false))
                .Add(Restrictions.Gt("Opened", DateTime.Now.Subtract(TimeSpan.FromMinutes(5))))
                .Add(Restrictions.Eq("AddressX", addressX))
                .Add(Restrictions.Eq("AddressY", addressY))
                .Add(Restrictions.Eq("Wall.Id", wallId))
                .SetProjection(Projections.Count("Id"))
                .UniqueResult<int>() > 0;
        }
    }
}
