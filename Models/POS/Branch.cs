using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class Branch
    {
        public Branch()
        {
            ApiOrders = new HashSet<ApiOrder>();
            Areas = new HashSet<Area>();
            Employees = new HashSet<Employee>();
            IntegrationBranchLevels = new HashSet<IntegrationBranchLevel>();
            OrderHeaders = new HashSet<OrderHeader>();
            PosDevices = new HashSet<PosDevice>();
            WorkDays = new HashSet<WorkDay>();
        }

        public byte[] RowVersion { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string VatRegNo { get; set; }
        public string FranshizeCompany { get; set; }
        public string VatGroupId { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }
        public string DefaultMenuId { get; set; }
        public string Crn { get; set; }
        public string Tin { get; set; }
        public string RegistrationName { get; set; }
        public string StreetName { get; set; }
        public string BuildingNumber { get; set; }
        public string PlotIdentification { get; set; }
        public string CitySubdivisionName { get; set; }
        public string CityName { get; set; }
        public string PostalZone { get; set; }
        public string CountrySubentity { get; set; }
        public string Country { get; set; }
        public virtual ReceiptSetting ReceiptSetting { get; set; }

        public virtual Employee CreateByNavigation { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual VatGroup VatGroup { get; set; }
        public virtual ICollection<ApiOrder> ApiOrders { get; set; }
        public virtual ICollection<Area> Areas { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<IntegrationBranchLevel> IntegrationBranchLevels { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaders { get; set; }
        public virtual ICollection<PosDevice> PosDevices { get; set; }
        public virtual ICollection<WorkDay> WorkDays { get; set; }
    }
}
