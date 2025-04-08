using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class Menu
    {
        public Menu()
        {
            MenuGroups = new HashSet<MenuGroup>();
            PosDeviceMenus = new HashSet<PosDeviceMenu>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string StatusId { get; set; }
        public string ImagePath { get; set; }
        public string ImageName { get; set; }
        public string Description { get; set; }

        public virtual Status Status { get; set; }
        public virtual ICollection<MenuGroup> MenuGroups { get; set; }
        public virtual ICollection<PosDeviceMenu> PosDeviceMenus { get; set; }
    }
}
