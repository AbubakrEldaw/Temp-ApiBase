using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class PosDeviceMenu
    {
        public string PosDeviceId { get; set; }
        public string MenuId { get; set; }

        public virtual Menu Menu { get; set; }
        public virtual PosDevice PosDevice { get; set; }
    }
}
