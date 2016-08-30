using System.Configuration;
using massive_moose.services.models;

namespace massive_moose.services.viewmodels
{
    public class WallViewModel
    {
        public BrickViewModel[,] Bricks { get; set; }
        public string InviteCode { get; set; }
        public Wall Wall { get; set; }
        public WallHistoryItem[] History { get; set; }
        public WallHistoryItem[] FocusBrickHistory { get; set; }
        public int DetailForX { get; set; }
        public int DetailForY { get; set; }
        public BrickViewModel FocusBrick { get; set; }
        public string BackgroundImageUrl { get; set; }
        public bool DontHelpMe { get; set; }

        public WallViewModel()
        {
            Bricks = new BrickViewModel[12,12];
        }

        public string GetThumbnailUrl(BrickViewModel brick)
        {
            return string.Format("{0}/v1/image/t/{1}/{2}/{3}",
                ConfigurationManager.AppSettings["MMApi"],
                InviteCode, brick.X, brick.Y);
        }
        public string GetImageUrl(BrickViewModel brick)
        {
            return string.Format("{0}/v1/image/{1}/{2}/{3}",
                ConfigurationManager.AppSettings["MMApi"],
                InviteCode, brick.X, brick.Y);
        }
    }
}