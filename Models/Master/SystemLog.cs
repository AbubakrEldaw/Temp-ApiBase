using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class SystemLog
    {
        public Guid Id { get; set; }
        public string CompanyId { get; set; } = null!;
        public string ErrorMessage { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? RequestJson { get; set; }
        public string? ResponseJson { get; set; }
        public DateTime Time { get; set; }
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }

        public virtual Company Company { get; set; } = null!;
    }
}
