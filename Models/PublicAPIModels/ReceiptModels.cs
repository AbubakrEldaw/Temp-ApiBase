namespace APIBase.PublicAPIModels
{
#nullable disable
    public class WorkShiftResponse
    {
        public List<WorkShift> Workshifts { get; set; }
    }
    public class WorkShift
    {
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string Id { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string Date { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string OpenTime { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string CloseTime { get; set; }
        public List<Receipt> Receipts { get; set; }
    }
    public class Receipt
    {
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string Id { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string WorkDayDate { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string LocationId { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string Type { get; set; } // sales, return
        /// <example>12</example>
        public int OrderNumber { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string OrderSourceId { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string OrderSource { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string DiningOptionId { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string DiningOption { get; set; }
        /// <example>7.5</example>
        public decimal TotalDiscount { get; set; }
        /// <example>5.4</example>
        public decimal TotalFees { get; set; }
        /// <example>65.5</example>
        public decimal TotalTaxes { get; set; }
        /// <example>6.8</example>
        public decimal NetTotal { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string CreateTime { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string LastModifiedTime { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string PaidTime { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string InvoiceNumber { get; set; }
        public List<ReceiptLine> ReceiptLines { get; set; }
        public List<ReceiptPayment> Payments { get; set; }
    }
    public class ReceiptLine
    {
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string Id { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string ItemId { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string VariantId { get; set; }
        /// <example>string</example>
        public string ItemDescription { get; set; }
        /// <example>6.4</example>
        public decimal Quantity { get; set; }
        /// <example>9.4</example>
        public decimal Price { get; set; }
        /// <example>6.5</example>
        public decimal TotalDiscountAmount { get; set; }
        /// <example>5.4</example>
        public decimal Total { get; set; }
        /// <example>true</example>
        public bool PriceTaxInclusive { get; set; }
        /// <example>5.4</example>
        public decimal TaxAmount { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string CreateTime { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string ModifyTime { get; set; }
        /// <example>true</example>
        public bool Void { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string VoidType { get; set; } //Waste, No Waste
        public List<ModifierLine> Modifiers { get; set; }
    }
    public class ModifierLine
    {
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string Id { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string ItemId { get; set; }
        /// <example>string</example>
        public string ItemDescription { get; set; }
        /// <example>6.4</example>
        public decimal Quantity { get; set; }
        /// <example>9.4</example>
        public decimal Price { get; set; }
        /// <example>6.5</example>
        public decimal TotalDiscountAmount { get; set; }
        /// <example>5.4</example>
        public decimal Total { get; set; }
        /// <example>true</example>
        public bool PriceTaxInclusive { get; set; }
        /// <example>5.4</example>
        public decimal TaxAmount { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string CreateTime { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string ModifyTime { get; set; }
        /// <example>true</example>
        public bool Void { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string VoidType { get; set; } //Waste, No Waste
    }
    public class ReceiptPayment
    {
        /// <example>5</example>
        public int LineIndex { get; set; }
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        public string PaymentType { get; set; }
        /// <example>6.4</example>
        public decimal Amount { get; set; }
    }
    public class Rplist
    {
        public List<ReceiptPayment> ReceiptPayments { get; set; }
    }
}
