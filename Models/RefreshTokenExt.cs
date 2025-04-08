using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class RefreshToken
    {

		public bool IsExpired => DateTime.UtcNow >= Expires;
		public bool IsActive => Revoked == null && !IsExpired;
	}
}
