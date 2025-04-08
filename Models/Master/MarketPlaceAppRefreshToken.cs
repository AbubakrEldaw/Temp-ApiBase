using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class MarketPlaceAppRefreshToken
    {
        public Guid Id { get; set; }
        public string MarketPlaceAppId { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; }

        public virtual MarketPlaceApp MarketPlaceApp { get; set; }
    }
}
