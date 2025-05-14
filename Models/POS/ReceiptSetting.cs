using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class ReceiptSetting
    {
        public string BranchId { get; set; }
        public string ImagePath { get; set; }
        public string ImageName { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public bool ShowCustomerInfo { get; set; }
        public bool ShowCustomerComment { get; set; }

        public virtual Branch Branch { get; set; }
    }
}
