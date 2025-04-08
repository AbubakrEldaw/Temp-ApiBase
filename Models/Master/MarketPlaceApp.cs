using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class MarketPlaceApp
    {
        public MarketPlaceApp()
        {
            CompanyAppSettings = new HashSet<CompanyAppSetting>();
            CompanyApps = new HashSet<CompanyApp>();
            MarketPlaceAppPlanAvailabilities = new HashSet<MarketPlaceAppPlanAvailability>();
            MarketPlaceAppRefreshTokens = new HashSet<MarketPlaceAppRefreshToken>();
            MarketPlaceCompanyBlacklists = new HashSet<MarketPlaceCompanyBlacklist>();
        }

        public string Id { get; set; }
        public string MarketPlaceCategoryId { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string Description { get; set; }
        public string Sdescription { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string ImagePath { get; set; }
        public string ImageName { get; set; }
        public string HtmlRaw { get; set; }
        public string ConnectionLevel { get; set; }
        public bool PosAccess { get; set; }
        public string Status { get; set; }
        public string JsonProp { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AppJsonProp { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }

        public virtual MarketPlaceCategory MarketPlaceCategory { get; set; }
        public virtual ICollection<CompanyAppSetting> CompanyAppSettings { get; set; }
        public virtual ICollection<CompanyApp> CompanyApps { get; set; }
        public virtual ICollection<MarketPlaceAppPlanAvailability> MarketPlaceAppPlanAvailabilities { get; set; }
        public virtual ICollection<MarketPlaceAppRefreshToken> MarketPlaceAppRefreshTokens { get; set; }
        public virtual ICollection<MarketPlaceCompanyBlacklist> MarketPlaceCompanyBlacklists { get; set; }
    }
}
