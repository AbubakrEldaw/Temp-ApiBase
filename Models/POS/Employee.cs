using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class Employee
    {
        public Employee()
        {
            AggregatorCreateByNavigations = new HashSet<Aggregator>();
            AggregatorModifyByNavigations = new HashSet<Aggregator>();
            AreaCreateByNavigations = new HashSet<Area>();
            AreaModifyByNavigations = new HashSet<Area>();
            BranchCreateByNavigations = new HashSet<Branch>();
            BranchModifyByNavigations = new HashSet<Branch>();
            CallCenterDeviceCreateByNavigations = new HashSet<CallCenterDevice>();
            CallCenterDeviceModifyByNavigations = new HashSet<CallCenterDevice>();
            CustomerCreateByNavigations = new HashSet<Customer>();
            CustomerModifyByNavigations = new HashSet<Customer>();
            DiscountCreateByNavigations = new HashSet<Discount>();
            DiscountModifyByNavigations = new HashSet<Discount>();
            EmployeeGroupCreateByNavigations = new HashSet<EmployeeGroup>();
            EmployeeGroupModifyByNavigations = new HashSet<EmployeeGroup>();
            FeeCreateByNavigations = new HashSet<Fee>();
            FeeModifyByNavigations = new HashSet<Fee>();
            InverseCreateByNavigation = new HashSet<Employee>();
            InverseModifyByNavigation = new HashSet<Employee>();
            ItemCategoryCreateByNavigations = new HashSet<ItemCategory>();
            ItemCategoryModifyByNavigations = new HashSet<ItemCategory>();
            ItemCreateByNavigations = new HashSet<Item>();
            ItemDivisionCreateByNavigations = new HashSet<ItemDivision>();
            ItemDivisionModifyByNavigations = new HashSet<ItemDivision>();
            ItemGroupCreateByNavigations = new HashSet<ItemGroup>();
            ItemGroupModifyByNavigations = new HashSet<ItemGroup>();
            ItemModifyByNavigations = new HashSet<Item>();
            NumberSeriesLineCreateByNavigations = new HashSet<NumberSeriesLine>();
            NumberSeriesLineModifyByNavigations = new HashSet<NumberSeriesLine>();
            OrderHeaderCloseByNavigations = new HashSet<OrderHeader>();
            OrderHeaderCreateByNavigations = new HashSet<OrderHeader>();
            OrderHeaderModifyByNavigations = new HashSet<OrderHeader>();
            OrderHeaderPaidByNavigations = new HashSet<OrderHeader>();
            OrderHeaderVoidByNavigations = new HashSet<OrderHeader>();
            OrderHeaderWaiters = new HashSet<OrderHeader>();
            OrderItemCreateByNavigations = new HashSet<OrderItem>();
            OrderItemModifyByNavigations = new HashSet<OrderItem>();
            OrderItemVoidByNavigations = new HashSet<OrderItem>();
            PaymentCreateByNavigations = new HashSet<Payment>();
            PaymentModifyByNavigations = new HashSet<Payment>();
            PosDeviceCreateByNavigations = new HashSet<PosDevice>();
            PosDeviceModifyByNavigations = new HashSet<PosDevice>();
            PosPrinterCreateByNavigations = new HashSet<PosPrinter>();
            PosPrinterModifyByNavigations = new HashSet<PosPrinter>();
            VatGroupCreateByNavigations = new HashSet<VatGroup>();
            VatGroupModifyByNavigations = new HashSet<VatGroup>();
            WorkDayCloseByNavigations = new HashSet<WorkDay>();
            WorkDayOpenByNavigations = new HashSet<WorkDay>();
            WorkDayShiftClosedByNavigations = new HashSet<WorkDayShift>();
            WorkDayShiftEmployees = new HashSet<WorkDayShift>();
        }

        public byte[] RowVersion { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string BranchId { get; set; }
        public string EmployeeGroupId { get; set; }
        public Guid? AspnetUserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Pin { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool? DefaultLanguage { get; set; }
        public string PermRoleId { get; set; }
        public string StatusId { get; set; }
        public string ImagePath { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }
        public string Biometrics { get; set; }

        public virtual Branch Branch { get; set; }
        public virtual Employee CreateByNavigation { get; set; }
        public virtual EmployeeGroup EmployeeGroup { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual PermRole PermRole { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<Aggregator> AggregatorCreateByNavigations { get; set; }
        public virtual ICollection<Aggregator> AggregatorModifyByNavigations { get; set; }
        public virtual ICollection<Area> AreaCreateByNavigations { get; set; }
        public virtual ICollection<Area> AreaModifyByNavigations { get; set; }
        public virtual ICollection<Branch> BranchCreateByNavigations { get; set; }
        public virtual ICollection<Branch> BranchModifyByNavigations { get; set; }
        public virtual ICollection<CallCenterDevice> CallCenterDeviceCreateByNavigations { get; set; }
        public virtual ICollection<CallCenterDevice> CallCenterDeviceModifyByNavigations { get; set; }
        public virtual ICollection<Customer> CustomerCreateByNavigations { get; set; }
        public virtual ICollection<Customer> CustomerModifyByNavigations { get; set; }
        public virtual ICollection<Discount> DiscountCreateByNavigations { get; set; }
        public virtual ICollection<Discount> DiscountModifyByNavigations { get; set; }
        public virtual ICollection<EmployeeGroup> EmployeeGroupCreateByNavigations { get; set; }
        public virtual ICollection<EmployeeGroup> EmployeeGroupModifyByNavigations { get; set; }
        public virtual ICollection<Fee> FeeCreateByNavigations { get; set; }
        public virtual ICollection<Fee> FeeModifyByNavigations { get; set; }
        public virtual ICollection<Employee> InverseCreateByNavigation { get; set; }
        public virtual ICollection<Employee> InverseModifyByNavigation { get; set; }
        public virtual ICollection<ItemCategory> ItemCategoryCreateByNavigations { get; set; }
        public virtual ICollection<ItemCategory> ItemCategoryModifyByNavigations { get; set; }
        public virtual ICollection<Item> ItemCreateByNavigations { get; set; }
        public virtual ICollection<ItemDivision> ItemDivisionCreateByNavigations { get; set; }
        public virtual ICollection<ItemDivision> ItemDivisionModifyByNavigations { get; set; }
        public virtual ICollection<ItemGroup> ItemGroupCreateByNavigations { get; set; }
        public virtual ICollection<ItemGroup> ItemGroupModifyByNavigations { get; set; }
        public virtual ICollection<Item> ItemModifyByNavigations { get; set; }
        public virtual ICollection<NumberSeriesLine> NumberSeriesLineCreateByNavigations { get; set; }
        public virtual ICollection<NumberSeriesLine> NumberSeriesLineModifyByNavigations { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaderCloseByNavigations { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaderCreateByNavigations { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaderModifyByNavigations { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaderPaidByNavigations { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaderVoidByNavigations { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaderWaiters { get; set; }
        public virtual ICollection<OrderItem> OrderItemCreateByNavigations { get; set; }
        public virtual ICollection<OrderItem> OrderItemModifyByNavigations { get; set; }
        public virtual ICollection<OrderItem> OrderItemVoidByNavigations { get; set; }
        public virtual ICollection<Payment> PaymentCreateByNavigations { get; set; }
        public virtual ICollection<Payment> PaymentModifyByNavigations { get; set; }
        public virtual ICollection<PosDevice> PosDeviceCreateByNavigations { get; set; }
        public virtual ICollection<PosDevice> PosDeviceModifyByNavigations { get; set; }
        public virtual ICollection<PosPrinter> PosPrinterCreateByNavigations { get; set; }
        public virtual ICollection<PosPrinter> PosPrinterModifyByNavigations { get; set; }
        public virtual ICollection<VatGroup> VatGroupCreateByNavigations { get; set; }
        public virtual ICollection<VatGroup> VatGroupModifyByNavigations { get; set; }
        public virtual ICollection<WorkDay> WorkDayCloseByNavigations { get; set; }
        public virtual ICollection<WorkDay> WorkDayOpenByNavigations { get; set; }
        public virtual ICollection<WorkDayShift> WorkDayShiftClosedByNavigations { get; set; }
        public virtual ICollection<WorkDayShift> WorkDayShiftEmployees { get; set; }
    }
}
