using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class OrderHeader
    {
        public OrderHeader()
        {
            OrderFees = new HashSet<OrderFee>();
            OrderItems = new HashSet<OrderItem>();
            OrderPayments = new HashSet<OrderPayment>();
        }

        public byte[] RowVersion { get; set; }
        public Guid Id { get; set; }
        public Guid? ServerId { get; set; }
        public Guid WorkDayId { get; set; }
        public string BranchId { get; set; }
        public Guid? WorkDayShiftId { get; set; }
        public bool IsReturn { get; set; }
        public int OrderNumber { get; set; }
        public string OrderSourceId { get; set; }
        public string DiningOptionId { get; set; }
        public string DeviceId { get; set; }
        public string AreaId { get; set; }
        public string AreaTableId { get; set; }
        public string OrderStatusId { get; set; }
        public int? GuestCount { get; set; }
        public string WaiterId { get; set; }
        public string DiscountId { get; set; }
        public string DiscountTypeId { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal HeaderDiscountAmount { get; set; }
        public decimal LineTotal { get; set; }
        public decimal FeesTotal { get; set; }
        public decimal Total { get; set; }
        public decimal TotalVat { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }
        public string PaidBy { get; set; }
        public DateTime? PaidAt { get; set; }
        public string VoidBy { get; set; }
        public DateTime? VoidAt { get; set; }
        public string VoidTypeId { get; set; }
        public string VoidReasonId { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerNotes { get; set; }
        public string Note { get; set; }
        public DateTime? CloseAt { get; set; }
        public string CloseBy { get; set; }
        public string AppId { get; set; }
        public string AppOrderId { get; set; }
        public string AppOrderNumber { get; set; }
        public string AppChannelName { get; set; }
        public string AppChannelOrderNumber { get; set; }
        public DateTime? AppOrderReceiveDatetime { get; set; }
        public DateTime? AppOrderPickupDatetime { get; set; }
        public DateTime? AppOrderDeliveryDatetime { get; set; }
        public string AppOrderStatus { get; set; }
        public string VatReportStatus { get; set; }
        public string ZatcaQrCode { get; set; }
        public string ZatcaSignedXmlInvoice { get; set; }
        public string ZatcaReportStatus { get; set; }
        public string ZatcaResponseJson { get; set; }
        public string InvoiceNumber { get; set; }
        public string ZatcaInvoiceHash { get; set; }
        public long? ZatcaInvoiceCounter { get; set; }
        public string SupplierCrn { get; set; }
        public string SupplierTin { get; set; }
        public string SupplierVatNo { get; set; }
        public string SupplierRegistrationName { get; set; }
        public string SupplierStreetName { get; set; }
        public string SupplierBuildingNumber { get; set; }
        public string SupplierPlotIdentification { get; set; }
        public string SupplierCitySubdivisionName { get; set; }
        public string SupplierCityName { get; set; }
        public string SupplierPostalZone { get; set; }
        public string SupplierCountrySubentity { get; set; }
        public string SupplierCountry { get; set; }
        public string CustomerNat { get; set; }
        public string CustomerCrn { get; set; }
        public string CustomerTin { get; set; }
        public string CustomerVatNo { get; set; }
        public string CustomerRegistrationName { get; set; }
        public string CustomerStreetName { get; set; }
        public string CustomerBuildingNumber { get; set; }
        public string CustomerPlotIdentification { get; set; }
        public string CustomerCitySubdivisionName { get; set; }
        public string CustomerCityName { get; set; }
        public string CustomerPostalZone { get; set; }
        public string CustomerCountrySubentity { get; set; }
        public string CustomerCountry { get; set; }

        public virtual ApiOrderStagingStatus AppOrderStatusNavigation { get; set; }
        public virtual Area Area { get; set; }
        public virtual Branch Branch { get; set; }
        public virtual Employee CloseByNavigation { get; set; }
        public virtual Employee CreateByNavigation { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual PosDevice Device { get; set; }
        public virtual DiningOption DiningOption { get; set; }
        public virtual Discount Discount { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual OrderSource OrderSource { get; set; }
        public virtual OrderStatus OrderStatus { get; set; }
        public virtual Employee PaidByNavigation { get; set; }
        public virtual Employee VoidByNavigation { get; set; }
        public virtual VoidReason VoidReason { get; set; }
        public virtual VoidType VoidType { get; set; }
        public virtual Employee Waiter { get; set; }
        public virtual WorkDay WorkDay { get; set; }
        public virtual WorkDayShift WorkDayShift { get; set; }
        public virtual ICollection<OrderFee> OrderFees { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<OrderPayment> OrderPayments { get; set; }
    }
}
