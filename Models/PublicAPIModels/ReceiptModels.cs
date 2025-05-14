namespace APIBase.PublicAPIModels
{
#nullable disable
    public class WorkShiftResponse
    {
        public List<WorkShift> Workshifts { get; set; }
    }
    public class WorkShift
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
        public List<Receipt> Receipts { get; set; }
    }
    public class Receipt
    {
        public string Id { get; set; }
        public string WorkDayDate { get; set; }
        public string LocationId { get; set; }
        public string Type { get; set; } // sales, return
        public int OrderNumber { get; set; }
        public string OrderSourceId { get; set; }
        public string OrderSource { get; set; }
        public string DiningOptionId { get; set; }
        public string DiningOption { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalFees { get; set; }
        public decimal TotalTaxes { get; set; }
        public decimal NetTotal { get; set; }
        public string CreateTime { get; set; }
        public string LastModifiedTime { get; set; }
        public string PaidTime { get; set; }
        public string InvoiceNumber { get; set; }
        public List<ReceiptLine> ReceiptLines { get; set; }
        public List<ReceiptPayment> Payments { get; set; }
    }
    public class ReceiptLine
    {
        public string Id { get; set; }
        public string ItemId { get; set; }
        public string VariantId { get; set; }
        public string ItemDescription { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal Total { get; set; }
        public bool PriceTaxInclusive { get; set; }
        public decimal TaxAmount { get; set; }
        public string CreateTime { get; set; }
        public string ModifyTime { get; set; }
        public bool Void { get; set; }
        public string VoidType { get; set; } //Waste, No Waste
        public List<ModifierLine> Modifiers { get; set; }
    }
    public class ModifierLine
    {
        public string Id { get; set; }
        public string ItemId { get; set; }
        public string ItemDescription { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal Total { get; set; }
        public bool PriceTaxInclusive { get; set; }
        public decimal TaxAmount { get; set; }
        public string CreateTime { get; set; }
        public string ModifyTime { get; set; }
        public bool Void { get; set; }
        public string VoidType { get; set; } //Waste, No Waste
    }
    public class ReceiptPayment
    {
        public int LineIndex { get; set; }
        public string PaymentType { get; set; }
        public decimal Amount { get; set; }
    }
    public class Rplist
    {
        public List<ReceiptPayment> ReceiptPayments { get; set; }
    }
}
