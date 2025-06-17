using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class BankTransfer
    {
        public Guid Id { get; set; }
        public byte[] FileData { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Guid TransactionId { get; set; }

        public virtual Transaction Transaction { get; set; } = null!;
    }
}
