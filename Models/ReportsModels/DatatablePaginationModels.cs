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
        public string? Branches { get; set; }
        public string? Status { get; set; }
        public HashSet<string> Discounts { get; set; }
        public HashSet<string> CustomerGroups { get; set; }
        public Search Search { get; set; }
        public List<Order> Order { get; set; }
        public List<Column> Columns { get; set; }
    }

    public class Search
    {
        public string Value { get; set; }
        public string Regex { get; set; }
    }


    public class DatatableResponse
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public dynamic Data { get; set; }
    }

    public class DatatableAPIRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string Id { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public HashSet<string> Branches { get; set; }
        public HashSet<string> Status { get; set; }
        public HashSet<string> Discounts { get; set; }
        public HashSet<string> CustomerGroups { get; set; }
        public Search Search { get; set; }
        public List<Order> Order { get; set; }
        public List<Column> Columns { get; set; }
    }

    public class Column
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public string Searchable { get; set; }
        public string Orderable { get; set; }
    }

    public class Order
    {
        public int Column { get; set; }
        public string Dir { get; set; }
    }
}
