using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using massive_moose.services.models;
using NHibernate;
using NHibernate.Criterion;
using massive_moose.services.models.drawing;
using massive_moose.services.viewmodels;
using massive_moose.services.caching;
using massive_moose.caching;

namespace massive_moose.services
{
    public class WallOperations : IWallOperations
    {
        private readonly IFileStorage _fileStorage;
        private readonly ISessionFactory _sessionFactory;
        private readonly IObjectCache<Wall> _wallCache;
        private readonly IObjectCache<BrickWallSet> _wallAtLocationCache;

        public WallOperations(IFileStorage fileStorage, 
            ISessionFactory sessionFactory,
            IObjectCache<Wall> wallCache,
            IObjectCache<BrickWallSet> wallAtLocationCache)
        {
            _fileStorage = fileStorage;
            _sessionFactory = sessionFactory;
            _wallCache = wallCache;
            _wallAtLocationCache = wallAtLocationCache;
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
            using (var session = _sessionFactory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                try
                {
                    var drawingSession = session.CreateCriteria<DrawingSession>()
                        .Add(Restrictions.Eq("SessionToken", sessionToken))
                        .UniqueResult<DrawingSession>();

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

                    brick.LastUpdated = DateTime.Now;
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
                    wallHistoryItem.Timestamp = DateTime.Now;
                    wallHistoryItem.ClientIp = clientIp;
                    wallHistoryItem.DrawingSession = drawingSession;
                    session.Save(wallHistoryItem);
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw;
                }
            }
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

        public BrickViewModel[,] GetBricksForWall(int originX, int originY, string wallKey, IStatelessSession session)
        {
            string cacheKey = string.Format("{0}_{1}_{2}", wallKey, originX, originY);
            var cachedBricks = _wallAtLocationCache.Get(cacheKey);
            if (cachedBricks != null && cachedBricks.Set != null)
                return cachedBricks.Set;

            Wall wallRecord = GetWallByKeyOrDefault(wallKey, session);

            IList<object[]> bricks = session.QueryOver<Brick>()
                .And(b => b.Wall.Id == wallRecord.Id)
                .Select(
                    b => b.AddressX,
                    b => b.AddressY,
                    b => b.LastUpdated,
                    b => b.Guid).List<object[]>();

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
                            X = (int)o[0],
                            Y = (int)o[1],
                            C=1,
                            D=((DateTime)o[2]).Ticks,
                            U = BrickInUse(session, addressX, addressY, wallRecord.Id) ? 1 : 0
                        };
                    }
                    else
                    {
                        wall[x, y] = new BrickViewModel()
                        {
                            X = addressX,
                            Y = addressY,
                            C = 0,
                            U = BrickInUse(session, addressX, addressY, wallRecord.Id) ? 1 : 0
                        };
                    }
                }
            }

            _wallAtLocationCache.Set(cacheKey, new BrickWallSet(wall) {Hello="test"});

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
    }
}
