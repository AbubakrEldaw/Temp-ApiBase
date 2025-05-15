using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class SystemLog
    {
        public Guid Id { get; set; }
        public string CompanyId { get; set; }
        public string ErrorMessage { get; set; }
        public string Type { get; set; }
        public string Action { get; set; }
        public string RequestJson { get; set; }
        public string ResponseJson { get; set; }
        public DateTime Time { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }

        public virtual Company Company { get; set; }
    }
}
