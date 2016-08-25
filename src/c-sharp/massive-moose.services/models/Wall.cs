using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace massive_moose.services.models
{
    public class Wall
    {
        public Wall()
        {
            History = new List<WallHistoryItem>();
            Guid = Guid.NewGuid();
            InviteCode = GenerateInviteCode();
            DateCreated = DateTime.Now;
        }
        public virtual int Id { get; protected set; }
        public virtual Guid Guid { get; protected set; }
        public virtual ApplicationUser Owner { get; set; }
        public virtual DateTime DateCreated { get; set; }
        public virtual IList<WallHistoryItem> History { get; set; }
        public virtual string InviteCode { get; set; }

        private static Random random = new Random();
        private static string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        private static string GenerateInviteCode()
        {
            var code = "";
            for (var i = 0; i < 8; i++)
            {
                code += validChars[random.Next(validChars.Length - 1)];
            }
            return code;
        }
    }

    public class WallHistoryItem
    {
        public virtual int Id { get; protected set; }
        public virtual string SnapshotJson { get; set; }
        public virtual string ClientIp { get; set; }
        public virtual byte[] SnapshotImage { get; set; }
        public virtual byte[] SnapshotImageThumbnail { get; set; }
        public virtual Wall Wall { get; set; }
    }
}
