using System;
using System.Collections.Generic;

#nullable disable

namespace APIBase.Models.POS
{
    public partial class Company
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Address { get; set; }
        public bool? SalePriceInclusiveOfVat { get; set; }
        public string Currency { get; set; }
        public string TimeZone { get; set; }
        public bool EnableSname { get; set; }
        public string Logo { get; set; }
        public string Crn { get; set; }
        public string Tin { get; set; }
        public string VatRegNo { get; set; }
        public string RegistrationName { get; set; }
        public string StreetName { get; set; }
        public string BuildingNumber { get; set; }
        public string PlotIdentification { get; set; }
        public string CitySubdivisionName { get; set; }
        public string CityName { get; set; }
        public string PostalZone { get; set; }
        public string CountrySubentity { get; set; }
        public string Country { get; set; }
    }
}
