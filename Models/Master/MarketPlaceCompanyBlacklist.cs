using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class MarketPlaceCompanyBlacklist
    {
        public string CompanyId { get; set; } = null!;
        public string MarketPlaceAppId { get; set; } = null!;
        public string Reason { get; set; } = null!;

        public virtual Company Company { get; set; } = null!;
        public virtual MarketPlaceApp MarketPlaceApp { get; set; } = null!;
    }
}
