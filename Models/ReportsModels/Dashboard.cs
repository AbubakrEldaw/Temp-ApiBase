using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.ReportsModels
{
    public partial class Dashboard
    {
        public decimal NetSales { get; set; }
        public decimal GrossSales { get; set; }
        public decimal Refunds { get; set; }
        public decimal Discounts { get; set; }
        public decimal GrossProfit { get; set; }

    }
}
