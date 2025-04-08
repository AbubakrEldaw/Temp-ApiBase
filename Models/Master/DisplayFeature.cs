using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class DisplayFeature
    {
        public DisplayFeature()
        {
            DisplayFeaturePlans = new HashSet<DisplayFeaturePlan>();
        }

        public string Id { get; set; }
        public int OrderIndex { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string ImageUrl { get; set; }

        public virtual ICollection<DisplayFeaturePlan> DisplayFeaturePlans { get; set; }
    }
}
