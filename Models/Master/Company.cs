using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class Company
    {
        public Company()
        {
            CompanyAppSettings = new HashSet<CompanyAppSetting>();
            CompanyApps = new HashSet<CompanyApp>();
            CompanyBranchSubescriptions = new HashSet<CompanyBranchSubescription>();
            CompanyBranches = new HashSet<CompanyBranch>();
            CompanyFeatures = new HashSet<CompanyFeature>();
            CompanyLicenses = new HashSet<CompanyLicense>();
            CompanySavedCards = new HashSet<CompanySavedCard>();
            MarketPlaceCompanyBlacklists = new HashSet<MarketPlaceCompanyBlacklist>();
            PaymentSessions = new HashSet<PaymentSession>();
            SystemLogs = new HashSet<SystemLog>();
            Transactions = new HashSet<Transaction>();
        }

        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string CrNumber { get; set; }
        public string Tin { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }

        public virtual Account Account { get; set; }
        public virtual CompanyPlan CompanyPlan { get; set; }
        public virtual ConnStr ConnStr { get; set; }
        public virtual ICollection<CompanyAppSetting> CompanyAppSettings { get; set; }
        public virtual ICollection<CompanyApp> CompanyApps { get; set; }
        public virtual ICollection<CompanyBranchSubescription> CompanyBranchSubescriptions { get; set; }
        public virtual ICollection<CompanyBranch> CompanyBranches { get; set; }
        public virtual ICollection<CompanyFeature> CompanyFeatures { get; set; }
        public virtual ICollection<CompanyLicense> CompanyLicenses { get; set; }
        public virtual ICollection<CompanySavedCard> CompanySavedCards { get; set; }
        public virtual ICollection<MarketPlaceCompanyBlacklist> MarketPlaceCompanyBlacklists { get; set; }
        public virtual ICollection<PaymentSession> PaymentSessions { get; set; }
        public virtual ICollection<SystemLog> SystemLogs { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
