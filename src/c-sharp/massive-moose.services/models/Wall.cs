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
        public virtual string Label { get; set; }
        public virtual Guid Guid { get; protected set; }
        public virtual ApplicationUser Owner { get; set; }
        public virtual DateTime DateCreated { get; set; }
        public virtual IList<WallHistoryItem> History { get; set; }
        public virtual IList<Brick> Bricks { get; set; }
        public virtual string InviteCode { get; set; }
        public virtual string BackgroundImageFilename { get; set; }

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
}
