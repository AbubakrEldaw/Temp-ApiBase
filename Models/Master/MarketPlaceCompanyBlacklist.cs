using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class MarketPlaceCompanyBlacklist
    {
        public string CompanyId { get; set; }
        public string MarketPlaceAppId { get; set; }
        public string Reason { get; set; }

        public virtual Company Company { get; set; }
        public virtual MarketPlaceApp MarketPlaceApp { get; set; }
    }
}
