using APIBase.Models.POS;

namespace APIBase.Models.ReportsModels
{
    public class CustomerReport
    {
        public Guid Id { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public int TotalVisits { get; set; }
        public decimal? TotalSpent { get; set; }
        public decimal TotalDiscount { get; set; }
        public DateTime? FirstVisit { get; set; }
        public DateTime? LastVisit { get; set; }
        public Employee CreateByNavigation { get; set; }
        public DateTime CreateAt { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }
    }
}
