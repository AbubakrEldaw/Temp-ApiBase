using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace APIBase.Models.Master
{
    public partial class forkpos_masterContext : DbContext
    {
        public forkpos_masterContext()
        {
        }

        public forkpos_masterContext(DbContextOptions<forkpos_masterContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<CompanyApp> CompanyApps { get; set; }
        public virtual DbSet<CompanyAppSetting> CompanyAppSettings { get; set; }
        public virtual DbSet<CompanyBranch> CompanyBranches { get; set; }
        public virtual DbSet<CompanyBranchIntegration> CompanyBranchIntegrations { get; set; }
        public virtual DbSet<CompanyBranchSubescription> CompanyBranchSubescriptions { get; set; }
        public virtual DbSet<CompanyBranchSubescriptionLicense> CompanyBranchSubescriptionLicenses { get; set; }
        public virtual DbSet<CompanyFeature> CompanyFeatures { get; set; }
        public virtual DbSet<CompanyLicense> CompanyLicenses { get; set; }
        public virtual DbSet<CompanyPlan> CompanyPlans { get; set; }
        public virtual DbSet<CompanySavedCard> CompanySavedCards { get; set; }
        public virtual DbSet<ConnStr> ConnStrs { get; set; }
        public virtual DbSet<DisplayFeature> DisplayFeatures { get; set; }
        public virtual DbSet<DisplayFeaturePlan> DisplayFeaturePlans { get; set; }
        public virtual DbSet<Feature> Features { get; set; }
        public virtual DbSet<LicenseType> LicenseTypes { get; set; }
        public virtual DbSet<MarketPlaceApp> MarketPlaceApps { get; set; }
        public virtual DbSet<MarketPlaceAppPlanAvailability> MarketPlaceAppPlanAvailabilities { get; set; }
        public virtual DbSet<MarketPlaceAppRefreshToken> MarketPlaceAppRefreshTokens { get; set; }
        public virtual DbSet<MarketPlaceCategory> MarketPlaceCategories { get; set; }
        public virtual DbSet<MarketPlaceCompanyBlacklist> MarketPlaceCompanyBlacklists { get; set; }
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
        public virtual DbSet<PaymentSession> PaymentSessions { get; set; }
        public virtual DbSet<Plan> Plans { get; set; }
        public virtual DbSet<PlanFeature> PlanFeatures { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<SubscriptionCategory> SubscriptionCategories { get; set; }
        public virtual DbSet<SystemLog> SystemLogs { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<TransactionLine> TransactionLines { get; set; }
        public virtual DbSet<TransactionPayment> TransactionPayments { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserLink> UserLinks { get; set; }

   
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("account");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("company");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.AccountId)
                    .HasMaxLength(20)
                    .HasColumnName("account_id");

                entity.Property(e => e.Address)
                    .HasMaxLength(50)
                    .HasColumnName("address");

                entity.Property(e => e.Country)
                    .HasMaxLength(2)
                    .HasColumnName("country")
                    .IsFixedLength(true)
                    .HasComment("Uses iso-2 country code standard");

                entity.Property(e => e.CrNumber)
                    .HasMaxLength(50)
                    .HasColumnName("cr_number");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .HasColumnName("email")
                    .IsFixedLength(true);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .HasColumnName("phone_number");

                entity.Property(e => e.Tin)
                    .HasMaxLength(50)
                    .HasColumnName("tin");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Companies)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_company_account");
            });

            modelBuilder.Entity<CompanyApp>(entity =>
            {
                entity.HasKey(e => new { e.CompanyId, e.MarketPlaceAppId })
                    .HasName("PK_company_app_1");

                entity.ToTable("company_app", "lic");

                entity.Property(e => e.CompanyId)
                    .HasMaxLength(20)
                    .HasColumnName("company_id");

                entity.Property(e => e.MarketPlaceAppId)
                    .HasMaxLength(20)
                    .HasColumnName("market_place_app_id");

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("amount");

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .HasColumnName("end_date");

                entity.Property(e => e.InTrial).HasColumnName("in_trial");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.JsonProp).HasColumnName("json_prop");

                entity.Property(e => e.StartDate)
                    .HasColumnType("date")
                    .HasColumnName("start_date");

                entity.Property(e => e.SubscriptionCategoryId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("subscription_category_id");

                entity.Property(e => e.TrialEndDate)
                    .HasColumnType("date")
                    .HasColumnName("trial_end_date");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.CompanyApps)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_company_app_company");

                entity.HasOne(d => d.MarketPlaceApp)
                    .WithMany(p => p.CompanyApps)
                    .HasForeignKey(d => d.MarketPlaceAppId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_company_app_market_place_app");

                entity.HasOne(d => d.SubscriptionCategory)
                    .WithMany(p => p.CompanyApps)
                    .HasForeignKey(d => d.SubscriptionCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_company_app_subscription_category");
            });

            modelBuilder.Entity<CompanyAppSetting>(entity =>
            {
                entity.HasKey(e => new { e.CompanyId, e.MarketPlaceAppId })
                    .HasName("PK_company_app");

                entity.ToTable("company_app_setting", "mp");

                entity.Property(e => e.CompanyId)
                    .HasMaxLength(20)
                    .HasColumnName("company_id");

                entity.Property(e => e.MarketPlaceAppId)
                    .HasMaxLength(20)
                    .HasColumnName("market_place_app_id");

                entity.Property(e => e.JsonProp).HasColumnName("json_prop");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.CompanyAppSettings)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_company_app_company");

                entity.HasOne(d => d.MarketPlaceApp)
                    .WithMany(p => p.CompanyAppSettings)
                    .HasForeignKey(d => d.MarketPlaceAppId)
                    .HasConstraintName("FK_company_app_market_place_app");
            });

            modelBuilder.Entity<CompanyBranch>(entity =>
            {
                entity.HasKey(e => new { e.CompanyId, e.PosBranchId });

                entity.ToTable("company_branch");

                entity.HasIndex(e => e.GlobalBranchId, "IX_company_branch")
                    .IsUnique();

                entity.Property(e => e.CompanyId)
                    .HasMaxLength(20)
                    .HasColumnName("company_id");

                entity.Property(e => e.PosBranchId)
                    .HasMaxLength(20)
                    .HasColumnName("pos_branch_id");

                entity.Property(e => e.GlobalBranchId)
                    .HasColumnName("global_branch_id")
                    .HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.CompanyBranches)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_company_branch_company");
            });

            modelBuilder.Entity<CompanyBranchIntegration>(entity =>
            {
                entity.HasKey(e => e.GlobalBranchId);

                entity.ToTable("company_branch_integration");

                entity.Property(e => e.GlobalBranchId)
                    .ValueGeneratedNever()
                    .HasColumnName("global_branch_id");

                entity.Property(e => e.FoodizoneBranchId).HasColumnName("foodizone_branch_id");

                entity.HasOne(d => d.GlobalBranch)
                    .WithOne(p => p.CompanyBranchIntegration)
                    .HasPrincipalKey<CompanyBranch>(p => p.GlobalBranchId)
                    .HasForeignKey<CompanyBranchIntegration>(d => d.GlobalBranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_company_branch_integration_company_branch");
            });

            modelBuilder.Entity<CompanyBranchSubescription>(entity =>
            {
                entity.HasKey(e => new { e.CompanyId, e.BranchId });

                entity.ToTable("company_branch_subescription", "lic");

                entity.Property(e => e.CompanyId)
                    .HasMaxLength(20)
                    .HasColumnName("company_id");

                entity.Property(e => e.BranchId)
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .HasColumnName("end_date");

                entity.Property(e => e.GracePeriodInDays).HasColumnName("grace_period_in_days");

                entity.Property(e => e.StartDate)
                    .HasColumnType("date")
                    .HasColumnName("start_date");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.CompanyBranchSubescriptions)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_company_branch_subescription_company");

                entity.HasOne(d => d.CompanyBranch)
                    .WithOne(p => p.CompanyBranchSubescription)
                    .HasForeignKey<CompanyBranchSubescription>(d => new { d.CompanyId, d.BranchId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_company_branch_subescription_company_branch");
            });

            modelBuilder.Entity<CompanyBranchSubescriptionLicense>(entity =>
            {
                entity.HasKey(e => new { e.CompanyId, e.BranchId, e.LicenseTypeId });

                entity.ToTable("company_branch_subescription_license", "lic");

                entity.Property(e => e.CompanyId)
                    .HasMaxLength(20)
                    .HasColumnName("company_id");

                entity.Property(e => e.BranchId)
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.LicenseTypeId)
                    .HasMaxLength(20)
                    .HasColumnName("license_type_id");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.HasOne(d => d.LicenseType)
                    .WithMany(p => p.CompanyBranchSubescriptionLicenses)
                    .HasForeignKey(d => d.LicenseTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_company_branch_subescription_license_license_type");

                entity.HasOne(d => d.CompanyBranchSubescription)
                    .WithMany(p => p.CompanyBranchSubescriptionLicenses)
                    .HasForeignKey(d => new { d.CompanyId, d.BranchId })
                    .HasConstraintName("FK_company_branch_subescription_license_company_branch_subescription");
            });

            modelBuilder.Entity<CompanyFeature>(entity =>
            {
                entity.HasKey(e => new { e.CompanyId, e.FeatureId });

                entity.ToTable("company_feature", "lic");

                entity.Property(e => e.CompanyId)
                    .HasMaxLength(20)
                    .HasColumnName("company_id");

                entity.Property(e => e.FeatureId)
                    .HasMaxLength(20)
                    .HasColumnName("feature_id");

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("amount");

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .HasColumnName("end_date");

                entity.Property(e => e.InTrial).HasColumnName("in_trial");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.StartDate)
                    .HasColumnType("date")
                    .HasColumnName("start_date");

                entity.Property(e => e.SubscriptionCategoryId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("subscription_category_id");

                entity.Property(e => e.TrialEndDate)
                    .HasColumnType("date")
                    .HasColumnName("trial_end_date");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.CompanyFeatures)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_company_feature_company");

                entity.HasOne(d => d.Feature)
                    .WithMany(p => p.CompanyFeatures)
                    .HasForeignKey(d => d.FeatureId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_company_feature_feature");

                entity.HasOne(d => d.SubscriptionCategory)
                    .WithMany(p => p.CompanyFeatures)
                    .HasForeignKey(d => d.SubscriptionCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_company_feature_subscription_category");
            });

            modelBuilder.Entity<CompanyLicense>(entity =>
            {
                entity.ToTable("company_license", "lic");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.CompanyId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("company_id");

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .HasColumnName("end_date");

                entity.Property(e => e.InTrial).HasColumnName("in_trial");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LicenseTypeId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("license_type_id");

                entity.Property(e => e.LinkId)
                    .HasMaxLength(50)
                    .HasColumnName("link_id");

                entity.Property(e => e.ParentId).HasColumnName("parent_id");

                entity.Property(e => e.StartDate)
                    .HasColumnType("date")
                    .HasColumnName("start_date");

                entity.Property(e => e.SubscriptionCategoryId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("subscription_category_id");

                entity.Property(e => e.TransactionLineId).HasColumnName("transaction_line_id");

                entity.Property(e => e.TrialEndDate)
                    .HasColumnType("date")
                    .HasColumnName("trial_end_date");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.CompanyLicenses)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_company_license_company");

                entity.HasOne(d => d.LicenseType)
                    .WithMany(p => p.CompanyLicenses)
                    .HasForeignKey(d => d.LicenseTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_company_license_license_type");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_company_license_company_license");

                entity.HasOne(d => d.SubscriptionCategory)
                    .WithMany(p => p.CompanyLicenses)
                    .HasForeignKey(d => d.SubscriptionCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_company_license_subscription_category");

                entity.HasOne(d => d.TransactionLine)
                    .WithMany(p => p.CompanyLicenses)
                    .HasForeignKey(d => d.TransactionLineId)
                    .HasConstraintName("FK_company_license_transaction_line");
            });

            modelBuilder.Entity<CompanyPlan>(entity =>
            {
                entity.HasKey(e => e.CompanyId);

                entity.ToTable("company_plan", "lic");

                entity.Property(e => e.CompanyId)
                    .HasMaxLength(20)
                    .HasColumnName("company_id");

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .HasColumnName("end_date");

                entity.Property(e => e.GracePeriod)
                    .HasColumnName("grace_period")
                    .HasDefaultValueSql("((90))");

                entity.Property(e => e.InTrial).HasColumnName("in_trial");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.PlanId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("plan_id");

                entity.Property(e => e.RenewalAttempts).HasColumnName("renewal_attempts");

                entity.Property(e => e.StartDate)
                    .HasColumnType("date")
                    .HasColumnName("start_date");

                entity.Property(e => e.TrialEndDate)
                    .HasColumnType("date")
                    .HasColumnName("trial_end_date");

                entity.HasOne(d => d.Company)
                    .WithOne(p => p.CompanyPlan)
                    .HasForeignKey<CompanyPlan>(d => d.CompanyId)
                    .HasConstraintName("FK_company_plan_company");

                entity.HasOne(d => d.Plan)
                    .WithMany(p => p.CompanyPlans)
                    .HasForeignKey(d => d.PlanId)
                    .HasConstraintName("FK_company_plan_plan");
            });

            modelBuilder.Entity<CompanySavedCard>(entity =>
            {
                entity.ToTable("company_saved_card", "trn");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Alias).HasColumnName("alias");

                entity.Property(e => e.Brand)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("brand");

                entity.Property(e => e.CardNumberMasked)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("card_number_masked");

                entity.Property(e => e.CompanyId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("company_id");

                entity.Property(e => e.DisplayToken).HasColumnName("display_token");

                entity.Property(e => e.ExpiryMonth)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasColumnName("expiry_month");

                entity.Property(e => e.ExpiryYear)
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .HasColumnName("expiry_year");

                entity.Property(e => e.IsDefault).HasColumnName("is_default");

                entity.Property(e => e.SubscriptionToken).HasColumnName("subscription_token");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.CompanySavedCards)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_company_saved_card_company");
            });

            modelBuilder.Entity<ConnStr>(entity =>
            {
                entity.HasKey(e => e.CompanyId);

                entity.HasIndex(e => e.CompanyId, "IX_ConnStrs_CompanyId")
                    .IsUnique();

                entity.HasIndex(e => e.DatabaseName, "IX_ConnStrs_DatabaseName")
                    .IsUnique();

                entity.Property(e => e.CompanyId).HasMaxLength(20);

                entity.Property(e => e.DatabaseName).IsRequired();

                entity.Property(e => e.InstanceName).IsRequired();

                entity.Property(e => e.Password).IsRequired();

                entity.Property(e => e.ServerName).IsRequired();

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('sa')");

                entity.HasOne(d => d.Company)
                    .WithOne(p => p.ConnStr)
                    .HasForeignKey<ConnStr>(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ConnStrs_company");
            });

            modelBuilder.Entity<DisplayFeature>(entity =>
            {
                entity.ToTable("display_feature");

                entity.Property(e => e.Id)
                    .HasMaxLength(30)
                    .HasColumnName("id");

                entity.Property(e => e.ImageUrl).HasColumnName("image_url");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.OrderIndex).HasColumnName("order_index");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("sname");
            });

            modelBuilder.Entity<DisplayFeaturePlan>(entity =>
            {
                entity.HasKey(e => new { e.DisplayFeatureId, e.PlanId })
                    .HasName("PK_display_plan_feature");

                entity.ToTable("display_feature_plan");

                entity.Property(e => e.DisplayFeatureId)
                    .HasMaxLength(30)
                    .HasColumnName("display_feature_id");

                entity.Property(e => e.PlanId)
                    .HasMaxLength(20)
                    .HasColumnName("plan_id");

                entity.HasOne(d => d.DisplayFeature)
                    .WithMany(p => p.DisplayFeaturePlans)
                    .HasForeignKey(d => d.DisplayFeatureId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_display_feature_plan_display_feature");

                entity.HasOne(d => d.Plan)
                    .WithMany(p => p.DisplayFeaturePlans)
                    .HasForeignKey(d => d.PlanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_display_feature_plan_plan");
            });

            modelBuilder.Entity<Feature>(entity =>
            {
                entity.ToTable("feature", "lic");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.IsPurchasable)
                    .IsRequired()
                    .HasColumnName("is_purchasable")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.MonthlyPrice)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("monthly_price");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sdescription).HasColumnName("sdescription");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.YearlyPrice)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("yearly_price");
            });

            modelBuilder.Entity<LicenseType>(entity =>
            {
                entity.ToTable("license_type", "lic");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.MonthlyPrice)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("monthly_price");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.YearlyPrice)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("yearly_price");
            });

            modelBuilder.Entity<MarketPlaceApp>(entity =>
            {
                entity.ToTable("market_place_app", "mp");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.AppJsonProp).HasColumnName("app_json_prop");

                entity.Property(e => e.ClientId)
                    .HasMaxLength(200)
                    .HasColumnName("client_id");

                entity.Property(e => e.ClientSecret)
                    .HasMaxLength(200)
                    .HasColumnName("client_secret");

                entity.Property(e => e.ConnectionLevel)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("connection_level")
                    .IsFixedLength(true)
                    .HasComment("c = company, b = branch level");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .HasColumnName("email");

                entity.Property(e => e.HtmlRaw).HasColumnName("html_raw");

                entity.Property(e => e.ImageName)
                    .HasMaxLength(50)
                    .HasColumnName("image_name");

                entity.Property(e => e.ImagePath).HasColumnName("image_path");

                entity.Property(e => e.JsonProp).HasColumnName("json_prop");

                entity.Property(e => e.MarketPlaceCategoryId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("market_place_category_id");

                entity.Property(e => e.MonthlyPrice)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("monthly_price");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .HasColumnName("phone");

                entity.Property(e => e.PosAccess).HasColumnName("pos_access");

                entity.Property(e => e.Sdescription).HasColumnName("sdescription");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("status")
                    .HasDefaultValueSql("('i')")
                    .IsFixedLength(true)
                    .HasComment("a=active, i = inactive, c=commingsoon");

                entity.Property(e => e.Website)
                    .HasMaxLength(50)
                    .HasColumnName("website");

                entity.Property(e => e.YearlyPrice)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("yearly_price");

                entity.HasOne(d => d.MarketPlaceCategory)
                    .WithMany(p => p.MarketPlaceApps)
                    .HasForeignKey(d => d.MarketPlaceCategoryId)
                    .HasConstraintName("FK_market_place_app_market_place_category");
            });

            modelBuilder.Entity<MarketPlaceAppPlanAvailability>(entity =>
            {
                entity.HasKey(e => new { e.MarketPlaceAppId, e.PlanId });

                entity.ToTable("market_place_app_plan_availability", "mp");

                entity.Property(e => e.MarketPlaceAppId)
                    .HasMaxLength(20)
                    .HasColumnName("market_place_app_id");

                entity.Property(e => e.PlanId)
                    .HasMaxLength(20)
                    .HasColumnName("plan_id");

                entity.HasOne(d => d.MarketPlaceApp)
                    .WithMany(p => p.MarketPlaceAppPlanAvailabilities)
                    .HasForeignKey(d => d.MarketPlaceAppId)
                    .HasConstraintName("FK_market_place_app_plan_availability_market_place_app");

                entity.HasOne(d => d.Plan)
                    .WithMany(p => p.MarketPlaceAppPlanAvailabilities)
                    .HasForeignKey(d => d.PlanId)
                    .HasConstraintName("FK_market_place_app_plan_availability_plan");
            });

            modelBuilder.Entity<MarketPlaceAppRefreshToken>(entity =>
            {
                entity.ToTable("market_place_app_refresh_token", "mp");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedByIp).IsRequired();

                entity.Property(e => e.MarketPlaceAppId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("market_place_app_id");

                entity.Property(e => e.Token).IsRequired();

                entity.HasOne(d => d.MarketPlaceApp)
                    .WithMany(p => p.MarketPlaceAppRefreshTokens)
                    .HasForeignKey(d => d.MarketPlaceAppId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_market_place_app_refresh_token_market_place_app");
            });

            modelBuilder.Entity<MarketPlaceCategory>(entity =>
            {
                entity.ToTable("market_place_category", "mp");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<MarketPlaceCompanyBlacklist>(entity =>
            {
                entity.HasKey(e => new { e.CompanyId, e.MarketPlaceAppId });

                entity.ToTable("market_place_company_blacklist", "mp");

                entity.Property(e => e.CompanyId)
                    .HasMaxLength(20)
                    .HasColumnName("company_id");

                entity.Property(e => e.MarketPlaceAppId)
                    .HasMaxLength(20)
                    .HasColumnName("market_place_app_id");

                entity.Property(e => e.Reason)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("reason");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.MarketPlaceCompanyBlacklists)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_market_place_company_blacklist_company");

                entity.HasOne(d => d.MarketPlaceApp)
                    .WithMany(p => p.MarketPlaceCompanyBlacklists)
                    .HasForeignKey(d => d.MarketPlaceAppId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_market_place_company_blacklist_market_place_app");
            });

            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.ToTable("payment_method", "trn");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Fees)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fees");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sdescription).HasColumnName("sdescription");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("status");
            });

            modelBuilder.Entity<PaymentSession>(entity =>
            {
                entity.ToTable("payment_session", "trn");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CompanyId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("company_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.PaymentJson)
                    .IsRequired()
                    .HasColumnName("payment_json");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("status");

                entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

                entity.Property(e => e.TransactionLinesJson)
                    .IsRequired()
                    .HasColumnName("transaction_lines_json");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.PaymentSessions)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_payment_session_company");

                entity.HasOne(d => d.Transaction)
                    .WithMany(p => p.PaymentSessions)
                    .HasForeignKey(d => d.TransactionId)
                    .HasConstraintName("FK_payment_session_transaction");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PaymentSessions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_payment_session_Users");
            });

            modelBuilder.Entity<Plan>(entity =>
            {
                entity.ToTable("plan", "lic");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.MonthlyPrice)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("monthly_price");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sdescription).HasColumnName("sdescription");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.YearlyPrice)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("yearly_price");
            });

            modelBuilder.Entity<PlanFeature>(entity =>
            {
                entity.HasKey(e => new { e.PlanId, e.FeatureId });

                entity.ToTable("plan_feature", "lic");

                entity.Property(e => e.PlanId)
                    .HasMaxLength(20)
                    .HasColumnName("plan_id");

                entity.Property(e => e.FeatureId)
                    .HasMaxLength(20)
                    .HasColumnName("feature_id");

                entity.HasOne(d => d.Feature)
                    .WithMany(p => p.PlanFeatures)
                    .HasForeignKey(d => d.FeatureId)
                    .HasConstraintName("FK_plan_feature_feature");

                entity.HasOne(d => d.Plan)
                    .WithMany(p => p.PlanFeatures)
                    .HasForeignKey(d => d.PlanId)
                    .HasConstraintName("FK_plan_feature_plan");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshToken");

                entity.HasIndex(e => e.UserId, "IX_RefreshToken_UserId");

                entity.Property(e => e.CreatedByIp).IsRequired();

                entity.Property(e => e.Token).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.RefreshTokens)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<SubscriptionCategory>(entity =>
            {
                entity.ToTable("subscription_category", "lic");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname");
            });

            modelBuilder.Entity<SystemLog>(entity =>
            {
                entity.ToTable("system_log");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("action");

                entity.Property(e => e.CompanyId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("company_id");

                entity.Property(e => e.ErrorMessage)
                    .IsRequired()
                    .HasColumnName("error_message");

                entity.Property(e => e.Notes).HasColumnName("notes");

                entity.Property(e => e.RequestJson).HasColumnName("request_json");

                entity.Property(e => e.ResponseJson).HasColumnName("response_json");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status");

                entity.Property(e => e.Time)
                    .HasColumnType("datetime")
                    .HasColumnName("time")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("type");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.SystemLogs)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_system_log_company");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transaction", "trn");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.CompanyId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("company_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.ModifiedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modified_at");

                entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");

                entity.Property(e => e.PaidAt)
                    .HasColumnType("datetime")
                    .HasColumnName("paid_at");

                entity.Property(e => e.PaidBy).HasColumnName("paid_by");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("status");

                entity.Property(e => e.SubTotal)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("sub_total");

                entity.Property(e => e.Total)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("total");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("type")
                    .HasDefaultValueSql("('Purchase')");

                entity.Property(e => e.VatAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("vat_amount");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_transaction_company");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.TransactionCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_transaction_Users");

                entity.HasOne(d => d.ModifiedByNavigation)
                    .WithMany(p => p.TransactionModifiedByNavigations)
                    .HasForeignKey(d => d.ModifiedBy)
                    .HasConstraintName("FK_transaction_Users1");

                entity.HasOne(d => d.PaidByNavigation)
                    .WithMany(p => p.TransactionPaidByNavigations)
                    .HasForeignKey(d => d.PaidBy)
                    .HasConstraintName("FK_transaction_Users2");
            });

            modelBuilder.Entity<TransactionLine>(entity =>
            {
                entity.ToTable("transaction_line", "trn");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.AutoRenew).HasColumnName("auto_renew");

                entity.Property(e => e.BasePrice)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("base_price");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("description")
                    .HasComment("Complete name of the purchased item");

                entity.Property(e => e.DiscountAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("discount_amount");

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .HasColumnName("end_date");

                entity.Property(e => e.OrderIndex).HasColumnName("order_index");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.ReferenceId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("reference_id")
                    .HasComment("License, Feature, or App Id");

                entity.Property(e => e.Sdescription)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("sdescription");

                entity.Property(e => e.StartDate)
                    .HasColumnType("date")
                    .HasColumnName("start_date");

                entity.Property(e => e.SubTotal)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("sub_total");

                entity.Property(e => e.Total)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("total");

                entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("type")
                    .HasComment("License, Feature, App");

                entity.Property(e => e.VatAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("vat_amount");

                entity.HasOne(d => d.Transaction)
                    .WithMany(p => p.TransactionLines)
                    .HasForeignKey(d => d.TransactionId)
                    .HasConstraintName("FK_transaction_line_transaction");
            });

            modelBuilder.Entity<TransactionPayment>(entity =>
            {
                entity.ToTable("transaction_payment", "trn");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("amount");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.OrderIndex).HasColumnName("order_index");

                entity.Property(e => e.PaymentMethodId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("payment_method_id");

                entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

                entity.HasOne(d => d.PaymentMethod)
                    .WithMany(p => p.TransactionPayments)
                    .HasForeignKey(d => d.PaymentMethodId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_transaction_payment_payment_method");

                entity.HasOne(d => d.Transaction)
                    .WithMany(p => p.TransactionPayments)
                    .HasForeignKey(d => d.TransactionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_transaction_payment_transaction");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.CompanyId, "IX_Users_CompanyId");

                entity.HasIndex(e => e.Username, "IX_Users_Username")
                    .IsUnique();

                entity.Property(e => e.CompanyId)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.EmployeeId).IsRequired();

                entity.Property(e => e.FirstName).IsRequired();

                entity.Property(e => e.IsAdmin)
                    .HasColumnName("is_admin")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.LastName).IsRequired();

                entity.Property(e => e.Otp)
                    .HasMaxLength(6)
                    .HasColumnName("OTP");

                entity.Property(e => e.OtpResendCount).HasColumnName("otpResendCount");

                entity.Property(e => e.OtpSentAt)
                    .HasColumnType("datetime")
                    .HasColumnName("otpSentAt");

                entity.Property(e => e.Password).IsRequired();

                entity.Property(e => e.PhoneNumber).HasMaxLength(20);

                entity.Property(e => e.Username).IsRequired();

                entity.Property(e => e.VerifiedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<UserLink>(entity =>
            {
                entity.ToTable("user_link");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.LinkedUserId).HasColumnName("linked_user_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.LinkedUser)
                    .WithMany(p => p.UserLinkLinkedUsers)
                    .HasForeignKey(d => d.LinkedUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_user_link_Users1");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserLinkUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_user_link_Users");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
