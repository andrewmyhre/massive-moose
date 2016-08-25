using massive_moose.services.models;

namespace massive_moose.web.owin.Models
{
    public class WallViewModel
    {
        public BrickViewModel[,] Bricks { get; set; }
        public string InviteCode { get; set; }
        public Wall Wall { get; set; }
        public WallHistoryItem[] History { get; internal set; }
        public WallHistoryItem[] BrickHistory { get; internal set; }

        public WallViewModel()
        {
            Bricks = new BrickViewModel[12,12];
        }
    }
}