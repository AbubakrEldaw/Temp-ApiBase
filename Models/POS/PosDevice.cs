using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class PosDevice
    {
        public PosDevice()
        {
            InverseMaster = new HashSet<PosDevice>();
            OrderHeaders = new HashSet<OrderHeader>();
            PosDeviceMenus = new HashSet<PosDeviceMenu>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string BranchId { get; set; }
        public string Type { get; set; }
        public string StatusId { get; set; }
        public string MasterId { get; set; }
        public string IpAddress { get; set; }
        public bool AutoPrint { get; set; }
        public int OrderNumberStart { get; set; }
        public int? OrderNumberEnd { get; set; }
        public string OrderSourceId { get; set; }
        public string DiningOptionId { get; set; }
        public int DefaultPrintCount { get; set; }
        public int PrintLanguage { get; set; }
        public bool? PrintOnKitchen { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }
        public string DefaultAreaId { get; set; }
        public string ActivationCode { get; set; }
        public bool ReceiveOnlineOrder { get; set; }
        public string PaymentAppId { get; set; }
        public string PaymentTerminalPortIp { get; set; }
        public Guid? ZatcaApiDeviceId { get; set; }
        public string ZatcaApiCsid { get; set; }
        public string ZatcaApiPrivateKey { get; set; }
        public string ZatcaApiSecret { get; set; }
        public string ZatcaApiCsr { get; set; }
        public int? ZatcaPhase { get; set; }

        public virtual Branch Branch { get; set; }
        public virtual Employee CreateByNavigation { get; set; }
        public virtual Area DefaultArea { get; set; }
        public virtual DiningOption DiningOption { get; set; }
        public virtual PosDevice Master { get; set; }
        public virtual Employee ModifyByNavigation { get; set; }
        public virtual OrderSource OrderSource { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<PosDevice> InverseMaster { get; set; }
        public virtual ICollection<OrderHeader> OrderHeaders { get; set; }
        public virtual ICollection<PosDeviceMenu> PosDeviceMenus { get; set; }
    }
}
