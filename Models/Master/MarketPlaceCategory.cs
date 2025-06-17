using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class MarketPlaceCategory
    {
        public MarketPlaceCategory()
        {
            MarketPlaceApps = new HashSet<MarketPlaceApp>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Sname { get; set; } = null!;

        public virtual ICollection<MarketPlaceApp> MarketPlaceApps { get; set; }
    }
}
