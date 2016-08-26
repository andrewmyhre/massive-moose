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

namespace massive_moose.services
{
    public class WallOperations
    {
        private readonly IFileStorage _fileStorage;

        public WallOperations(IFileStorage fileStorage)
        {
            _fileStorage = fileStorage;
        }
        public Wall GetWallByKeyOrDefault(string wallKey, IStatelessSession session)
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

        public void Contribute(string inputJson, Canvas canvas, DrawingSession drawingSession, ISession dbSession, string clientIp)
        {

            var imageData = new BrickRenderer().Render(canvas);
            string fullSizeImagePath = string.Format("{0}/b_{1}-{2}-{3}.png",
                ConfigurationManager.AppSettings["storageContainer"],
                drawingSession.Wall.InviteCode, drawingSession.AddressX, drawingSession.AddressY);
            _fileStorage.Store(fullSizeImagePath, imageData, true);

            int thumbnailWidth = 400, thumbnailHeight=200;
            int.TryParse(ConfigurationManager.AppSettings["thumbnailWidth"], out thumbnailWidth);
            int.TryParse(ConfigurationManager.AppSettings["thumbnailHeight"], out thumbnailHeight);

            string thumbnailImagePath = string.Format("{0}/b_{1}-{2}-{3}_1.png",
                ConfigurationManager.AppSettings["storageContainer"],
                drawingSession.Wall.InviteCode, drawingSession.AddressX, drawingSession.AddressY);
            System.IO.MemoryStream myMemStream = new System.IO.MemoryStream(imageData);
            System.Drawing.Image fullsizeImage = System.Drawing.Image.FromStream(myMemStream);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(thumbnailWidth, thumbnailHeight, null, IntPtr.Zero);
            System.IO.MemoryStream myResult = new System.IO.MemoryStream();
            newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Png);

            var thumbnailImageData = myResult.ToArray();
            _fileStorage.Store(thumbnailImagePath, thumbnailImageData, true);

            // store a history entry for this contribution
            var wallHistoryItem = new WallHistoryItem();
            wallHistoryItem.Wall = drawingSession.Wall;
            wallHistoryItem.SnapshotImage = imageData;
            wallHistoryItem.SnapshotImageThumbnail = thumbnailImageData;
            wallHistoryItem.SnapshotJson = inputJson;
            wallHistoryItem.Timestamp = DateTime.Now;
            wallHistoryItem.ClientIp = clientIp;
            wallHistoryItem.DrawingSession = drawingSession;
            dbSession.Save(wallHistoryItem);
            dbSession.Flush();
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
    }
}
