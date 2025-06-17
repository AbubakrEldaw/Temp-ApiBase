using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class Account
    {
        public Account()
        {
            Companies = new HashSet<Company>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Sname { get; set; }

        public virtual ICollection<Company> Companies { get; set; }
    }
}
