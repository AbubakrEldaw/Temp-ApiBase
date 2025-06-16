using APIBase.Models.Enums;
using APIBase.Models.POS;

namespace APIBase.Models.CustomModels
{
    public class ReportsRequest
    {
        public string[] BranchesIds { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string[] ItemGroupsIds { get; set; }
        public string[] OrderSourcesIds { get; set; }

        public ReportsRequest(string branches, DateTime from, DateTime to, string itemGroups, string orderSources)
        {
            if (from > To)
            {
                throw new Exception();
            }

            From = from;
            To = to;

            BranchesIds = string.IsNullOrEmpty(branches) ? [] : branches.Split(",");
            ItemGroupsIds = string.IsNullOrEmpty(branches) ? [] : branches.Split(",");
            OrderSourcesIds = string.IsNullOrEmpty(branches) ? [] : branches.Split(",");
        }
    }
}