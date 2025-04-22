using System;
using System.Collections.Generic;
using APIBase.Models.POS;
#nullable disable

namespace APIBase.Models.ReportsModels
{
    public partial class SalesReportByXNModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public decimal AverageOrder { get; set; }
        public decimal AveragePerGuest { get; set; }
        public int CustomersCount { get; set; }
        public int GuestsCount { get; set; }
        public int OrdersCount { get; set; }
        public decimal Cost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrossSales { get; set; }
        public decimal NetQuantity { get; set; }
        public decimal NetSales { get; set; }
        public decimal NetSalesWithTax { get; set; }
        public decimal Profit { get; set; }
        public decimal RefundQuantity { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal VoidQuantity { get; set; }
        public decimal VoidAmount { get; set; }
    }

    //public partial class SalesReportModel
    //{
    //    public string Id { get; set; }
    //    public string Name { get; set; }
    //    public string Sname { get; set; }
    //    public string EmployeeName { get; set; }
    //    public string EmployeeSname { get; set; }
    //    public Guid ShiftId { get; set; }
    //    public string ShiftStartTime { get; set; }
    //    public string ShiftEndTime { get; set; }
    //    public string ShiftDate { get; set; }
    //    public string Date { get; set; }
    //    public string ReceiptDate { get; set; }
    //    public int OrderNumber { get; set; }
    //    public string AverageOrder { get; set; } = "0";
    //    public string CustomerName { get; set; }
    //    public string ReceiptType { get; set; }
    //    public string ReceiptTotal { get; set; } = "0";
    //    public string AveragePerGuest { get; set; } = "0";
    //    public string CustomersCount { get; set; } = "0";
    //    public string GuestsCount { get; set; } = "0";
    //    public string OrdersCount { get; set; } = "0";
    //    public string Cost { get; set; } = "0";
    //    public string DiscountAmount { get; set; } = "0";
    //    public string VatAmount { get; set; } = "0";
    //    public string GrossSales { get; set; } = "0";
    //    public string NetQuantity { get; set; } = "0";
    //    public string NetSales { get; set; } = "0";
    //    public string NetSalesWithTax { get; set; } = "0";
    //    public string Profit { get; set; } = "0";
    //    public string RefundQuantity { get; set; } = "0";
    //    public string RefundAmount { get; set; } = "0";
    //    public string VoidQuantity { get; set; } = "0";
    //    public string VoidAmount { get; set; } = "0";
    //    public string PaymentAmount { get; set; } = "0";
    //    public string PaymentNetAmount { get; set; } = "0";
    //    public string PaymentCount { get; set; } = "0";
    //    public string PaymentRefundAmount { get; set; } = "0";
    //    public string PaymentRefundCount { get; set; } = "0";

    //}


    public partial class IdNameModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
    }

    public partial class CustomerModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }

    public partial class SalesReportByDateModel
    {
        public string Date { get; set; }
        public string AverageOrder { get; set; } = "0";
        public string AveragePerGuest { get; set; } = "0";
        public string CustomersCount { get; set; } = "0";
        public string GuestsCount { get; set; } = "0";
        public string OrdersCount { get; set; } = "0";
        public string Cost { get; set; } = "0";
        public string DiscountAmount { get; set; } = "0";
        public string VatAmount { get; set; } = "0";
        public string GrossSales { get; set; } = "0";
        public string NetQuantity { get; set; } = "0";
        public string NetSales { get; set; } = "0";
        public string NetSalesWithTax { get; set; } = "0";
        public string Profit { get; set; } = "0";
        public string RefundQuantity { get; set; } = "0";
        public string RefundAmount { get; set; } = "0";
        public string VoidQuantity { get; set; } = "0";
        public string VoidAmount { get; set; } = "0";
    }
    public partial class SalesReportByHourModel
    {
        public int Hour { get; set; }

        public string AverageOrder { get; set; } = "0";
        public string AveragePerGuest { get; set; } = "0";
        public string CustomersCount { get; set; } = "0";
        public string GuestsCount { get; set; } = "0";
        public string OrdersCount { get; set; } = "0";
        public string Cost { get; set; } = "0";
        public string DiscountAmount { get; set; } = "0";
        public string VatAmount { get; set; } = "0";
        public string GrossSales { get; set; } = "0";
        public string NetQuantity { get; set; } = "0";
        public string NetSales { get; set; } = "0";
        public string NetSalesWithTax { get; set; } = "0";
        public string Profit { get; set; } = "0";
        public string RefundQuantity { get; set; } = "0";
        public string RefundAmount { get; set; } = "0";
        public string VoidQuantity { get; set; } = "0";
        public string VoidAmount { get; set; } = "0";
    }
    public partial class SalesReportByShiftModel
    {
        public string Id { get; set; }
        public string EmpId { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string Date { set; get; }
        public string OpenedAt { set; get; }
        public String ClosedAt { set; get; }
        public string AverageOrder { get; set; } = "0";
        public string AveragePerGuest { get; set; } = "0";
        public string CustomersCount { get; set; } = "0";
        public string GuestsCount { get; set; } = "0";
        public string OrdersCount { get; set; } = "0";
        public string Cost { get; set; } = "0";
        public string DiscountAmount { get; set; } = "0";
        public string VatAmount { get; set; } = "0";
        public string GrossSales { get; set; } = "0";
        public string NetQuantity { get; set; } = "0";
        public string NetSales { get; set; } = "0";
        public string NetSalesWithTax { get; set; } = "0";
        public string Profit { get; set; } = "0";
        public string RefundQuantity { get; set; } = "0";
        public string RefundAmount { get; set; } = "0";
        public string VoidQuantity { get; set; } = "0";
        public string VoidAmount { get; set; } = "0";
    }

    public partial class SalesReportByWorkDayModel
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string Date { set; get; }
        public string OpenAt { set; get; }
        public String CloseAt { set; get; }
        public string OpenBy { set; get; }
        public String CloseBy { set; get; }
        public string AverageOrder { get; set; } = "0";
        public string AveragePerGuest { get; set; } = "0";
        public string CustomersCount { get; set; } = "0";
        public string GuestsCount { get; set; } = "0";
        public string OrdersCount { get; set; } = "0";
        public string Cost { get; set; } = "0";
        public string DiscountAmount { get; set; } = "0";
        public string VatAmount { get; set; } = "0";
        public string GrossSales { get; set; } = "0";
        public string NetQuantity { get; set; } = "0";
        public string NetSales { get; set; } = "0";
        public string NetSalesWithTax { get; set; } = "0";
        public string Profit { get; set; } = "0";
        public string RefundQuantity { get; set; } = "0";
        public string RefundAmount { get; set; } = "0";
        public string VoidQuantity { get; set; } = "0";
        public string VoidAmount { get; set; } = "0";
    }

    public partial class WorkDayDetailsModel
    {
        //public SalesReportByWorkDayModel WorkDaySummary { get; set; }
        public string BranchName { get; set; }
        public string PaymentId { get; set; }
        public string PaymentName { get; set; }
        public string PaymentSname { get; set; }
        public decimal Total { set; get; }
    }

    //public partial class WorkDayDetailsModel
    //{
    //    public string Id { get; set; }
    //    public string BranchId { get; set; }
    //    public string OrderHeaderId { set; get; }
    //    public List<OrderPayment> OrderPayments { get; set; }
    //}

    public partial class SalesReportByXModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string AverageOrder { get; set; } = "0";
        public string AveragePerGuest { get; set; } = "0";
        public string CustomersCount { get; set; } = "0";
        public string GuestsCount { get; set; } = "0";
        public string OrdersCount { get; set; } = "0";
        public string Cost { get; set; } = "0";
        public string DiscountAmount { get; set; } = "0";
        public string VatAmount { get; set; } = "0";
        public string GrossSales { get; set; } = "0";
        public string NetQuantity { get; set; } = "0";
        public string NetSales { get; set; } = "0";
        public string NetSalesWithTax { get; set; } = "0";
        public string Profit { get; set; } = "0";
        public string RefundQuantity { get; set; } = "0";
        public string RefundAmount { get; set; } = "0";
        public string VoidQuantity { get; set; } = "0";
        public string VoidAmount { get; set; } = "0";
    }

    public partial class SalesReportByVatGroupModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string TotalAmount { get; set; } = "0";
    }

    public partial class SalesReportByFeeModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string TotalVat { get; set; } = "0";
        public string TotalAmount { get; set; } = "0";
    }


    public partial class SalesReportByDiscountModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string TotalAmount { get; set; } = "0";
        public string OrderLevelAmount { get; set; } = "0";
        public string LineLevelAmount { get; set; } = "0";
    }
    public partial class SalesReportByPaymentModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string TotalAmount { get; set; } = "0";
        public string RefundAmount { get; set; } = "0";
        public string NetAmount { get; set; } = "0";
        public string PaymentTypeId { get; set; }
    }
    
    public partial class SalesReportByReceiptModel
    {
        public Guid Id { get; set; }
        public string Date { get; set; }
        public IdNameModel Branch { get; set; }
        public string Number { get; set; }
        public string InvoiceNumber { get; set; }
        public IdNameModel Type { get; set; }
        public string Time { get; set; }
        public string SubTotal { get; set; }
        public string Total { get; set; }
        public string TotalFees { get; set; }
        public string TotalVat { get; set; }
        public string FeesVat { get; set; }
        public IdNameModel DiningOption { get; set; }
        public IdNameModel OrderSource { get; set; }
        public IdNameModel CreatedBy { get; set; }
        public IdNameModel WaiterName { get; set; }
        public IdNameModel Cashair { get; set; }
        public CustomerModel Customer { get; set; }
        public ICollection<OrderPayment> OrderPayments { get; set; }
    }

    public partial class SalesReportTopModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public decimal Amount { get; set; } = 0;
    }
    public class DashboardModel
    {
        public decimal GrossSales { get; set; } = 0;
        public decimal RefundAmount { get; set; } = 0;
        public decimal RefundQuantity { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal NetSales { get; set; } = 0;
        public decimal NetSalesWithVat { get; set; } = 0;
        public decimal VatAmount { get; set; } = 0;
        public decimal NetQuantity { get; set; } = 0;
        public int OrdersCount { get; set; } = 0;
        public int GuestsCount { get; set; } = 0;
        public decimal AverageOrder { get; set; } = 0;
        public decimal AveragePerGuest { get; set; } = 0;
        public int CustomersCount { get; set; } = 0;
        public decimal VoidQuantity { get; set; } = 0;
        public decimal VoidAmount { get; set; } = 0;
        public List<SalesReportByDateModel> SalesByDate { get; set; }
        public List<SalesReportTopModel> TopItems { get; set; }
        public List<SalesReportTopModel> TopItemGroups { get; set; }
        public List<SalesReportTopModel> TopModifiers { get; set; }
        public List<SalesReportTopModel> TopEmployees { get; set; }
        public List<SalesReportTopModel> TopPaymentTypes { get; set; }
        public List<SalesReportTopModel> TopDiscounts { get; set; }
        public List<SalesReportTopModel> TopOrderSources { get; set; }
        public List<SalesReportTopModel> TopDiningOptions { get; set; }

    }

    public class HoursOfDay
    {
        public int Hour { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
    }

    public partial class ZatcaReport
    {
        public string Id { get; set; }
        public Guid UUID { get; set; }
        public int OrderNumber { get; set; }
        public bool IsReturn { get; set; }
        public IdNameModel Branch { get; set; }
        public string Total { get; set; }
        public CustomerModel Customer { get; set; }
        public ICollection<OrderPayment> OrderPayments { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string VatReportStatus { get; set; }
        public string ZatcaReportStatus { get; set; }
    }
}
