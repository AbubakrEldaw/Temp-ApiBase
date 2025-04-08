using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class ApiOrderStatusHistory
    {
        public Guid Id { get; set; }
        public DateTime StatusTime { get; set; }
        public Guid ApiOrderId { get; set; }
        public string ApiStatusId { get; set; }
        public string UpdateSource { get; set; }

        public virtual ApiOrder ApiOrder { get; set; }
        public virtual ApiOrderStagingStatus ApiStatus { get; set; }
    }
}
