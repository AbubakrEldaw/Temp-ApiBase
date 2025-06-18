using APIBase.Models.Enums;
using APIBase.Models.POS;

namespace APIBase.Models.CustomModels
{
    public class ReportsRequest
    {
        public string[] BranchesIds { get; set; } = [];
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string[] ItemGroupsIds { get; set; } = [];
        public string[] OrderSourcesIds { get; set; } = [];
    }
}