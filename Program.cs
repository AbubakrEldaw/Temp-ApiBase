using APIBase.Helpers;
using APIBase.Models.Master;
using APIBase.Models.POS;
using APIBase.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MvcOptions>(options =>
{
    options.MaxModelBindingCollectionSize = 20480; 
});

builder.Host.ConfigureServices((context, services) =>
{
    HostConfig.CertPath = context.Configuration["CertPath"];
    HostConfig.CertPassword = context.Configuration["CertPass"];
});

builder.WebHost.UseKestrel(opt =>
{
    opt.Limits.MinResponseDataRate = null;

    var host = Dns.GetHostEntry(Dns.GetHostName());
    var ipV4Address = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

    if (ipV4Address != null)
    {
        // Listen on the selected IPv4 address
        opt.Listen(ipV4Address, 5081, listOpt =>
        {
            listOpt.UseHttps(HostConfig.CertPath, HostConfig.CertPassword);
        });
    }
    else
    {
        throw new Exception("No IPv4 address found to bind to.");
    }
});

builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddDbContext<forkpos_masterContext>(Options => Options.UseSqlServer(builder.Configuration.GetConnectionString("master")));
builder.Services.AddDbContext<POSContext>();

builder.Services.AddCors();
builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault);

builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.None);
builder.Logging.AddFilter("System.Net.Http.HttpClient.*.LogicalHandler", LogLevel.None);
builder.Logging.AddFilter("System.Net.Http.HttpClient.*.ClientHandler", LogLevel.None);
builder.Services.AddHttpClient();

// configure strongly typed settings objects

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));

var AppSett = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
EncMaster enms = new EncMaster(AppSett);
builder.Services.AddSingleton<IEncMaster>(enms);

// configure DI for application services
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretkey = Encoding.UTF8.GetBytes(AppSett.SecretKey);
        var encryptionkey = Encoding.UTF8.GetBytes(AppSett.TEncryptkey);

        var validationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero, // default: 5 min
            RequireSignedTokens = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretkey),

            RequireExpirationTime = true,
            ValidateLifetime = true,
            ValidateAudience = true, //default : false
            ValidAudience = AppSett.Audience,

            ValidateIssuer = true, //default : false
            ValidIssuer = AppSett.Issuer,

            TokenDecryptionKey = new SymmetricSecurityKey(encryptionkey)
        };

        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = validationParameters;
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs/apiorders")))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});



builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
    options.HttpsPort = 8081;
});

builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                    new HeaderApiVersionReader("x-api-version"),
                                                    new MediaTypeApiVersionReader("x-api-version"));
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

//builder.Services.AddEndpointsApiExplorer();
if (!InvStatus.isProduction)
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "Bearer "
        });

        c.EnableAnnotations();
    });

}

builder.Services.AddSignalR();

var app = builder.Build();
if (!InvStatus.isProduction)
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/swagger/v2/swagger.json",
               ("v2").ToUpperInvariant());
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors(x => x
                //.SetIsOriginAllowed(origin => true)
                .AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod()
               // .AllowCredentials()
               );

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHttpsRedirection();
}

if (string.IsNullOrWhiteSpace(app.Environment.WebRootPath))
{
    app.Environment.WebRootPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
}


app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(
    Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Companies")),
    RequestPath = new PathString("/c")
});

//app.UseCors("CorsPolicy");
app.UseRouting();
// global cors policy


app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    //endpoints.MapAreaControllerRoute(
    //        name: "adminapi",
    //        areaName: "adminapi",
    //        pattern: "{controller=Home}/{action=Index}/{id?}"
    //    ).RequireHost("localhost:5000", "forkpos.com");
    endpoints.MapControllers();
});

app.Run();



public static class HostConfig
{
    public static string CertPath { get; set; }
    public static string CertPassword { get; set; }
}

public static class InvStatus
{
    public static bool isProduction { get; set; } = false;
}

