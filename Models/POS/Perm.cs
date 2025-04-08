using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class Perm
    {
        public Perm()
        {
            PermRolePerms = new HashSet<PermRolePerm>();
        }

        public string Id { get; set; }
        public string PermGroupId { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public int OrderIndex { get; set; }

        public virtual PermGroup PermGroup { get; set; }
        public virtual ICollection<PermRolePerm> PermRolePerms { get; set; }
    }
}
