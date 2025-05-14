using APIBase.Helpers;
using APIBase.Models.Master;
using APIBase.PublicAPIModels;
using APIBase.PublicAPIModels.OAuth;
using APIBase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APIBase.Controllers.v2;
[Route("public/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
public class OauthController : ControllerBase
{
    private IUserService _userService;
    private IEncMaster _encMaster;
    private forkpos_masterContext _MasterContext;
    private readonly AppSettings _appSettings;
    public OauthController(forkpos_masterContext MasterContext, IUserService userService, IEncMaster encMaster, IOptions<AppSettings> appSettings)
    {
        _MasterContext = MasterContext;
        _encMaster = encMaster;
        _userService = userService;
        _appSettings = appSettings.Value;
    }

    [HttpPost("Token")]
    [Tags("Authentication")]
    [AllowAnonymous]
    [MapToApiVersion("2.0")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, StatusCode = 200, Type = typeof(TokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BasicError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(BasicError))]
    public async Task<IActionResult> Token([FromBody] TokenRequest model)
    {
        var mpAPP = await _MasterContext.MarketPlaceApps.Where(x => x.ClientId == model.ClientId && x.ClientSecret == model.ClientSecret && x.Status == "a").FirstOrDefaultAsync();

        if (mpAPP != null)
        {
            // authentication successful so generate jwt and refresh tokens
            var jwtToken = generateJwtTokenForAppAsync(mpAPP);
            var refreshToken = generateRefreshTokenForApp(GetIpAddress());

            // save refresh token
            mpAPP.MarketPlaceAppRefreshTokens.Add(refreshToken);
            _MasterContext.SaveChanges();
            return Ok(new TokenResponse() { Token = jwtToken.token, ExpireAtUTC = jwtToken.ExpireAt.ToString("O"), RefreshToken = refreshToken.Token, RefreshTokenExpireAtUTC = refreshToken.Expires.ToString("O") });
        }
        else
        {
            return Unauthorized(new BasicError() { Error = "RefreshToken Error", ErrorDescription = "App is Unauthorized" });
        }
    }

    [HttpPost("RefreshToken")]
    [Tags("Authentication")]
    [AllowAnonymous]
    [MapToApiVersion("2.0")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, StatusCode = 200, Type = typeof(TokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BasicError))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(BasicError))]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest RefreshTokenRequest)
    {
        try
        {
            var mpAppToken = _MasterContext.MarketPlaceAppRefreshTokens.Where(x => x.Token == RefreshTokenRequest.RefreshToken).FirstOrDefault();

            // return null if no user found with token
            if (mpAppToken == null) return Unauthorized(new BasicError() { Error = "RefreshToken Error", ErrorDescription = "Unauthorized" });

            // return null if token is no longer active
            if (!mpAppToken.IsActive)
            {
                Console.WriteLine("Market Place App Refresh Token is expired:" + RefreshTokenRequest.RefreshToken);
                return BadRequest(new BasicError() { Error = "RefreshToken Error", ErrorDescription = "Refresh Token is expired" });
            }

            // replace old refresh token with a new one and save
            var newRefreshToken = generateRefreshTokenForApp(GetIpAddress());

            mpAppToken.Revoked = DateTime.UtcNow;
            mpAppToken.RevokedByIp = GetIpAddress();
            mpAppToken.ReplacedByToken = newRefreshToken.Token;
            _MasterContext.SaveChanges();

            var mpApp = await _MasterContext.MarketPlaceApps.Where(x => x.Id == mpAppToken.MarketPlaceAppId).FirstAsync();
            // generate new jwt
            var jwtToken = generateJwtTokenForAppAsync(mpApp);
            return Ok(new TokenResponse() { Token = jwtToken.token, ExpireAtUTC = jwtToken.ExpireAt.ToString("O"), RefreshToken = newRefreshToken.Token, RefreshTokenExpireAtUTC = newRefreshToken.Expires.ToString("O") });

        }
        catch (Exception)
        {
            return BadRequest(new BasicError() { Error = "RefreshToken Error", ErrorDescription = "Unknown Error!" });
        }
    }

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        else
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "";
    }

    private (string token, DateTime ExpireAt) generateJwtTokenForAppAsync(MarketPlaceApp mpApp)
    {
        var secretKey = Encoding.UTF8.GetBytes(_appSettings.SecretKey); // longer that 16 character
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature);

        var encryptionkey = Encoding.UTF8.GetBytes(_appSettings.TEncryptkey); //must be 16 character
        var encryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(encryptionkey), SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);

        var claims = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, mpApp.Name),
                    new Claim("MarketPlaceAppId", mpApp.Id),
                });

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "MyWebsite",
            Audience = "MyWebsite",
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow.AddMinutes(_appSettings.NotBeforeMinutes),
            Expires = DateTime.UtcNow.AddMinutes(_appSettings.ExpirationMinutes),
            SigningCredentials = signingCredentials,
            EncryptingCredentials = encryptingCredentials,
            Subject = claims
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var securityToken = tokenHandler.CreateToken(descriptor);

        var encryptedJwt = tokenHandler.WriteToken(securityToken);

        return (encryptedJwt, descriptor.Expires.Value);
    }

    private MarketPlaceAppRefreshToken generateRefreshTokenForApp(string ipAddress)
    {
        using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
        {
            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return new MarketPlaceAppRefreshToken
            {
                Id = Guid.NewGuid(),
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddMinutes(_appSettings.RefreshTokenExpirationMinutes),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }
    }
}

