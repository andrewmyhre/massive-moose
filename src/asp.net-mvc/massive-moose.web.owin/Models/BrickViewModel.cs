namespace massive_moose.web.owin.Models
{
    public class BrickViewModel
    {
        public int AddressX { get; set; }
        public int AddressY { get; set; }
        public string ImageUrl { get; internal set; }
        public string ThumbnailImageUrl { get; set; }
    }
}