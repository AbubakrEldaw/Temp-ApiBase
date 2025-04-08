using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.ReportsModels
{
    public class DatatableRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string Id { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string SearchValue { get; set; }
        public string SearchRegex { get; set; }
        public string? Branches { get; set; }
        public string? Status { get; set; }
        public string? Discounts { get; set; }
        public string? CustomerGroups { get; set; }
    }

    public class DatatableResponse
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public dynamic Data { get; set; }
    }
}
