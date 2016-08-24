namespace massive_moose.web.owin.Models
{
    public class BrickViewModel
    {
        public int AddressX { get; set; }
        public int AddressY { get; set; }
        public string ImageUrl { get; set; }
    }

    public class WallViewModel
    {
        public BrickViewModel[,] Bricks { get; set; }

        public WallViewModel()
        {
            Bricks = new BrickViewModel[12,12];
        }
    }
}