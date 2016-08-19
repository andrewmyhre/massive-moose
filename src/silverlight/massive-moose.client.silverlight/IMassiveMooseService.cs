using System;

namespace massive_moose.client.silverlight
{
    public interface IMassiveMooseService
    {
        event EventHandler<GotBricksEventArgs> OnGotBricks;
        void GetBricksAround(int originX, int originY);
    }
}