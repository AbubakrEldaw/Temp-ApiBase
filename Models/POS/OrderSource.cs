using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class OrderSource
    {
        public OrderSource()
        {
            CallCenterDevices = new HashSet<CallCenterDevice>();
            OrderHeaders = new HashSet<OrderHeader>();
            PosDevices = new HashSet<PosDevice>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public int OrderIndex { get; set; }
        public string StatusId { get; set; }

        public virtual Status Status { get; set; }
        public virtual ICollection<CallCenterDevice> CallCenterDevices { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaders { get; set; }
        public virtual ICollection<PosDevice> PosDevices { get; set; }
    }
}
