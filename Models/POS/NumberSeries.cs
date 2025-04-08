using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class NumberSeries
    {
        public NumberSeries()
        {
            NumberSeriesLines = new HashSet<NumberSeriesLine>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }

        public virtual ICollection<NumberSeriesLine> NumberSeriesLines { get; set; }
    }
}
