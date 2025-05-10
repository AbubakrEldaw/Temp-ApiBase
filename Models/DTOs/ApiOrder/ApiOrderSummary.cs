using APIBase.Models.POS;

namespace APIBase.Models.DTOs.ApiOrder
{
    // TODO: only user transactionlinetypes enum
    public class ApiOrderSummary
    {
        public POS.ApiOrder ApiOrder { get; set; }
        public List<OrderItemSummary> Items { get; set; }
    }

    public class OrderItemSummary
    {
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
    }

}
