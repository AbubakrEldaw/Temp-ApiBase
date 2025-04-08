using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class MarketPlaceAppPlanAvailability
    {
        public string MarketPlaceAppId { get; set; }
        public string PlanId { get; set; }

        public virtual MarketPlaceApp MarketPlaceApp { get; set; }
        public virtual Plan Plan { get; set; }
    }
}
