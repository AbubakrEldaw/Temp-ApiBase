using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class UserLink
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public int LinkedUserId { get; set; }

        public virtual User LinkedUser { get; set; }
        public virtual User User { get; set; }
    }
}
