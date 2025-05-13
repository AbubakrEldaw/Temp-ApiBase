using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class MarketPlaceAppRefreshToken
    {

		public bool IsExpired => DateTime.UtcNow >= Expires;
		public bool IsActive => Revoked == null && !IsExpired;
	}
}
