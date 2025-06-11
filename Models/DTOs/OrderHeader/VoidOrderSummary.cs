using APIBase.Models.DTOs.ApiOrder;
using APIBase.Models.POS;

namespace APIBase.Models.DTOs.OrderHeader
{
    public class VoidOrderByEmployeeSummary
    {
        public Employee Employee { get; set; }
        public List<VoidOrderSummary> VoidOrders { get; set; } = [];
    }

    public class VoidOrderSummary
    {
        public Guid Id { get; set; }
        public int OrderNumber { get; set; }
        public List<OrderItemSummary> Items { get; set; } = [];
        public DateTime? VoidAt { get; set; }
        public bool IsVoid { get; set; }
        public string VoidTypeId { get; set; }
        public string VoidReasonId { get; set; }
        public decimal TotalVoid { get; set; }
        public decimal Total { get; set; }
    }
}
