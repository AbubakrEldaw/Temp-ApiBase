using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class Status
    {
        public Status()
        {
            Aggregators = new HashSet<Aggregator>();
            Areas = new HashSet<Area>();
            CallCenterDevices = new HashSet<CallCenterDevice>();
            Discounts = new HashSet<Discount>();
            Employees = new HashSet<Employee>();
            Fees = new HashSet<Fee>();
            ItemCategories = new HashSet<ItemCategory>();
            ItemDivisions = new HashSet<ItemDivision>();
            ItemGroups = new HashSet<ItemGroup>();
            Items = new HashSet<Item>();
            MenuGroups = new HashSet<MenuGroup>();
            Menus = new HashSet<Menu>();
            NumberSeriesLines = new HashSet<NumberSeriesLine>();
            OrderSources = new HashSet<OrderSource>();
            Payments = new HashSet<Payment>();
            PosDevices = new HashSet<PosDevice>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual ICollection<Aggregator> Aggregators { get; set; }
        public virtual ICollection<Area> Areas { get; set; }
        public virtual ICollection<CallCenterDevice> CallCenterDevices { get; set; }
        public virtual ICollection<Discount> Discounts { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<Fee> Fees { get; set; }
        public virtual ICollection<ItemCategory> ItemCategories { get; set; }
        public virtual ICollection<ItemDivision> ItemDivisions { get; set; }
        public virtual ICollection<ItemGroup> ItemGroups { get; set; }
        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<MenuGroup> MenuGroups { get; set; }
        public virtual ICollection<Menu> Menus { get; set; }
        public virtual ICollection<NumberSeriesLine> NumberSeriesLines { get; set; }
        public virtual ICollection<OrderSource> OrderSources { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<PosDevice> PosDevices { get; set; }
    }
}
