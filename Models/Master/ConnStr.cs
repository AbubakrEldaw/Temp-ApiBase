using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class ConnStr
    {
        public string CompanyId { get; set; } = null!;
        public string ServerName { get; set; } = null!;
        public string InstanceName { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string? UserName { get; set; }
        public string Password { get; set; } = null!;

        public virtual Company Company { get; set; } = null!;
    }
}
