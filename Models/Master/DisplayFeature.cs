using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class DisplayFeature
    {
        public DisplayFeature()
        {
            Plans = new HashSet<Plan>();
        }

        public string Id { get; set; } = null!;
        public int OrderIndex { get; set; }
        public string Name { get; set; } = null!;
        public string Sname { get; set; } = null!;
        public string? ImageUrl { get; set; }

        public virtual ICollection<Plan> Plans { get; set; }
    }
}
