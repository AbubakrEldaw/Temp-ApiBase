using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class POSContext : DbContext
    {
        public virtual DbSet<AggrInfo> AggrInfos { get; set; }
        public virtual DbSet<AggrItemMapping> AggrItemMappings { get; set; }
        public virtual DbSet<Aggregator> Aggregators { get; set; }
        public virtual DbSet<ApiOrder> ApiOrders { get; set; }
        public virtual DbSet<ApiOrderStagingStatus> ApiOrderStagingStatuses { get; set; }
        public virtual DbSet<ApiOrderStatusHistory> ApiOrderStatusHistories { get; set; }
        public virtual DbSet<Area> Areas { get; set; }
        public virtual DbSet<Branch> Branches { get; set; }
        public virtual DbSet<CallCenterDevice> CallCenterDevices { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public virtual DbSet<CustomerComment> CustomerComments { get; set; }
        public virtual DbSet<CustomerCustomerGroup> CustomerCustomerGroups { get; set; }
        public virtual DbSet<CustomerGroup> CustomerGroups { get; set; }
        public virtual DbSet<CustomerLoyaltyTransaction> CustomerLoyaltyTransactions { get; set; }
        public virtual DbSet<DeliveryZone> DeliveryZones { get; set; }
        public virtual DbSet<DiningOption> DiningOptions { get; set; }
        public virtual DbSet<Discount> Discounts { get; set; }
        public virtual DbSet<DiscountCustomerGroup> DiscountCustomerGroups { get; set; }
        public virtual DbSet<DiscountType> DiscountTypes { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<EmployeeGroup> EmployeeGroups { get; set; }
        public virtual DbSet<Fee> Fees { get; set; }
        public virtual DbSet<FeeItem> FeeItems { get; set; }
        public virtual DbSet<FeeType> FeeTypes { get; set; }
        public virtual DbSet<IntegrationBranchLevel> IntegrationBranchLevels { get; set; }
        public virtual DbSet<IntegrationDef> IntegrationDefs { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<ItemBranchPrice> ItemBranchPrices { get; set; }
        public virtual DbSet<ItemCategory> ItemCategories { get; set; }
        public virtual DbSet<ItemDiscount> ItemDiscounts { get; set; }
        public virtual DbSet<ItemDivision> ItemDivisions { get; set; }
        public virtual DbSet<ItemGroup> ItemGroups { get; set; }
        public virtual DbSet<ItemModifier> ItemModifiers { get; set; }
        public virtual DbSet<ItemModifierGroup> ItemModifierGroups { get; set; }
        public virtual DbSet<ItemNotInBranch> ItemNotInBranches { get; set; }
        public virtual DbSet<KitchenPrintGroup> KitchenPrintGroups { get; set; }
        public virtual DbSet<KitchenPrintGroupItem> KitchenPrintGroupItems { get; set; }
        public virtual DbSet<KitchenPrintGroupPosPrinter> KitchenPrintGroupPosPrinters { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<MenuGroup> MenuGroups { get; set; }
        public virtual DbSet<ModifierGroup> ModifierGroups { get; set; }
        public virtual DbSet<ModifierGroupItem> ModifierGroupItems { get; set; }
        public virtual DbSet<NumberSeries> NumberSeries { get; set; }
        public virtual DbSet<NumberSeriesLine> NumberSeriesLines { get; set; }
        public virtual DbSet<NumberSeriesSetting> NumberSeriesSettings { get; set; }
        public virtual DbSet<OrderFee> OrderFees { get; set; }
        public virtual DbSet<OrderHeader> OrderHeaders { get; set; }
        public virtual DbSet<OrderItem> OrderItems { get; set; }
        public virtual DbSet<OrderItemCustomerComment> OrderItemCustomerComments { get; set; }
        public virtual DbSet<OrderPayment> OrderPayments { get; set; }
        public virtual DbSet<OrderSource> OrderSources { get; set; }
        public virtual DbSet<OrderStatus> OrderStatuses { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<PaymentType> PaymentTypes { get; set; }
        public virtual DbSet<Perm> Perms { get; set; }
        public virtual DbSet<PermGroup> PermGroups { get; set; }
        public virtual DbSet<PermRole> PermRoles { get; set; }
        public virtual DbSet<PermRolePerm> PermRolePerms { get; set; }
        public virtual DbSet<PosDevice> PosDevices { get; set; }
        public virtual DbSet<PosDeviceMenu> PosDeviceMenus { get; set; }
        public virtual DbSet<PosPrinter> PosPrinters { get; set; }
        public virtual DbSet<Status> Statuses { get; set; }
        public virtual DbSet<SysFeature> SysFeatures { get; set; }
        public virtual DbSet<VatGroup> VatGroups { get; set; }
        public virtual DbSet<VoidReason> VoidReasons { get; set; }
        public virtual DbSet<VoidType> VoidTypes { get; set; }
        public virtual DbSet<WorkDay> WorkDays { get; set; }
        public virtual DbSet<WorkDayShift> WorkDayShifts { get; set; }
        public virtual DbSet<WorkDayShiftCashTransaction> WorkDayShiftCashTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<AggrInfo>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.BranchId });

                entity.ToTable("aggr_info", "int");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.BranchId)
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.CashPaymentId)
                    .HasMaxLength(20)
                    .HasColumnName("cash_payment_id");

                entity.Property(e => e.CreditPaymentId)
                    .HasMaxLength(20)
                    .HasColumnName("credit_payment_id");

                entity.Property(e => e.DiningOptionId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("dining_option_id");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("email");

                entity.Property(e => e.OrderSourceId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("order_source_id");

                entity.Property(e => e.Pay)
                    .IsRequired()
                    .HasColumnName("pay")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.PayAs)
                    .HasMaxLength(20)
                    .HasColumnName("pay_as");

                entity.Property(e => e.Pwd)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("pwd");

                entity.Property(e => e.Text).HasColumnName("text");

                entity.Property(e => e.Token).HasColumnName("token");

                entity.Property(e => e.UnmapedCustomerCommentId)
                    .HasMaxLength(20)
                    .HasColumnName("unmaped_customer_comment_id");

                entity.Property(e => e.UnmapedItemId)
                    .HasMaxLength(20)
                    .HasColumnName("unmaped_item_id");

                entity.Property(e => e.UnmapedModifierId)
                    .HasMaxLength(20)
                    .HasColumnName("unmaped_modifier_id");
            });

            modelBuilder.Entity<ItemDiscount>(entity =>
            {
                entity.HasKey(e => new { e.DiscountId, e.ItemId })
                    .HasName("PK_discount_item");

                entity.ToTable("item_discount", "def");

                entity.Property(e => e.DiscountId)
                    .HasMaxLength(20)
                    .HasColumnName("discount_id");

                entity.Property(e => e.ItemId)
                    .HasMaxLength(20)
                    .HasColumnName("item_id");

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.ItemDiscounts)
                    .HasForeignKey(d => d.DiscountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_discount_discount");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemDiscounts)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_discount_item");
            });

            modelBuilder.Entity<AggrItemMapping>(entity =>
            {
                entity.HasKey(e => new { e.AggrId, e.ItemType, e.ItemAggrId, e.AggrVariantId })
                    .HasName("PK_aggr_item_mapping_1");

                entity.ToTable("aggr_item_mapping", "int");

                entity.Property(e => e.AggrId)
                    .HasMaxLength(50)
                    .HasColumnName("aggr_id");

                entity.Property(e => e.ItemType)
                    .HasMaxLength(50)
                    .HasColumnName("item_type");

                entity.Property(e => e.ItemAggrId)
                    .HasMaxLength(100)
                    .HasColumnName("item_aggr_id");

                entity.Property(e => e.AggrVariantId)
                    .HasMaxLength(100)
                    .HasColumnName("aggr_variant_id");

                entity.Property(e => e.AggrPrice)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("aggr_price");

                entity.Property(e => e.CustomerComentId)
                    .HasMaxLength(20)
                    .HasColumnName("customer_coment_id");

                entity.Property(e => e.ItemId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("item_id");

                entity.Property(e => e.ItemVariantId)
                    .HasMaxLength(20)
                    .HasColumnName("item_variant_id");
            });

            modelBuilder.Entity<Aggregator>(entity =>
            {
                entity.ToTable("aggregator", "int");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.ImageName).HasColumnName("image_name");

                entity.Property(e => e.ImagePath).HasColumnName("image_path");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id")
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.AggregatorCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_aggregator_created_by_employee");

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.AggregatorModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy)
                    .HasConstraintName("FK_aggregator_modified_by_employee");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Aggregators)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_aggregator_status");
            });

            modelBuilder.Entity<ApiOrder>(entity =>
            {
                entity.ToTable("api_order", "int");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.AppId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("app_id");

                entity.Property(e => e.AppOrderId)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("app_order_id");

                entity.Property(e => e.AppOrderNumber)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("app_order_number");

                entity.Property(e => e.AppOrderPickupDatetime)
                    .HasColumnType("datetime")
                    .HasColumnName("app_order_pickup_datetime");

                entity.Property(e => e.AppOrderReceiveDatetime)
                    .HasColumnType("datetime")
                    .HasColumnName("app_order_receive_datetime");

                entity.Property(e => e.BranchId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.DiscountAmount)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("discount_amount");

                entity.Property(e => e.GlobalLocationId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("global_location_id");

                entity.Property(e => e.GrandTotal)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("grand_total");

                entity.Property(e => e.OrderIsPaid).HasColumnName("order_is_paid");

                entity.Property(e => e.OrderModel)
                    .IsRequired()
                    .HasColumnName("order_model");

                entity.Property(e => e.OrderSource)
                    .IsRequired()
                    .HasColumnName("order_source");

                entity.Property(e => e.OrderType)
                    .HasColumnName("order_type")
                    .HasDefaultValueSql("((1))")
                    .HasComment("1=Delivery, 2=DineIn, 3=Takeaway");

                entity.Property(e => e.PaymentType)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("payment_type");

                entity.Property(e => e.PosOrderId).HasColumnName("pos_order_id");

                entity.Property(e => e.PosOrderNumber)
                    .HasMaxLength(50)
                    .HasColumnName("pos_order_number");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.StagingStatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("staging_status_id");

                entity.Property(e => e.SubTotal)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("sub_total");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.ApiOrders)
                    .HasForeignKey(d => d.BranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_api_order_branch");

                entity.HasOne(d => d.StagingStatus)
                    .WithMany(p => p.ApiOrders)
                    .HasForeignKey(d => d.StagingStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_api_order_api_order_staging_status");
            });

            modelBuilder.Entity<ApiOrderStagingStatus>(entity =>
            {
                entity.ToTable("api_order_staging_status", "int");

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

            modelBuilder.Entity<ApiOrderStatusHistory>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.StatusTime });

                entity.ToTable("api_order_status_history", "int");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.StatusTime)
                    .HasColumnType("datetime")
                    .HasColumnName("status_time");

                entity.Property(e => e.ApiOrderId).HasColumnName("api_order_id");

                entity.Property(e => e.ApiStatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("api_status_id");

                entity.Property(e => e.UpdateSource)
                    .HasMaxLength(50)
                    .HasColumnName("update_source");

                entity.HasOne(d => d.ApiOrder)
                    .WithMany(p => p.ApiOrderStatusHistories)
                    .HasForeignKey(d => d.ApiOrderId)
                    .HasConstraintName("FK_api_order_status_history_api_order");

                entity.HasOne(d => d.ApiStatus)
                    .WithMany(p => p.ApiOrderStatusHistories)
                    .HasForeignKey(d => d.ApiStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_api_order_status_history_api_order_staging_status");
            });

            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("area", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.BranchId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.DiningOptionId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("dining_option_id");

                entity.Property(e => e.List).HasColumnName("list");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.NumberOfTables).HasColumnName("number_of_tables");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.Sname)
                    .HasMaxLength(50)
                    .HasColumnName("sname");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id")
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.Areas)
                    .HasForeignKey(d => d.BranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_area_branch");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.AreaCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.DiningOption)
                    .WithMany(p => p.Areas)
                    .HasForeignKey(d => d.DiningOptionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_area_dining_option");

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.AreaModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Areas)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_area_status");
            });

            modelBuilder.Entity<Branch>(entity =>
            {
                entity.ToTable("branch", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(100)
                    .HasColumnName("address")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.BuildingNumber)
                    .HasMaxLength(50)
                    .HasColumnName("building_number");

                entity.Property(e => e.CityName)
                    .HasMaxLength(50)
                    .HasColumnName("city_name");

                entity.Property(e => e.CitySubdivisionName)
                    .HasMaxLength(50)
                    .HasColumnName("city_subdivision_name");

                entity.Property(e => e.Country)
                    .HasMaxLength(50)
                    .HasColumnName("country");

                entity.Property(e => e.CountrySubentity)
                    .HasMaxLength(50)
                    .HasColumnName("country_subentity");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.Crn)
                    .HasMaxLength(50)
                    .HasColumnName("crn");

                entity.Property(e => e.DefaultMenuId)
                    .HasMaxLength(20)
                    .HasColumnName("default_menu_id");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .HasColumnName("email");

                entity.Property(e => e.FranshizeCompany)
                    .HasMaxLength(200)
                    .HasColumnName("franshize_company");

                entity.Property(e => e.Latitude)
                    .HasMaxLength(50)
                    .HasColumnName("latitude");

                entity.Property(e => e.Longitude)
                    .HasMaxLength(50)
                    .HasColumnName("longitude");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Phone)
                    .HasMaxLength(20)
                    .HasColumnName("phone");

                entity.Property(e => e.PlotIdentification)
                    .HasMaxLength(50)
                    .HasColumnName("plot_Identification");

                entity.Property(e => e.PostalZone)
                    .HasMaxLength(50)
                    .HasColumnName("postal_zone");

                entity.Property(e => e.RegistrationName)
                    .HasMaxLength(50)
                    .HasColumnName("registration_name");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.Sname)
                    .HasMaxLength(50)
                    .HasColumnName("sname");

                entity.Property(e => e.StreetName)
                    .HasMaxLength(50)
                    .HasColumnName("street_name");

                entity.Property(e => e.Tin)
                    .HasMaxLength(50)
                    .HasColumnName("tin");

                entity.Property(e => e.VatGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("vat_group_id");

                entity.Property(e => e.VatRegNo)
                    .HasMaxLength(100)
                    .HasColumnName("vat_reg_no");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.BranchCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.BranchModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.VatGroup)
                    .WithMany(p => p.Branches)
                    .HasForeignKey(d => d.VatGroupId)
                    .HasConstraintName("FK_branch_vat_group");
            });

            modelBuilder.Entity<CallCenterDevice>(entity =>
            {
                entity.ToTable("call_center_device", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.ActivationCode)
                    .HasMaxLength(10)
                    .HasColumnName("activation_code");

                entity.Property(e => e.AutoPrint).HasColumnName("auto_print");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.DefaultPrintCount).HasColumnName("default_print_count");

                entity.Property(e => e.DiningOptionId)
                    .HasMaxLength(20)
                    .HasColumnName("dining_option_id");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.OrderSourceId)
                    .HasMaxLength(20)
                    .HasColumnName("order_source_id");

                entity.Property(e => e.PrintLanguage).HasColumnName("print_language");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.CallCenterDeviceCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.DiningOption)
                    .WithMany(p => p.CallCenterDevices)
                    .HasForeignKey(d => d.DiningOptionId)
                    .HasConstraintName("FK_call_center_device_dining_option");

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.CallCenterDeviceModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.OrderSource)
                    .WithMany(p => p.CallCenterDevices)
                    .HasForeignKey(d => d.OrderSourceId)
                    .HasConstraintName("FK_call_center_device_order_source");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.CallCenterDevices)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_call_center_device_status");
            });

            modelBuilder.Entity<City>(entity =>
            {
                entity.ToTable("city", "crm");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.CountryId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("country_id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Cities)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_city_country");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("company", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(N'company01')");

                entity.Property(e => e.Address)
                    .HasMaxLength(100)
                    .HasColumnName("address");

                entity.Property(e => e.BuildingNumber)
                    .HasMaxLength(50)
                    .HasColumnName("building_number");

                entity.Property(e => e.CityName)
                    .HasMaxLength(50)
                    .HasColumnName("city_name");

                entity.Property(e => e.CitySubdivisionName)
                    .HasMaxLength(50)
                    .HasColumnName("city_subdivision_name");

                entity.Property(e => e.Country)
                    .HasMaxLength(50)
                    .HasColumnName("country");

                entity.Property(e => e.CountrySubentity)
                    .HasMaxLength(50)
                    .HasColumnName("country_subentity");

                entity.Property(e => e.Crn)
                    .HasMaxLength(50)
                    .HasColumnName("crn");

                entity.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("currency");

                entity.Property(e => e.EnableSname).HasColumnName("enable_sname");

                entity.Property(e => e.Latitude)
                    .HasMaxLength(50)
                    .HasColumnName("latitude");

                entity.Property(e => e.Logo).HasColumnName("logo");

                entity.Property(e => e.Longitude)
                    .HasMaxLength(50)
                    .HasColumnName("longitude");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.PlotIdentification)
                    .HasMaxLength(50)
                    .HasColumnName("plot_Identification");

                entity.Property(e => e.PostalZone)
                    .HasMaxLength(50)
                    .HasColumnName("postal_zone");

                entity.Property(e => e.RegistrationName)
                    .HasMaxLength(50)
                    .HasColumnName("registration_name");

                entity.Property(e => e.SalePriceInclusiveOfVat)
                    .HasColumnName("sale_price_inclusive_of_vat")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StreetName)
                    .HasMaxLength(50)
                    .HasColumnName("street_name");

                entity.Property(e => e.TimeZone)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("time_zone");

                entity.Property(e => e.Tin)
                    .HasMaxLength(50)
                    .HasColumnName("tin");

                entity.Property(e => e.VatRegNo)
                    .HasMaxLength(100)
                    .HasColumnName("vat_reg_no");
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("country", "crm");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("code");

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

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("customer", "crm");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.BuildingNumber)
                    .HasMaxLength(50)
                    .HasColumnName("building_number");

                entity.Property(e => e.CityName)
                    .HasMaxLength(50)
                    .HasColumnName("city_name");

                entity.Property(e => e.CitySubdivisionName)
                    .HasMaxLength(50)
                    .HasColumnName("city_subdivision_name");

                entity.Property(e => e.Country)
                    .HasMaxLength(50)
                    .HasColumnName("country");

                entity.Property(e => e.CountrySubentity)
                    .HasMaxLength(50)
                    .HasColumnName("country_subentity");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.Crn)
                    .HasMaxLength(50)
                    .HasColumnName("crn");

                entity.Property(e => e.FirstVisit)
                    .HasColumnType("datetime")
                    .HasColumnName("first_visit");

                entity.Property(e => e.LastVisit)
                    .HasColumnType("datetime")
                    .HasColumnName("last_visit");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("name");

                entity.Property(e => e.Nat)
                    .HasMaxLength(50)
                    .HasColumnName("nat");

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("phone");

                entity.Property(e => e.PlotIdentification)
                    .HasMaxLength(50)
                    .HasColumnName("plot_Identification");

                entity.Property(e => e.Points).HasColumnName("points");

                entity.Property(e => e.PostalZone)
                    .HasMaxLength(50)
                    .HasColumnName("postal_zone");

                entity.Property(e => e.RegistrationName)
                    .HasMaxLength(50)
                    .HasColumnName("registration_name");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.StreetName)
                    .HasMaxLength(50)
                    .HasColumnName("street_name");

                entity.Property(e => e.TotalSpent)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("total_spent");

                entity.Property(e => e.TotalVisits).HasColumnName("total_visits");

                entity.Property(e => e.VatRegNo)
                    .HasMaxLength(100)
                    .HasColumnName("vat_reg_no");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.CustomerCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.CustomerModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);
            });

            modelBuilder.Entity<CustomerAddress>(entity =>
            {
                entity.ToTable("customer_address", "crm");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CityId)
                    .HasMaxLength(20)
                    .HasColumnName("city_id");

                entity.Property(e => e.CountryId)
                    .HasMaxLength(20)
                    .HasColumnName("country_id");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.CustomerId).HasColumnName("customer_id");

                entity.Property(e => e.DeliveryZoneId)
                    .HasMaxLength(20)
                    .HasColumnName("delivery_zone_id");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.CustomerAddresses)
                    .HasForeignKey(d => d.CityId)
                    .HasConstraintName("FK_customer_address_city");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.CustomerAddresses)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_customer_address_country");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerAddresses)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_customer_address_customer");

                entity.HasOne(d => d.DeliveryZone)
                    .WithMany(p => p.CustomerAddresses)
                    .HasForeignKey(d => d.DeliveryZoneId)
                    .HasConstraintName("FK_customer_address_delivery_zone");
            });

            modelBuilder.Entity<CustomerComment>(entity =>
            {
                entity.ToTable("customer_comment", "def");

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

            modelBuilder.Entity<CustomerCustomerGroup>(entity =>
            {
                entity.HasKey(e => new { e.CustomerId, e.CustomerGroupId });

                entity.ToTable("customer_customer_group", "crm");

                entity.Property(e => e.CustomerId).HasColumnName("customer_id");

                entity.Property(e => e.CustomerGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("customer_group_id");

                entity.HasOne(d => d.CustomerGroup)
                    .WithMany(p => p.CustomerCustomerGroups)
                    .HasForeignKey(d => d.CustomerGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_customer_customer_group_customer_group");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerCustomerGroups)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_customer_customer_group_customer");
            });

            modelBuilder.Entity<CustomerGroup>(entity =>
            {
                entity.ToTable("customer_group", "crm");

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

            modelBuilder.Entity<CustomerLoyaltyTransaction>(entity =>
            {
                entity.ToTable("customer_loyalty_transaction", "trn");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.BaseAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("base_amount");

                entity.Property(e => e.CustomerId).HasColumnName("customer_id");

                entity.Property(e => e.OrderHeaderId).HasColumnName("order_header_id");

                entity.Property(e => e.Points).HasColumnName("points");

                entity.Property(e => e.Rate)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("rate");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("type");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerLoyaltyTransactions)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_customer_loyalty_transaction_customer");
            });

            modelBuilder.Entity<DeliveryZone>(entity =>
            {
                entity.ToTable("delivery_zone", "crm");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.CityId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("city_id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.DeliveryZones)
                    .HasForeignKey(d => d.CityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_delivery_zone_city");
            });

            modelBuilder.Entity<DiningOption>(entity =>
            {
                entity.ToTable("dining_option", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("name");

                entity.Property(e => e.OrderIndex).HasColumnName("order_index");

                entity.Property(e => e.Sname)
                    .HasMaxLength(20)
                    .HasColumnName("sname");
            });

            modelBuilder.Entity<Discount>(entity =>
            {
                entity.ToTable("discount", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.AutoApply).HasColumnName("auto_apply");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.DiscountTypeId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("discount_type_id");

                entity.Property(e => e.MaxDiscountAmount)
                    .HasColumnType("decimal(8, 2)")
                    .HasColumnName("max_discount_amount");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Priority).HasColumnName("priority");

                entity.Property(e => e.RequireCustomer).HasColumnName("require_customer");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Value)
                    .HasColumnType("decimal(8, 2)")
                    .HasColumnName("value");

                entity.Property(e => e.WholeOrder).HasColumnName("whole_order");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.DiscountCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.DiscountType)
                    .WithMany(p => p.Discounts)
                    .HasForeignKey(d => d.DiscountTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_discount_discount_type");

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.DiscountModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Discounts)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_discount_status");
            });

            modelBuilder.Entity<DiscountCustomerGroup>(entity =>
            {
                entity.HasKey(e => new { e.DiscountId, e.CustomerGroupId });

                entity.ToTable("discount_customer_group", "def");

                entity.Property(e => e.DiscountId)
                    .HasMaxLength(20)
                    .HasColumnName("discount_id");

                entity.Property(e => e.CustomerGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("customer_group_id");

                entity.HasOne(d => d.CustomerGroup)
                    .WithMany(p => p.DiscountCustomerGroups)
                    .HasForeignKey(d => d.CustomerGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_discount_customer_group_customer_group");

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.DiscountCustomerGroups)
                    .HasForeignKey(d => d.DiscountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_discount_customer_group_discount");
            });

            modelBuilder.Entity<DiscountType>(entity =>
            {
                entity.ToTable("discount_type", "def");

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

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("employee", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.AspnetUserId).HasColumnName("aspnet_user_id");

                entity.Property(e => e.Biometrics).HasColumnName("biometrics");

                entity.Property(e => e.BranchId)
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.DefaultLanguage)
                    .HasColumnName("default_language")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .HasColumnName("email");

                entity.Property(e => e.EmployeeGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("employee_group_id");

                entity.Property(e => e.ImagePath).HasColumnName("image_path");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Password).HasColumnName("password");

                entity.Property(e => e.PermRoleId)
                    .HasMaxLength(20)
                    .HasColumnName("perm_role_id");

                entity.Property(e => e.Phone)
                    .HasMaxLength(20)
                    .HasColumnName("phone");

                entity.Property(e => e.Pin).HasColumnName("pin");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.UserName)
                    .HasMaxLength(20)
                    .HasColumnName("user_name");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.BranchId)
                    .HasConstraintName("FK_employee_branch");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.InverseCreateByNavigation)
                    .HasForeignKey(d => d.CreateBy);

                entity.HasOne(d => d.EmployeeGroup)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.EmployeeGroupId)
                    .HasConstraintName("FK_employee_employee_group");

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.InverseModifyByNavigation)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.PermRole)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.PermRoleId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_employee_perm_role");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_employee_status");
            });

            modelBuilder.Entity<EmployeeGroup>(entity =>
            {
                entity.ToTable("employee_group", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.EmployeeGroupCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.EmployeeGroupModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);
            });

            modelBuilder.Entity<Fee>(entity =>
            {
                entity.ToTable("fee", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.ApplyOnItem).HasColumnName("apply_on_item");

                entity.Property(e => e.ApplyPerUnit).HasColumnName("apply_per_unit");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.FeeTypeId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("fee_type_id");

                entity.Property(e => e.MinCharge)
                    .HasColumnType("decimal(8, 2)")
                    .HasColumnName("min_charge");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Value)
                    .HasColumnType("decimal(8, 2)")
                    .HasColumnName("value");

                entity.Property(e => e.VatGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("vat_group_id");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.FeeCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.FeeType)
                    .WithMany(p => p.Fees)
                    .HasForeignKey(d => d.FeeTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_fee_fee_type");

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.FeeModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Fees)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_fee_status");

                entity.HasOne(d => d.VatGroup)
                    .WithMany(p => p.Fees)
                    .HasForeignKey(d => d.VatGroupId)
                    .HasConstraintName("FK_fee_vat_group");
            });

            modelBuilder.Entity<FeeItem>(entity =>
            {
                entity.HasKey(e => new { e.FeeId, e.ItemId });

                entity.ToTable("fee_item", "def");

                entity.Property(e => e.FeeId)
                    .HasMaxLength(20)
                    .HasColumnName("fee_id");

                entity.Property(e => e.ItemId)
                    .HasMaxLength(20)
                    .HasColumnName("item_id");

                entity.HasOne(d => d.Fee)
                    .WithMany(p => p.FeeItems)
                    .HasForeignKey(d => d.FeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_fee_item_fee");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.FeeItems)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_fee_item_item");
            });

            modelBuilder.Entity<FeeType>(entity =>
            {
                entity.ToTable("fee_type", "def");

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

            modelBuilder.Entity<IntegrationBranchLevel>(entity =>
            {
                entity.HasKey(e => new { e.IntId, e.BranchId });

                entity.ToTable("integration_branch_level", "int");

                entity.Property(e => e.IntId)
                    .HasMaxLength(20)
                    .HasColumnName("int_id");

                entity.Property(e => e.BranchId)
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.JsonValue)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasColumnName("json_value");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.IntegrationBranchLevels)
                    .HasForeignKey(d => d.BranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_integration_branch_level_branch");

                entity.HasOne(d => d.Int)
                    .WithMany(p => p.IntegrationBranchLevels)
                    .HasForeignKey(d => d.IntId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_integration_branch_level_integration_def");
            });

            modelBuilder.Entity<IntegrationDef>(entity =>
            {
                entity.ToTable("integration_def", "int");

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

            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("item", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Barcode).HasColumnName("barcode");

                entity.Property(e => e.Cost)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("cost");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.HasVariants).HasColumnName("has_variants");

                entity.Property(e => e.ImageName).HasColumnName("image_name");

                entity.Property(e => e.ImagePath).HasColumnName("image_path");

                entity.Property(e => e.ItemCategoryId)
                    .HasMaxLength(20)
                    .HasColumnName("item_category_id");

                entity.Property(e => e.ItemDivisionId)
                    .HasMaxLength(20)
                    .HasColumnName("item_division_id");

                entity.Property(e => e.ItemGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("item_group_id");

                entity.Property(e => e.MaxQty).HasColumnName("max_qty");

                entity.Property(e => e.MinQty).HasColumnName("min_qty");

                entity.Property(e => e.Modifier).HasColumnName("modifier");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Nutrition).HasColumnName("nutrition");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.Purchase).HasColumnName("purchase");

                entity.Property(e => e.PurchaseToRecipeUom).HasColumnName("purchase_to_recipe_uom");

                entity.Property(e => e.PurchaseUom)
                    .HasMaxLength(20)
                    .HasColumnName("purchase_uom");

                entity.Property(e => e.Recipe).HasColumnName("recipe");

                entity.Property(e => e.RecipeUom)
                    .HasMaxLength(20)
                    .HasColumnName("recipe_uom");

                entity.Property(e => e.ReorderPoint).HasColumnName("reorder_point");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.Sale).HasColumnName("sale");

                entity.Property(e => e.SaleByWeight)
                    .HasColumnName("sale_by_weight")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Sdescription)
                    .HasColumnName("sdescription")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.UseProduction).HasColumnName("use_production");

                entity.Property(e => e.VariantParentId)
                    .HasMaxLength(20)
                    .HasColumnName("variant_parent_id");

                entity.Property(e => e.VatGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("vat_group_id");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.ItemCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ItemCategory)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.ItemCategoryId)
                    .HasConstraintName("FK_item_item_category");

                entity.HasOne(d => d.ItemDivision)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.ItemDivisionId)
                    .HasConstraintName("FK_item_item_division");

                entity.HasOne(d => d.ItemGroup)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.ItemGroupId)
                    .HasConstraintName("FK_item_item_group");

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.ItemModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_status");

                entity.HasOne(d => d.VariantParent)
                    .WithMany(p => p.InverseVariantParent)
                    .HasForeignKey(d => d.VariantParentId)
                    .HasConstraintName("FK_item_item_variant");

                entity.HasOne(d => d.VatGroup)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.VatGroupId)
                    .HasConstraintName("FK_item_vat_group");
            });

            modelBuilder.Entity<ItemBranchPrice>(entity =>
            {
                entity.HasKey(e => new { e.BranchId, e.ItemId });

                entity.ToTable("item_branch_price", "def");

                entity.Property(e => e.BranchId)
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.ItemId)
                    .HasMaxLength(20)
                    .HasColumnName("item_id");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("price");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.ItemBranchPrices)
                    .HasForeignKey(d => d.BranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_branch_price_branch");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemBranchPrices)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("FK_item_branch_price_item");
            });

            modelBuilder.Entity<ItemCategory>(entity =>
            {
                entity.ToTable("item_category", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.ItemDivisionId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("item_division_id");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id")
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.ItemCategoryCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ItemDivision)
                    .WithMany(p => p.ItemCategories)
                    .HasForeignKey(d => d.ItemDivisionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_category_item_division");

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.ItemCategoryModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy)
                    .HasConstraintName("FK_item_category_employee");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.ItemCategories)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_category_status");
            });

            modelBuilder.Entity<ItemDiscount>(entity =>
            {
                entity.HasKey(e => new { e.DiscountId, e.ItemId })
                    .HasName("PK_discount_item");

                entity.ToTable("item_discount", "def");

                entity.Property(e => e.DiscountId)
                    .HasMaxLength(20)
                    .HasColumnName("discount_id");

                entity.Property(e => e.ItemId)
                    .HasMaxLength(20)
                    .HasColumnName("item_id");

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.ItemDiscounts)
                    .HasForeignKey(d => d.DiscountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_discount_discount");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemDiscounts)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_discount_item");
            });

            modelBuilder.Entity<ItemDivision>(entity =>
            {
                entity.ToTable("item_division", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id")
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.ItemDivisionCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.ItemDivisionModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.ItemDivisions)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_division_status");
            });

            modelBuilder.Entity<ItemGroup>(entity =>
            {
                entity.ToTable("item_group", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.ImageName).HasColumnName("image_name");

                entity.Property(e => e.ImagePath).HasColumnName("image_path");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.OrderIndex).HasColumnName("order_index");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id")
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.ItemGroupCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.ItemGroupModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.ItemGroups)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_group_status");
            });

            modelBuilder.Entity<ItemModifier>(entity =>
            {
                entity.HasKey(e => new { e.ItemId, e.ModifierItemId });

                entity.ToTable("item_modifier", "def");

                entity.Property(e => e.ItemId)
                    .HasMaxLength(20)
                    .HasColumnName("item_id");

                entity.Property(e => e.ModifierItemId)
                    .HasMaxLength(20)
                    .HasColumnName("modifier_item_id");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemModifierItems)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_modifier_item_item");

                entity.HasOne(d => d.ModifierItem)
                    .WithMany(p => p.ItemModifierModifierItems)
                    .HasForeignKey(d => d.ModifierItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_modifier_item_modifier");
            });

            modelBuilder.Entity<ItemModifierGroup>(entity =>
            {
                entity.HasKey(e => new { e.ItemId, e.ModifierGroupId });

                entity.ToTable("item_modifier_group", "def");

                entity.Property(e => e.ItemId)
                    .HasMaxLength(20)
                    .HasColumnName("item_id");

                entity.Property(e => e.ModifierGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("modifier_group_id");

                entity.Property(e => e.Free).HasColumnName("free");

                entity.Property(e => e.Max).HasColumnName("max");

                entity.Property(e => e.Min).HasColumnName("min");

                entity.Property(e => e.Multiple).HasColumnName("multiple");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemModifierGroups)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_modifier_group_item");

                entity.HasOne(d => d.ModifierGroup)
                    .WithMany(p => p.ItemModifierGroups)
                    .HasForeignKey(d => d.ModifierGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_modifier_group_modifier_group");
            });

            modelBuilder.Entity<ItemNotInBranch>(entity =>
            {
                entity.HasKey(e => new { e.ItemId, e.BranchId });

                entity.ToTable("item_not_in_branch", "def");

                entity.Property(e => e.ItemId)
                    .HasMaxLength(20)
                    .HasColumnName("item_id");

                entity.Property(e => e.BranchId)
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.ItemNotInBranches)
                    .HasForeignKey(d => d.BranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_item_not_in_branch_branch");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemNotInBranches)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("FK_item_not_in_branch_item");
            });

            modelBuilder.Entity<KitchenPrintGroup>(entity =>
            {
                entity.ToTable("kitchen_print_group", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.AdvancedRouting).HasColumnName("advanced_routing");

                entity.Property(e => e.BranchId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.PrintLanguage).HasColumnName("print_language");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.KitchenPrintGroups)
                    .HasForeignKey(d => d.BranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_kitchen_print_group_branch");
            });

            modelBuilder.Entity<KitchenPrintGroupItem>(entity =>
            {
                entity.HasKey(e => new { e.KitchenPrintGroupId, e.ItemId });

                entity.ToTable("kitchen_print_group_item", "def");

                entity.Property(e => e.KitchenPrintGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("kitchen_print_group_id");

                entity.Property(e => e.ItemId)
                    .HasMaxLength(20)
                    .HasColumnName("item_id");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.KitchenPrintGroupItems)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("FK_kitchen_print_group_item_item");

                entity.HasOne(d => d.KitchenPrintGroup)
                    .WithMany(p => p.KitchenPrintGroupItems)
                    .HasForeignKey(d => d.KitchenPrintGroupId)
                    .HasConstraintName("FK_kitchen_print_group_item_kitchen_print_group");
            });

            modelBuilder.Entity<KitchenPrintGroupPosPrinter>(entity =>
            {
                entity.HasKey(e => new { e.KitchenPrintGroupId, e.PosPrinterId })
                    .HasName("PK_pos_printer_kitchen_group");

                entity.ToTable("kitchen_print_group_pos_printer", "def");

                entity.Property(e => e.KitchenPrintGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("kitchen_print_group_id");

                entity.Property(e => e.PosPrinterId)
                    .HasMaxLength(20)
                    .HasColumnName("pos_printer_id");

                entity.HasOne(d => d.KitchenPrintGroup)
                    .WithMany(p => p.KitchenPrintGroupPosPrinters)
                    .HasForeignKey(d => d.KitchenPrintGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_kitchen_group_pos_printer_kitchen_print_group");

                entity.HasOne(d => d.PosPrinter)
                    .WithMany(p => p.KitchenPrintGroupPosPrinters)
                    .HasForeignKey(d => d.PosPrinterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_kitchen_group_pos_printer_pos_printer");
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("menu", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ImageName).HasColumnName("image_name");

                entity.Property(e => e.ImagePath).HasColumnName("image_path");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Menus)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_menu_status");
            });

            modelBuilder.Entity<MenuGroup>(entity =>
            {
                entity.ToTable("menu_group", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.ImageName).HasColumnName("image_name");

                entity.Property(e => e.ImagePath).HasColumnName("image_path");

                entity.Property(e => e.MenuId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("menu_id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.OrderIndex).HasColumnName("order_index");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id");

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.MenuGroups)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("FK_menu_group_menu");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.MenuGroups)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_menu_group_status");
            });

            modelBuilder.Entity<ModifierGroup>(entity =>
            {
                entity.ToTable("modifier_group", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Free)
                    .HasColumnName("free")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Max).HasColumnName("max");

                entity.Property(e => e.Min).HasColumnName("min");

                entity.Property(e => e.Multiple).HasColumnName("multiple");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.OrderIndex).HasColumnName("order_index");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<ModifierGroupItem>(entity =>
            {
                entity.HasKey(e => new { e.ItemId, e.ModifierGroupId });

                entity.ToTable("modifier_group_item", "def");

                entity.Property(e => e.ItemId)
                    .HasMaxLength(20)
                    .HasColumnName("item_id");

                entity.Property(e => e.ModifierGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("modifier_group_id");

                entity.Property(e => e.OrderIndex).HasColumnName("order_index");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("price");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ModifierGroupItems)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_modifier_group_item_item");

                entity.HasOne(d => d.ModifierGroup)
                    .WithMany(p => p.ModifierGroupItems)
                    .HasForeignKey(d => d.ModifierGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_modifier_group_item_modifier_group");
            });

            modelBuilder.Entity<NumberSeries>(entity =>
            {
                entity.ToTable("number_series", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(N'(SELECT  alpha + REPLICATE(''0'',len(start)-LEN(isnull(last_used + increment,start))) + cast(isnull(last_used + increment,start) as nvarchar(10))')");

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

            modelBuilder.Entity<NumberSeriesLine>(entity =>
            {
                entity.HasKey(e => new { e.NumberSeriesId, e.StartDate })
                    .HasName("PK_number_series_1");

                entity.ToTable("number_series_line", "def");

                entity.Property(e => e.NumberSeriesId)
                    .HasMaxLength(20)
                    .HasColumnName("number_series_id")
                    .HasDefaultValueSql("(N'(SELECT  alpha + REPLICATE(''0'',len(start)-LEN(isnull(last_used + increment,start))) + cast(isnull(last_used + increment,start) as nvarchar(10))')");

                entity.Property(e => e.StartDate)
                    .HasColumnType("date")
                    .HasColumnName("start_date");

                entity.Property(e => e.Alpha)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("alpha");

                entity.Property(e => e.AlphaNumericLastUsed)
                    .HasMaxLength(20)
                    .HasColumnName("alpha_numeric_last_used");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.Increment).HasColumnName("increment");

                entity.Property(e => e.LastUsed)
                    .HasMaxLength(10)
                    .HasColumnName("last_used");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.Start)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("start");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id")
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.NumberSeriesLineCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.NumberSeriesLineModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.NumberSeries)
                    .WithMany(p => p.NumberSeriesLines)
                    .HasForeignKey(d => d.NumberSeriesId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_number_series_line_number_series");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.NumberSeriesLines)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_number_series_line_status");
            });

            modelBuilder.Entity<NumberSeriesSetting>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.NumberSeriesId })
                    .HasName("PK_company_number_series_setting");

                entity.ToTable("number_series_setting", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");

                entity.Property(e => e.NumberSeriesId)
                    .HasMaxLength(50)
                    .HasColumnName("number_series_id");
            });

            modelBuilder.Entity<OrderFee>(entity =>
            {
                entity.HasKey(e => new { e.OrderHeaderId, e.FeeId });

                entity.ToTable("order_fee", "trn");

                entity.Property(e => e.OrderHeaderId).HasColumnName("order_header_id");

                entity.Property(e => e.FeeId)
                    .HasMaxLength(20)
                    .HasColumnName("fee_id");

                entity.Property(e => e.BaseAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("base_amount");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.FeeTypeId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("fee_type_id");

                entity.Property(e => e.FeeValue)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fee_value");

                entity.Property(e => e.PriceVatInclusive).HasColumnName("price_vat_inclusive");

                entity.Property(e => e.Total)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("total");

                entity.Property(e => e.VatAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("vat_amount");

                entity.Property(e => e.VatGroupId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("vat_group_id");

                entity.Property(e => e.VatPercentage)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("vat_percentage");

                entity.HasOne(d => d.Fee)
                    .WithMany(p => p.OrderFees)
                    .HasForeignKey(d => d.FeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_fee_fee");

                entity.HasOne(d => d.FeeType)
                    .WithMany(p => p.OrderFees)
                    .HasForeignKey(d => d.FeeTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_fee_fee_type");

                entity.HasOne(d => d.OrderHeader)
                    .WithMany(p => p.OrderFees)
                    .HasForeignKey(d => d.OrderHeaderId)
                    .HasConstraintName("FK_order_fee_order_header");
            });

            modelBuilder.Entity<OrderHeader>(entity =>
            {
                entity.ToTable("order_header", "trn");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.AppChannelName)
                    .HasMaxLength(50)
                    .HasColumnName("app_channel_name");

                entity.Property(e => e.AppChannelOrderNumber)
                    .HasMaxLength(50)
                    .HasColumnName("app_channel_order_number");

                entity.Property(e => e.AppId)
                    .HasMaxLength(20)
                    .HasColumnName("app_id");

                entity.Property(e => e.AppOrderDeliveryDatetime)
                    .HasColumnType("datetime")
                    .HasColumnName("app_order_delivery_datetime");

                entity.Property(e => e.AppOrderId)
                    .HasMaxLength(200)
                    .HasColumnName("app_order_id");

                entity.Property(e => e.AppOrderNumber)
                    .HasMaxLength(50)
                    .HasColumnName("app_order_number");

                entity.Property(e => e.AppOrderPickupDatetime)
                    .HasColumnType("datetime")
                    .HasColumnName("app_order_pickup_datetime");

                entity.Property(e => e.AppOrderReceiveDatetime)
                    .HasColumnType("datetime")
                    .HasColumnName("app_order_receive_datetime");

                entity.Property(e => e.AppOrderStatus)
                    .HasMaxLength(20)
                    .HasColumnName("app_order_status");

                entity.Property(e => e.AreaId)
                    .HasMaxLength(20)
                    .HasColumnName("area_id");

                entity.Property(e => e.AreaTableId)
                    .HasMaxLength(20)
                    .HasColumnName("area_table_id");

                entity.Property(e => e.BranchId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.CloseAt)
                    .HasColumnType("datetime")
                    .HasColumnName("close_at");

                entity.Property(e => e.CloseBy)
                    .HasMaxLength(20)
                    .HasColumnName("close_by");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.CustomerAddress)
                    .HasMaxLength(200)
                    .HasColumnName("customer_address");

                entity.Property(e => e.CustomerBuildingNumber)
                    .HasMaxLength(50)
                    .HasColumnName("customer_building_number");

                entity.Property(e => e.CustomerCityName)
                    .HasMaxLength(50)
                    .HasColumnName("customer_city_name");

                entity.Property(e => e.CustomerCitySubdivisionName)
                    .HasMaxLength(50)
                    .HasColumnName("customer_city_subdivision_name");

                entity.Property(e => e.CustomerCountry)
                    .HasMaxLength(50)
                    .HasColumnName("customer_country");

                entity.Property(e => e.CustomerCountrySubentity)
                    .HasMaxLength(50)
                    .HasColumnName("customer_country_subentity");

                entity.Property(e => e.CustomerCrn)
                    .HasMaxLength(50)
                    .HasColumnName("customer_crn");

                entity.Property(e => e.CustomerId).HasColumnName("customer_id");

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(200)
                    .HasColumnName("customer_name");

                entity.Property(e => e.CustomerNat)
                    .HasMaxLength(50)
                    .HasColumnName("customer_nat");

                entity.Property(e => e.CustomerNotes)
                    .HasMaxLength(200)
                    .HasColumnName("customer_notes");

                entity.Property(e => e.CustomerPhone)
                    .HasMaxLength(50)
                    .HasColumnName("customer_phone");

                entity.Property(e => e.CustomerPlotIdentification)
                    .HasMaxLength(50)
                    .HasColumnName("customer_plot_Identification");

                entity.Property(e => e.CustomerPostalZone)
                    .HasMaxLength(50)
                    .HasColumnName("customer_postal_zone");

                entity.Property(e => e.CustomerRegistrationName)
                    .HasMaxLength(50)
                    .HasColumnName("customer_registration_name");

                entity.Property(e => e.CustomerStreetName)
                    .HasMaxLength(50)
                    .HasColumnName("customer_street_name");

                entity.Property(e => e.CustomerTin)
                    .HasMaxLength(50)
                    .HasColumnName("customer_tin");

                entity.Property(e => e.CustomerVatNo)
                    .HasMaxLength(50)
                    .HasColumnName("customer_vat_no");

                entity.Property(e => e.DeviceId)
                    .HasMaxLength(20)
                    .HasColumnName("device_id");

                entity.Property(e => e.DiningOptionId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("dining_option_id");

                entity.Property(e => e.DiscountId)
                    .HasMaxLength(20)
                    .HasColumnName("discount_id");

                entity.Property(e => e.DiscountTypeId)
                    .HasMaxLength(20)
                    .HasColumnName("discount_type_id");

                entity.Property(e => e.DiscountValue)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("discount_value");

                entity.Property(e => e.FeesTotal)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fees_total");

                entity.Property(e => e.GuestCount).HasColumnName("guest_count");

                entity.Property(e => e.HeaderDiscountAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("header_discount_amount");

                entity.Property(e => e.InvoiceNumber).HasColumnName("invoice_number");

                entity.Property(e => e.IsReturn).HasColumnName("is_return");

                entity.Property(e => e.LineTotal)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("line_total");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Note).HasColumnName("note");

                entity.Property(e => e.OrderNumber).HasColumnName("order_number");

                entity.Property(e => e.OrderSourceId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("order_source_id");

                entity.Property(e => e.OrderStatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("order_status_id");

                entity.Property(e => e.PaidAt)
                    .HasColumnType("datetime")
                    .HasColumnName("paid_at");

                entity.Property(e => e.PaidBy)
                    .HasMaxLength(20)
                    .HasColumnName("paid_by");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.ServerId).HasColumnName("server_id");

                entity.Property(e => e.SupplierBuildingNumber)
                    .HasMaxLength(50)
                    .HasColumnName("supplier_building_number");

                entity.Property(e => e.SupplierCityName)
                    .HasMaxLength(50)
                    .HasColumnName("supplier_city_name");

                entity.Property(e => e.SupplierCitySubdivisionName)
                    .HasMaxLength(50)
                    .HasColumnName("supplier_city_subdivision_name");

                entity.Property(e => e.SupplierCountry)
                    .HasMaxLength(50)
                    .HasColumnName("supplier_country");

                entity.Property(e => e.SupplierCountrySubentity)
                    .HasMaxLength(50)
                    .HasColumnName("supplier_country_subentity");

                entity.Property(e => e.SupplierCrn)
                    .HasMaxLength(50)
                    .HasColumnName("supplier_crn");

                entity.Property(e => e.SupplierPlotIdentification)
                    .HasMaxLength(50)
                    .HasColumnName("supplier_plot_Identification");

                entity.Property(e => e.SupplierPostalZone)
                    .HasMaxLength(50)
                    .HasColumnName("supplier_postal_zone");

                entity.Property(e => e.SupplierRegistrationName)
                    .HasMaxLength(50)
                    .HasColumnName("supplier_registration_name");

                entity.Property(e => e.SupplierStreetName)
                    .HasMaxLength(50)
                    .HasColumnName("supplier_street_name");

                entity.Property(e => e.SupplierTin)
                    .HasMaxLength(50)
                    .HasColumnName("supplier_tin");

                entity.Property(e => e.SupplierVatNo)
                    .HasMaxLength(50)
                    .HasColumnName("supplier_vat_no");

                entity.Property(e => e.Total)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("total");

                entity.Property(e => e.TotalVat)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("total_vat");

                entity.Property(e => e.VatReportStatus)
                    .HasMaxLength(50)
                    .HasColumnName("vat_report_status")
                    .HasComment("pending, reported, failed - mark reporting to failed after sometime");

                entity.Property(e => e.VoidAt)
                    .HasColumnType("datetime")
                    .HasColumnName("void_at");

                entity.Property(e => e.VoidBy)
                    .HasMaxLength(20)
                    .HasColumnName("void_by");

                entity.Property(e => e.VoidReasonId)
                    .HasMaxLength(20)
                    .HasColumnName("void_reason_id");

                entity.Property(e => e.VoidTypeId)
                    .HasMaxLength(20)
                    .HasColumnName("void_type_id");


                entity.Property(e => e.WaiterId)
                    .HasMaxLength(20)
                    .HasColumnName("waiter_id");

                entity.Property(e => e.WorkDayId).HasColumnName("work_day_id");

                entity.Property(e => e.WorkDayShiftId).HasColumnName("work_day_shift_id");

                entity.Property(e => e.ZatcaInvoiceCounter).HasColumnName("zatca_invoice_counter");

                entity.Property(e => e.ZatcaInvoiceHash).HasColumnName("zatca_invoice_hash");

                entity.Property(e => e.ZatcaQrCode).HasColumnName("zatca_qr_code");

                entity.Property(e => e.ZatcaReportStatus)
                    .HasMaxLength(50)
                    .HasColumnName("zatca_report_status");

                entity.Property(e => e.ZatcaResponseJson).HasColumnName("zatca_response_json");

                entity.Property(e => e.ZatcaSignedXmlInvoice).HasColumnName("zatca_signed_xml_invoice");

                entity.HasOne(d => d.AppOrderStatusNavigation)
                    .WithMany(p => p.OrderHeaders)
                    .HasForeignKey(d => d.AppOrderStatus)
                    .HasConstraintName("FK_order_header_api_order_staging_status");

                entity.HasOne(d => d.Area)
                    .WithMany(p => p.OrderHeaders)
                    .HasForeignKey(d => d.AreaId)
                    .HasConstraintName("FK_order_header_area");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.OrderHeaders)
                    .HasForeignKey(d => d.BranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_header_branch");

                entity.HasOne(d => d.CloseByNavigation)
                    .WithMany(p => p.OrderHeaderCloseByNavigations)
                    .HasForeignKey(d => d.CloseBy)
                    .HasConstraintName("FK_order_header_employee");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.OrderHeaderCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.OrderHeaders)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_order_header_customer");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.OrderHeaders)
                    .HasForeignKey(d => d.DeviceId)
                    .HasConstraintName("FK_order_header_pos_device");

                entity.HasOne(d => d.DiningOption)
                    .WithMany(p => p.OrderHeaders)
                    .HasForeignKey(d => d.DiningOptionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_header_dining_option");

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.OrderHeaders)
                    .HasForeignKey(d => d.DiscountId)
                    .HasConstraintName("FK_order_header_discount");

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.OrderHeaderModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.OrderSource)
                    .WithMany(p => p.OrderHeaders)
                    .HasForeignKey(d => d.OrderSourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_header_order_source");

                entity.HasOne(d => d.OrderStatus)
                    .WithMany(p => p.OrderHeaders)
                    .HasForeignKey(d => d.OrderStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_header_order_status");

                entity.HasOne(d => d.PaidByNavigation)
                    .WithMany(p => p.OrderHeaderPaidByNavigations)
                    .HasForeignKey(d => d.PaidBy);

                entity.HasOne(d => d.VoidByNavigation)
                    .WithMany(p => p.OrderHeaderVoidByNavigations)
                    .HasForeignKey(d => d.VoidBy);

                entity.HasOne(d => d.VoidReason)
                    .WithMany(p => p.OrderHeaders)
                    .HasForeignKey(d => d.VoidReasonId)
                    .HasConstraintName("FK_order_header_void_reason");

                entity.HasOne(d => d.VoidType)
                    .WithMany(p => p.OrderHeaders)
                    .HasForeignKey(d => d.VoidTypeId)
                    .HasConstraintName("FK_order_header_void_type");

                entity.HasOne(d => d.Waiter)
                    .WithMany(p => p.OrderHeaderWaiters)
                    .HasForeignKey(d => d.WaiterId);

                entity.HasOne(d => d.WorkDay)
                    .WithMany(p => p.OrderHeaders)
                    .HasForeignKey(d => d.WorkDayId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_header_work_day");

                entity.HasOne(d => d.WorkDayShift)
                    .WithMany(p => p.OrderHeaders)
                    .HasForeignKey(d => d.WorkDayShiftId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_order_header_work_day_shift");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_item", "trn");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.DiscountAmount)
                    .HasColumnType("decimal(8, 2)")
                    .HasColumnName("discount_amount");

                entity.Property(e => e.DiscountId)
                    .HasMaxLength(20)
                    .HasColumnName("discount_id");

                entity.Property(e => e.DiscountTypeId)
                    .HasMaxLength(20)
                    .HasColumnName("discount_type_id");

                entity.Property(e => e.DiscountValue)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("discount_value");

                entity.Property(e => e.HeaderDiscountAmount)
                    .HasColumnType("decimal(8, 2)")
                    .HasColumnName("header_discount_amount");

                entity.Property(e => e.ItemDescription)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("item_description");

                entity.Property(e => e.ItemId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("item_id");

                entity.Property(e => e.KotPrinted).HasColumnName("kot_printed");

                entity.Property(e => e.MenuGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("menu_group_id");

                entity.Property(e => e.Modifier).HasColumnName("modifier");

                entity.Property(e => e.ModifierGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("modifier_group_id");

                entity.Property(e => e.ModifierParent).HasColumnName("modifier_parent");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Note).HasColumnName("note");

                entity.Property(e => e.OrderHeaderId).HasColumnName("order_header_id");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(8, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.PriceVatInclusive).HasColumnName("price_vat_inclusive");

                entity.Property(e => e.Quantity)
                    .HasColumnType("decimal(8, 3)")
                    .HasColumnName("quantity");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.Total)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("total");

                entity.Property(e => e.VariantId)
                    .HasMaxLength(20)
                    .HasColumnName("variant_id");

                entity.Property(e => e.VatAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("vat_amount");

                entity.Property(e => e.VatGroupId)
                    .HasMaxLength(20)
                    .HasColumnName("vat_group_id");

                entity.Property(e => e.VatPercentage)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("vat_percentage");

                entity.Property(e => e.Void).HasColumnName("void");

                entity.Property(e => e.VoidAt)
                    .HasColumnType("datetime")
                    .HasColumnName("void_at");

                entity.Property(e => e.VoidBy)
                    .HasMaxLength(20)
                    .HasColumnName("void_by");

                entity.Property(e => e.VoidTypeId)
                    .HasMaxLength(20)
                    .HasColumnName("void_type_id");

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.DiscountId)
                    .HasConstraintName("FK_order_item_discount");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.OrderItemCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.DiscountId)
                    .HasConstraintName("FK_order_item_discount");

                entity.HasOne(d => d.DiscountType)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.DiscountTypeId)
                    .HasConstraintName("FK_order_item_discount_type");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.OrderItemItems)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_item_item_order_item");

                entity.HasOne(d => d.ModifierGroup)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.ModifierGroupId)
                    .HasConstraintName("FK_order_item_modifier_group");

                entity.HasOne(d => d.ModifierParentNavigation)
                    .WithMany(p => p.InverseModifierParentNavigation)
                    .HasForeignKey(d => d.ModifierParent)
                    .HasConstraintName("FK_order_item_modifier_parent");

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.OrderItemModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.OrderHeader)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.OrderHeaderId)
                    .HasConstraintName("FK_order_item_order_header");

                entity.HasOne(d => d.Variant)
                    .WithMany(p => p.OrderItemVariants)
                    .HasForeignKey(d => d.VariantId)
                    .HasConstraintName("FK_order_item_variant");

                entity.HasOne(d => d.VatGroup)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.VatGroupId)
                    .HasConstraintName("FK_order_item_vat_group");

                entity.HasOne(d => d.VoidByNavigation)
                    .WithMany(p => p.OrderItemVoidByNavigations)
                    .HasForeignKey(d => d.VoidBy);

                entity.HasOne(d => d.VoidType)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.VoidTypeId)
                    .HasConstraintName("FK_order_item_void_type");
            });

            modelBuilder.Entity<OrderItemCustomerComment>(entity =>
            {
                entity.HasKey(e => new { e.OrderItemId, e.CustomerCommentId });

                entity.ToTable("order_item_customer_comment", "trn");

                entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");

                entity.Property(e => e.CustomerCommentId)
                    .HasMaxLength(20)
                    .HasColumnName("customer_comment_id");

                entity.HasOne(d => d.CustomerComment)
                    .WithMany(p => p.OrderItemCustomerComments)
                    .HasForeignKey(d => d.CustomerCommentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_item_customer_comment_customer_comment");

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.OrderItemCustomerComments)
                    .HasForeignKey(d => d.OrderItemId)
                    .HasConstraintName("FK_order_item_customer_comment_order_item");
            });

            modelBuilder.Entity<OrderPayment>(entity =>
            {
                entity.HasKey(e => new { e.OrderHeaderId, e.LineIndex });

                entity.ToTable("order_payment", "trn");

                entity.Property(e => e.OrderHeaderId).HasColumnName("order_header_id");

                entity.Property(e => e.LineIndex).HasColumnName("line_index");

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(8, 2)")
                    .HasColumnName("amount");

                entity.Property(e => e.PaymentId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("payment_id");

                entity.Property(e => e.PaymentTypeDescription)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("payment_type_description");

                entity.Property(e => e.ReferenceNumber)
                    .HasMaxLength(200)
                    .HasColumnName("reference_number");

                entity.HasOne(d => d.OrderHeader)
                    .WithMany(p => p.OrderPayments)
                    .HasForeignKey(d => d.OrderHeaderId)
                    .HasConstraintName("FK_order_payment_order_header");

                entity.HasOne(d => d.Payment)
                    .WithMany(p => p.OrderPayments)
                    .HasForeignKey(d => d.PaymentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_payment_payment");
            });

            modelBuilder.Entity<OrderSource>(entity =>
            {
                entity.ToTable("order_source", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.OrderIndex).HasColumnName("order_index");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id")
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.OrderSources)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_order_source_status");
            });

            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.ToTable("order_status", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.OrderIndex).HasColumnName("order_index");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payment", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.OpenCashDrawer).HasColumnName("open_cash_drawer");

                entity.Property(e => e.PaymentTypeId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("payment_type_id");

                entity.Property(e => e.RequireReferenceNumber).HasColumnName("require_reference_number");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id")
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.PaymentCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.PaymentModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.PaymentType)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.PaymentTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_payment_payment_type");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_payment_status");
            });

            modelBuilder.Entity<PaymentType>(entity =>
            {
                entity.ToTable("payment_type", "def");

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

            modelBuilder.Entity<Perm>(entity =>
            {
                entity.ToTable("perm", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.OrderIndex).HasColumnName("order_index");

                entity.Property(e => e.PermGroupId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("perm_group_id");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.HasOne(d => d.PermGroup)
                    .WithMany(p => p.Perms)
                    .HasForeignKey(d => d.PermGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_perm_perm_group");
            });

            modelBuilder.Entity<PermGroup>(entity =>
            {
                entity.ToTable("perm_group", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("description")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Icon)
                    .HasMaxLength(20)
                    .HasColumnName("icon");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sdescription)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("sdescription")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<PermRole>(entity =>
            {
                entity.ToTable("perm_role", "def");

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

            modelBuilder.Entity<ReceiptSetting>(entity =>
            {
                entity.HasKey(e => e.BranchId)
                    .HasName("PK_receipt_settings");

                entity.ToTable("receipt_setting", "def");

                entity.Property(e => e.BranchId)
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.Footer)
                    .IsRequired()
                    .HasColumnName("footer")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Header)
                    .IsRequired()
                    .HasColumnName("header")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ImageName).HasColumnName("image_name");

                entity.Property(e => e.ImagePath).HasColumnName("image_path");

                entity.Property(e => e.ShowCustomerComment).HasColumnName("show_customer_comment");

                entity.Property(e => e.ShowCustomerInfo).HasColumnName("show_customer_info");

                entity.HasOne(d => d.Branch)
                    .WithOne(p => p.ReceiptSetting)
                    .HasForeignKey<ReceiptSetting>(d => d.BranchId)
                    .HasConstraintName("FK_receipt_setting_branch");
            });


            modelBuilder.Entity<PermRolePerm>(entity =>
            {
                entity.HasKey(e => new { e.PermRoleId, e.PermId });

                entity.ToTable("perm_role_perm", "def");

                entity.Property(e => e.PermRoleId)
                    .HasMaxLength(20)
                    .HasColumnName("perm_role_id");

                entity.Property(e => e.PermId)
                    .HasMaxLength(20)
                    .HasColumnName("perm_id");

                entity.HasOne(d => d.Perm)
                    .WithMany(p => p.PermRolePerms)
                    .HasForeignKey(d => d.PermId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_perm_role_perm_perm");

                entity.HasOne(d => d.PermRole)
                    .WithMany(p => p.PermRolePerms)
                    .HasForeignKey(d => d.PermRoleId)
                    .HasConstraintName("FK_perm_role_perm_perm_role");
            });

            modelBuilder.Entity<PosDevice>(entity =>
            {
                entity.ToTable("pos_device", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.ActivationCode)
                    .HasMaxLength(10)
                    .HasColumnName("activation_code");

                entity.Property(e => e.AutoPrint).HasColumnName("auto_print");

                entity.Property(e => e.BranchId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.DefaultAreaId)
                    .HasMaxLength(20)
                    .HasColumnName("default_area_id");

                entity.Property(e => e.DefaultPrintCount)
                    .HasColumnName("default_print_count")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.DiningOptionId)
                    .HasMaxLength(20)
                    .HasColumnName("dining_option_id");

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(50)
                    .HasColumnName("ip_address");

                entity.Property(e => e.MasterId)
                    .HasMaxLength(20)
                    .HasColumnName("master_id");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.OrderNumberEnd).HasColumnName("order_number_end");

                entity.Property(e => e.OrderNumberStart)
                    .HasColumnName("order_number_start")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.OrderSourceId)
                    .HasMaxLength(20)
                    .HasColumnName("order_source_id");

                entity.Property(e => e.PaymentAppId)
                    .HasMaxLength(20)
                    .HasColumnName("payment_app_id");

                entity.Property(e => e.PaymentTerminalPortIp)
                    .HasMaxLength(20)
                    .HasColumnName("payment_terminal_port_ip");

                entity.Property(e => e.PrintLanguage).HasColumnName("print_language");

                entity.Property(e => e.PrintOnKitchen)
                    .IsRequired()
                    .HasColumnName("print_on_kitchen")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ReceiveOnlineOrder).HasColumnName("receive_online_order");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("status_id")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .HasColumnName("type");

                entity.Property(e => e.ZatcaApiCsid).HasColumnName("zatca_api_csid");

                entity.Property(e => e.ZatcaApiCsr).HasColumnName("zatca_api_csr");

                entity.Property(e => e.ZatcaApiDeviceId).HasColumnName("zatca_api_device_id");

                entity.Property(e => e.ZatcaApiPrivateKey).HasColumnName("zatca_api_private_key");

                entity.Property(e => e.ZatcaApiSecret).HasColumnName("zatca_api_secret");

                entity.Property(e => e.ZatcaPhase)
                    .HasColumnName("zatca_phase")
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.PosDevices)
                    .HasForeignKey(d => d.BranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_pos_device_branch");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.PosDeviceCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.DefaultArea)
                    .WithMany(p => p.PosDevices)
                    .HasForeignKey(d => d.DefaultAreaId)
                    .HasConstraintName("fk_posdevice_default_area");

                entity.HasOne(d => d.DiningOption)
                    .WithMany(p => p.PosDevices)
                    .HasForeignKey(d => d.DiningOptionId)
                    .HasConstraintName("FK_pos_device_dining_option");

                entity.HasOne(d => d.Master)
                    .WithMany(p => p.InverseMaster)
                    .HasForeignKey(d => d.MasterId)
                    .HasConstraintName("FK_pos_device_pos_device");

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.PosDeviceModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);

                entity.HasOne(d => d.OrderSource)
                    .WithMany(p => p.PosDevices)
                    .HasForeignKey(d => d.OrderSourceId)
                    .HasConstraintName("FK_pos_device_order_source");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.PosDevices)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_pos_device_status");
            });

            modelBuilder.Entity<PosDeviceMenu>(entity =>
            {
                entity.HasKey(e => new { e.PosDeviceId, e.MenuId });

                entity.ToTable("pos_device_menu", "def");

                entity.Property(e => e.PosDeviceId)
                    .HasMaxLength(20)
                    .HasColumnName("pos_device_id");

                entity.Property(e => e.MenuId)
                    .HasMaxLength(20)
                    .HasColumnName("menu_id");

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.PosDeviceMenus)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("FK_pos_device_menu_menu");

                entity.HasOne(d => d.PosDevice)
                    .WithMany(p => p.PosDeviceMenus)
                    .HasForeignKey(d => d.PosDeviceId)
                    .HasConstraintName("FK_pos_device_menu_pos_device");
            });

            modelBuilder.Entity<PosPrinter>(entity =>
            {
                entity.ToTable("pos_printer", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.BranchId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.Model)
                    .HasMaxLength(20)
                    .HasColumnName("model");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.PrinterName)
                    .HasMaxLength(50)
                    .HasColumnName("printer_name");

                entity.Property(e => e.Sname)
                    .HasMaxLength(50)
                    .HasColumnName("sname");

                entity.Property(e => e.TcpIp)
                    .HasMaxLength(50)
                    .HasColumnName("tcp_ip");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.PosPrinters)
                    .HasForeignKey(d => d.BranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_pos_printer_branch");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.PosPrinterCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.PosPrinterModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);
            });

            modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("status", "def");

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

            modelBuilder.Entity<SysFeature>(entity =>
            {
                entity.ToTable("sys_feature", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("description");

                entity.Property(e => e.Icon)
                    .HasMaxLength(100)
                    .HasColumnName("icon");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sdescription)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("sdescription")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<VatGroup>(entity =>
            {
                entity.ToTable("vat_group", "def");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("id");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.ModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modify_at");

                entity.Property(e => e.ModifyBy)
                    .HasMaxLength(20)
                    .HasColumnName("modify_by");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Percentage).HasColumnName("percentage");

                entity.Property(e => e.Sname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("sname")
                    .HasDefaultValueSql("('')");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.VatGroupCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ModifyByNavigation)
                    .WithMany(p => p.VatGroupModifyByNavigations)
                    .HasForeignKey(d => d.ModifyBy);
            });

            modelBuilder.Entity<VoidReason>(entity =>
            {
                entity.ToTable("void_reason", "def");

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

            modelBuilder.Entity<VoidType>(entity =>
            {
                entity.ToTable("void_type", "def");

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

            modelBuilder.Entity<WorkDay>(entity =>
            {
                entity.ToTable("work_day", "trn");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.BranchId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("branch_id");

                entity.Property(e => e.CloseAt)
                    .HasColumnType("datetime")
                    .HasColumnName("close_at");

                entity.Property(e => e.CloseBy)
                    .HasMaxLength(20)
                    .HasColumnName("close_by");

                entity.Property(e => e.Date)
                    .HasColumnType("date")
                    .HasColumnName("date");

                entity.Property(e => e.OpenAt)
                    .HasColumnType("datetime")
                    .HasColumnName("open_at")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.OpenBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("open_by");

                entity.Property(e => e.OperationData).HasColumnName("operation_data");

                entity.Property(e => e.OperationIsRunning).HasColumnName("operation_is_running");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.ServerId).HasColumnName("server_id");

                entity.Property(e => e.WorkShiftOn).HasColumnName("work_shift_on");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.WorkDays)
                    .HasForeignKey(d => d.BranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_work_day_branch");

                entity.HasOne(d => d.CloseByNavigation)
                    .WithMany(p => p.WorkDayCloseByNavigations)
                    .HasForeignKey(d => d.CloseBy);

                entity.HasOne(d => d.OpenByNavigation)
                    .WithMany(p => p.WorkDayOpenByNavigations)
                    .HasForeignKey(d => d.OpenBy)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<WorkDayShift>(entity =>
            {
                entity.ToTable("work_day_shift", "trn");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CloseCashAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("close_cash_amount");

                entity.Property(e => e.ClosedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("closed_at");

                entity.Property(e => e.ClosedBy)
                    .HasMaxLength(20)
                    .HasColumnName("closed_by");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at");

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("employee_id");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.ServerId).HasColumnName("server_id");

                entity.Property(e => e.ShiftCashAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("shift_cash_amount");

                entity.Property(e => e.StartingCashAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("starting_cash_amount");

                entity.Property(e => e.VarianceCashAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("variance_cash_amount");

                entity.Property(e => e.WorkDayId).HasColumnName("work_day_id");

                entity.HasOne(d => d.ClosedByNavigation)
                    .WithMany(p => p.WorkDayShiftClosedByNavigations)
                    .HasForeignKey(d => d.ClosedBy)
                    .HasConstraintName("FK_work_day_shift_close_employee");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.WorkDayShiftEmployees)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_work_day_shift_employee");

                entity.HasOne(d => d.WorkDay)
                    .WithMany(p => p.WorkDayShifts)
                    .HasForeignKey(d => d.WorkDayId)
                    .HasConstraintName("FK_work_day_shift_work_day");
            });

            modelBuilder.Entity<WorkDayShiftCashTransaction>(entity =>
            {
                entity.ToTable("work_day_shift_cash_transaction", "trn");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("amount");

                entity.Property(e => e.Comment)
                    .IsRequired()
                    .HasColumnName("comment");

                entity.Property(e => e.CreateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("create_at");

                entity.Property(e => e.CreateBy)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("create_by");

                entity.Property(e => e.RowVersion)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("row_version");

                entity.Property(e => e.ServerId).HasColumnName("server_id");

                entity.Property(e => e.WorkDayShiftId).HasColumnName("work_day_shift_id");

                entity.HasOne(d => d.WorkDayShift)
                    .WithMany(p => p.WorkDayShiftCashTransactions)
                    .HasForeignKey(d => d.WorkDayShiftId)
                    .HasConstraintName("FK_work_day_shift_cash_transaction_work_day_shift");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
