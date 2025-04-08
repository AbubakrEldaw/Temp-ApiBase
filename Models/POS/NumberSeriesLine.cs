using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class NumberSeriesLine
    {
        public byte[] RowVersion { get; set; }
        public string NumberSeriesId { get; set; }
        public DateTime StartDate { get; set; }
        public string Alpha { get; set; }
        public string Start { get; set; }
        public int Increment { get; set; }
        public string LastUsed { get; set; }
        public string AlphaNumericLastUsed { get; set; }
        public string StatusId { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }

        public virtual Employee CreateByNavigation { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual NumberSeries NumberSeries { get; set; }
        public virtual Status Status { get; set; }
    }
}
