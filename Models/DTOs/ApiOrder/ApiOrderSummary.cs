using APIBase.Models.POS;

namespace APIBase.Models.DTOs.ApiOrder
{
    // TODO: only user transactionlinetypes enum
    public class ApiOrderSummary
    {
        public POS.ApiOrder ApiOrder { get; set; }
        public List<OrderItemSummary> Items { get; set; }
        public LocalizedName DiningOption { get; set; }
        public LocalizedName OrderSource { get; set; }
        public string Note { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderItemSummary
    {
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public decimal DiscountAmount { get; set; }
        public LocalizedName Name { get; set; }

    }

}
