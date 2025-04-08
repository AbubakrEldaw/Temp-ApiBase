using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class PermRolePerm
    {
        public string PermRoleId { get; set; }
        public string PermId { get; set; }

        public virtual Perm Perm { get; set; }
        public virtual PermRole PermRole { get; set; }
    }
}
