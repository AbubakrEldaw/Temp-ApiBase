using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class AppsCredential
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string AppSecret { get; set; } = null!;
        public bool IsActive { get; set; }
        public string CompanyId { get; set; } = null!;
        public int UserId { get; set; }
        public string? ConnStrCompanyId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
