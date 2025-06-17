using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class MarketPlaceApp
    {
        public MarketPlaceApp()
        {
            CompanyAppSettings = new HashSet<CompanyAppSetting>();
            CompanyApps = new HashSet<CompanyApp>();
            MarketPlaceAppRefreshTokens = new HashSet<MarketPlaceAppRefreshToken>();
            MarketPlaceCompanyBlacklists = new HashSet<MarketPlaceCompanyBlacklist>();
            Plans = new HashSet<Plan>();
        }

        public string Id { get; set; } = null!;
        public string MarketPlaceCategoryId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Sname { get; set; } = null!;
        public string? Description { get; set; }
        public string? Sdescription { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? ImagePath { get; set; }
        public string? ImageName { get; set; }
        public string? HtmlRaw { get; set; }
        /// <summary>
        /// c = company, b = branch level
        /// </summary>
        public string ConnectionLevel { get; set; } = null!;
        public bool PosAccess { get; set; }
        /// <summary>
        /// a=active, i = inactive, c=commingsoon
        /// </summary>
        public string Status { get; set; } = null!;
        public string? JsonProp { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? AppJsonProp { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }

        public virtual MarketPlaceCategory MarketPlaceCategory { get; set; } = null!;
        public virtual ICollection<CompanyAppSetting> CompanyAppSettings { get; set; }
        public virtual ICollection<CompanyApp> CompanyApps { get; set; }
        public virtual ICollection<MarketPlaceAppRefreshToken> MarketPlaceAppRefreshTokens { get; set; }
        public virtual ICollection<MarketPlaceCompanyBlacklist> MarketPlaceCompanyBlacklists { get; set; }

        public virtual ICollection<Plan> Plans { get; set; }
    }
}
