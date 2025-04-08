using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class MarketPlaceCategory
    {
        public MarketPlaceCategory()
        {
            MarketPlaceApps = new HashSet<MarketPlaceApp>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual ICollection<MarketPlaceApp> MarketPlaceApps { get; set; }
    }
}
