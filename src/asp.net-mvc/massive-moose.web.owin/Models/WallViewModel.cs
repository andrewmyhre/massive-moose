using massive_moose.services.models;

namespace massive_moose.web.owin.Models
{
    public class WallViewModel
    {
        public BrickViewModel[,] Bricks { get; set; }
        public string InviteCode { get; set; }
        public Wall Wall { get; set; }
        public WallHistoryItem[] History { get; internal set; }
        public WallHistoryItem[] FocusBrickHistory { get; internal set; }
        public int DetailForX { get; internal set; }
        public int DetailForY { get; internal set; }
        public BrickViewModel FocusBrick { get; internal set; }
        public string BackgroundImageUrl { get; internal set; }
        public bool DontHelpMe { get; internal set; }

        public WallViewModel()
        {
            Bricks = new BrickViewModel[12,12];
        }
    }
}