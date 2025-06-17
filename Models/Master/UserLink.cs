using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class UserLink
    {
        public int UserId { get; set; }
        public int LinkedUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual User LinkedUser { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
