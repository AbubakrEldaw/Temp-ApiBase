using APIBase.Models.DTOs.ApiOrder;

namespace APIBase.Models.DTOs.OrderHeader
{
    public class VoidOrderSummary
    {
        public Guid Id { get; set; }
        public int OrderNumber { get; set; }
        public List<OrderItemSummary> Items { get; set; } = [];
        public DateTime? VoidAt { get; set; }
        public bool IsVoid { get; set; }
        public decimal TotalVoid { get; set; }
        public decimal Total { get; set; }
    }

        //
    //public LocalizedName DiningOption { get; set; } = new LocalizedName();
    //public LocalizedName OrderSource { get; set; } = new LocalizedName();
    //public LocalizedName? VoidType { get; set; }
    //public LocalizedName? VoidReason { get; set; }
    //public bool IsVoid { get; set; }
}
