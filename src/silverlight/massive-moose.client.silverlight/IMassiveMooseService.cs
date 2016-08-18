using System;

namespace massive_moose.client.silverlight
{
    public interface IMassiveMooseService
    {
        event EventHandler<GotBricksEventArgs> OnGotBricks;
        void GetBricksInRange(int minX, int minY, int maxX, int maxY);
    }
}