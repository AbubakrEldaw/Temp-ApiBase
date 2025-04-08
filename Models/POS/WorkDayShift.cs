using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class WorkDayShift
    {
        public WorkDayShift()
        {
            OrderHeaders = new HashSet<OrderHeader>();
            WorkDayShiftCashTransactions = new HashSet<WorkDayShiftCashTransaction>();
        }

        public byte[] RowVersion { get; set; }
        public Guid Id { get; set; }
        public Guid? ServerId { get; set; }
        public Guid WorkDayId { get; set; }
        public string EmployeeId { get; set; }
        public decimal StartingCashAmount { get; set; }
        public decimal? ShiftCashAmount { get; set; }
        public decimal? CloseCashAmount { get; set; }
        public decimal? VarianceCashAmount { get; set; }
        public DateTime CreateAt { get; set; }
        public string ClosedBy { get; set; }
        public DateTime? ClosedAt { get; set; }

        public virtual Employee ClosedByNavigation { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual WorkDay WorkDay { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaders { get; set; }
        public virtual ICollection<WorkDayShiftCashTransaction> WorkDayShiftCashTransactions { get; set; }
    }
}
