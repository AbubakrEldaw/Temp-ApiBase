using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class DiningOption
    {
        public DiningOption()
        {
            Areas = new HashSet<Area>();
            CallCenterDevices = new HashSet<CallCenterDevice>();
            OrderHeaders = new HashSet<OrderHeader>();
            PosDevices = new HashSet<PosDevice>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public int OrderIndex { get; set; }

        public virtual ICollection<Area> Areas { get; set; }
        public virtual ICollection<CallCenterDevice> CallCenterDevices { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaders { get; set; }
        public virtual ICollection<PosDevice> PosDevices { get; set; }
    }
}
