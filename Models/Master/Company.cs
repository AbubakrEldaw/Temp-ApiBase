using System;
using System.Collections.Generic;

namespace APIBase.Models.Master
{
    public partial class Company
    {
        public Company()
        {
            CompanyAppSettings = new HashSet<CompanyAppSetting>();
            CompanyApps = new HashSet<CompanyApp>();
            CompanyBranches = new HashSet<CompanyBranch>();
            CompanyFeatures = new HashSet<CompanyFeature>();
            CompanyLicenses = new HashSet<CompanyLicense>();
            CompanySavedCards = new HashSet<CompanySavedCard>();
            MarketPlaceCompanyBlacklists = new HashSet<MarketPlaceCompanyBlacklist>();
            PaymentSessions = new HashSet<PaymentSession>();
            SystemLogs = new HashSet<SystemLog>();
            Transactions = new HashSet<Transaction>();
            Users = new HashSet<User>();
        }

        public string Id { get; set; } = null!;
        public string? AccountId { get; set; }
        public string Name { get; set; } = null!;
        public string? Sname { get; set; }
        public string? Address { get; set; }
        public string? CrNumber { get; set; }
        public string? Tin { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Country { get; set; }

        public virtual Account? Account { get; set; }
        public virtual CompanyPlan CompanyPlan { get; set; } = null!;
        public virtual ConnStr ConnStr { get; set; } = null!;
        public virtual ICollection<CompanyAppSetting> CompanyAppSettings { get; set; }
        public virtual ICollection<CompanyApp> CompanyApps { get; set; }
        public virtual ICollection<CompanyBranch> CompanyBranches { get; set; }
        public virtual ICollection<CompanyFeature> CompanyFeatures { get; set; }
        public virtual ICollection<CompanyLicense> CompanyLicenses { get; set; }
        public virtual ICollection<CompanySavedCard> CompanySavedCards { get; set; }
        public virtual ICollection<MarketPlaceCompanyBlacklist> MarketPlaceCompanyBlacklists { get; set; }
        public virtual ICollection<PaymentSession> PaymentSessions { get; set; }
        public virtual ICollection<SystemLog> SystemLogs { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
