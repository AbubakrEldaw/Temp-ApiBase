using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class WorkDayShiftCashTransaction
    {
        public byte[] RowVersion { get; set; }
        public Guid Id { get; set; }
        public Guid? ServerId { get; set; }
        public Guid WorkDayShiftId { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }

        public virtual WorkDayShift WorkDayShift { get; set; }
    }
}
