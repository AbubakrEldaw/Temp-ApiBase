using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class PermGroup
    {
        public PermGroup()
        {
            Perms = new HashSet<Perm>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string Description { get; set; }
        public string Sdescription { get; set; }
        public string Icon { get; set; }

        public virtual ICollection<Perm> Perms { get; set; }
    }
}
