using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class AggrItemMapping
    {
        public string AggrId { get; set; }
        public string ItemType { get; set; }
        public string ItemAggrId { get; set; }
        public string AggrVariantId { get; set; }
        public string ItemId { get; set; }
        public string ItemVariantId { get; set; }
        public string CustomerComentId { get; set; }
        public decimal? AggrPrice { get; set; }
    }
}
