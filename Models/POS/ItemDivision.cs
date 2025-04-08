using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class ItemDivision
    {
        public ItemDivision()
        {
            ItemCategories = new HashSet<ItemCategory>();
            Items = new HashSet<Item>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string StatusId { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }

        public virtual Employee CreateByNavigation { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<ItemCategory> ItemCategories { get; set; }
        public virtual ICollection<Item> Items { get; set; }
    }
}
