using System;
using massive_moose.services.models;
using massive_moose.services.viewmodels;
using NHibernate;
using System.Collections.Generic;

namespace massive_moose.services
{
    public interface IWallOperations
    {
        Wall GetWallByKeyOrDefault(string wallKey, IStatelessSession session);
        Wall GetWallByKeyOrDefault(string wallKey, ISession session);

        void Contribute(string inputJson, Guid sessionToken,
            byte[] imageData, string snapshotJson, ISessionFactory sessionFactory, string clientIp);

        string GetImageUrl(Brick brick);
        string GetThumbnailImageUrl(Brick brick);
        string GetImageUrl(string inviteCode, int addressX, int addressY);
        string GetThumbnailImageUrl(string inviteCode, int addressX, int addressY);
        BrickViewModel[,] GetWallSnapshot(int originX, int originY, string wallKey, IStatelessSession session, bool mustBeUpToDate = false, bool updateCache = true);
        IList<DrawingSession> GetActiveDrawingSessions(int wallId, IStatelessSession session);

        BrickViewModel[,] GetLatestWallSnapshotAndUpdateCache(int originX, int originY, string wallKey, IStatelessSession session, bool updateCache = true);

        BrickViewModel[,] GetLatestWallSnapshotAndUpdateCache(int originX, int originY, string wallKey, ISession session, bool updateCache = true);
    }
}