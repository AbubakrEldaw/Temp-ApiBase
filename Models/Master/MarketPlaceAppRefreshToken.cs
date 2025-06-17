using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class MarketPlaceAppRefreshToken
    {
        public Guid Id { get; set; }
        public string MarketPlaceAppId { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; } = null!;
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }

        public virtual MarketPlaceApp MarketPlaceApp { get; set; } = null!;
    }
}
