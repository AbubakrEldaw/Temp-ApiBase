using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class MenuGroup
    {
        public string Id { get; set; }
        public string MenuId { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string ImagePath { get; set; }
        public string ImageName { get; set; }
        public string StatusId { get; set; }
        public int OrderIndex { get; set; }

        public virtual Menu Menu { get; set; }
        public virtual Status Status { get; set; }
    }
}
