using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class ApiOrderStagingStatus
    {
        public ApiOrderStagingStatus()
        {
            ApiOrderStatusHistories = new HashSet<ApiOrderStatusHistory>();
            ApiOrders = new HashSet<ApiOrder>();
            OrderHeaders = new HashSet<OrderHeader>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual ICollection<ApiOrderStatusHistory> ApiOrderStatusHistories { get; set; }
        public virtual ICollection<ApiOrder> ApiOrders { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaders { get; set; }
    }
}
