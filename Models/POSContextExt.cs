using System;
using System.Linq;
using System.Security.Claims;
using APIBase.Helpers;
using APIBase.Models.Master;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;

#nullable disable

namespace APIBase.Models.POS
{

    public partial class POSContext : DbContext
    {
        private readonly HttpContext _httpContext;
        private readonly forkpos_masterContext _userDBContext;
        private readonly AppSettings _appSettings;

        private readonly string _companyId;
        public POSContext(IHttpContextAccessor httpContextAccessor, forkpos_masterContext userDBContext, IOptions<AppSettings> appSettings)
        {
            _httpContext = httpContextAccessor?.HttpContext;
            _userDBContext = userDBContext;
            _appSettings = appSettings.Value;
        }

        public POSContext(string companyId, forkpos_masterContext userDBContext, AppSettings appSettings)
        {
            _companyId = companyId;
            _userDBContext = userDBContext;
            _appSettings = appSettings;
        } 

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var companyId = "";
            if (_companyId != null)
            {
                companyId = _companyId;
            }
            else
            {
                var companyClaim = _httpContext?.User.Claims.Where(x => x.Type == "CompanyId").FirstOrDefault();
                if (companyClaim == null)
                {
                    return;
                }
                else
                {
                    companyId = companyClaim.Value;
                }
            }

            var user = companyId != "SubPos" ? "sa" : "SubPos";

            if (!optionsBuilder.IsConfigured)
            {
                var connStr = _userDBContext.ConnStrs.FirstOrDefault(x => x.CompanyId == companyId);
                var cs = string.Format(_appSettings.Client_DB, connStr.ServerName, connStr.InstanceName, connStr.DatabaseName, user, connStr.Password);

                optionsBuilder.UseSqlServer(
                   cs
                    , o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            }
        }
    
    }
}
