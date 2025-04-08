using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class WorkDay
    {
        public WorkDay()
        {
            OrderHeaders = new HashSet<OrderHeader>();
            WorkDayShifts = new HashSet<WorkDayShift>();
        }

        public byte[] RowVersion { get; set; }
        public Guid Id { get; set; }
        public Guid? ServerId { get; set; }
        public string BranchId { get; set; }
        public DateTime Date { get; set; }
        public bool Active { get; set; }
        public bool WorkShiftOn { get; set; }
        public string OpenBy { get; set; }
        public DateTime OpenAt { get; set; }
        public string CloseBy { get; set; }
        public DateTime? CloseAt { get; set; }
        public bool OperationIsRunning { get; set; }
        public string OperationData { get; set; }

        public virtual Branch Branch { get; set; }
        public virtual Employee CloseByNavigation { get; set; }
        public virtual Employee OpenByNavigation { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaders { get; set; }
        public virtual ICollection<WorkDayShift> WorkDayShifts { get; set; }
    }
}
