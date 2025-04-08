using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class PermRole
    {
        public PermRole()
        {
            Employees = new HashSet<Employee>();
            PermRolePerms = new HashSet<PermRolePerm>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<PermRolePerm> PermRolePerms { get; set; }
    }
}
