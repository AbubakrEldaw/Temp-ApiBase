using APIBase.Helpers;
using APIBase.Models;
using APIBase.Models.Master;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APIBase.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
         Task<AuthenticateResponse> RefreshToken(string token, string ipAddress);
        bool RevokeToken(string token, string ipAddress);
        IEnumerable<User> GetAll();
        User GetById(int id);
    }

    public class UserService : IUserService
    {
        private forkpos_masterContext _MasterContext;
        private readonly AppSettings _appSettings;

        public UserService(
            forkpos_masterContext context,
            IOptions<AppSettings> appSettings)
        {
            _MasterContext = context;
            _appSettings = appSettings.Value;
        }
        public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
        {
            var users = _MasterContext.Users.Where(x => x.Username == model.Username && x.Password == model.Password);

            if (users.Count() > 1)
            {
                return null;
            }

            var user = users.FirstOrDefault();
            // return null if user not found
            if (user == null) return null;

            //if (model.PosDeviceId != "")
            //{
            //    POSContext _posContext = new POSContext(user.CompanyId, _MasterContext, _appSettings);
            //    var posdevice = _posContext.PosDevices.FirstOrDefault(x => x.Id == model.PosDeviceId);
            //    if (posdevice != null)
            //    {
            //        if (posdevice.StatusId != "st-active")
            //        {
            //            return null;
            //        }
            //    }
            //    else
            //    {
            //        return null;
            //    }
            //}

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = generateJwtTokenAsync(user, model.PosDeviceId);
            var refreshToken = generateRefreshToken(ipAddress);

            // save refresh token
            user.RefreshTokens.Add(refreshToken);

            _MasterContext.Update(user);
            _MasterContext.SaveChanges();

            return new AuthenticateResponse(user, jwtToken.token, jwtToken.ExpireAt, refreshToken.Token, refreshToken.Expires);
        }

        public async Task<AuthenticateResponse> RefreshToken(string token, string ipAddress)
        {
            var user = await _MasterContext.Users.Include(t => t.RefreshTokens.Where(x => x.Token == token)).FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            // return null if no user found with token
            if (user == null) return null;

            var refreshToken = user.RefreshTokens.FirstOrDefault(x => x.Token == token);
            if (refreshToken == null)
            {
                return null;
            }
            // return null if token is no longer active
            if (!refreshToken.IsActive)
            {
                Console.WriteLine("Refresh Token is expired:" + token);
                return null;
            }

            // replace old refresh token with a new one and save
            var newRefreshToken = generateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);
            _MasterContext.Update(user);
            _MasterContext.SaveChanges();

            // generate new jwt
            var jwtToken = generateJwtTokenAsync(user);

            return new AuthenticateResponse(user, jwtToken.token, jwtToken.ExpireAt, newRefreshToken.Token, newRefreshToken.Expires);
        }

        public bool RevokeToken(string token, string ipAddress)
        {
            var user = _MasterContext.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            // return false if no user found with token
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _MasterContext.Update(user);
            _MasterContext.SaveChanges();

            return true;
        }

        public IEnumerable<User> GetAll()
        {
            return _MasterContext.Users;
        }

        public User GetById(int id)
        {
            return _MasterContext.Users.Find(id);
        }

        private (string token, DateTime ExpireAt) generateJwtTokenAsync(User user, string PosDeviceId = "")
        {

            var secretKey = Encoding.UTF8.GetBytes(_appSettings.SecretKey); // longer that 16 character
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature);

            var encryptionkey = Encoding.UTF8.GetBytes(_appSettings.TEncryptkey); //must be 16 character
            var encryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(encryptionkey), SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);

            var claims = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("CompanyId", user.CompanyId),
                    new Claim("EmployeeId", user.EmployeeId),
                    new Claim("PosDeviceId", PosDeviceId??""),
                    //,new Claim("PublicKey", "ClientPublic")
                });

            //POSContext _posContext = new POSContext(user.CompanyId, _MasterContext, _appSettings);
            //var employee = _posContext.Employees.Include(x => x.PermRole).ThenInclude(x => x.PermRolePerms).AsNoTracking().FirstOrDefault(x => x.Id == user.EmployeeId);
            //if (employee.PermRole != null)
            //{
            //    foreach (var item in employee.PermRole.PermRolePerms)
            //    {
            //        claims.AddClaim(new Claim(ClaimTypes.Role, item.PermId));
            //    }
            //}

            var _plan = _MasterContext.CompanyPlans.Include(x => x.Plan).ThenInclude(x => x.PlanFeatures).FirstOrDefault(x => x.CompanyId == user.CompanyId);
            if (_plan != null)
            {
                foreach (var feature in _plan.Plan.PlanFeatures)
                {
                    claims.AddClaim(new Claim(ClaimTypes.Role, feature.FeatureId));
                }
            }


            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Issuer,
                Audience = _appSettings.Audience,
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

        private RefreshToken generateRefreshToken(string ipAddress)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddMinutes(_appSettings.RefreshTokenExpirationMinutes),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }

   
      
   
        //private void ValidateToken(string token)
        //{
        //    var handler = new JwtSecurityTokenHandler();
        //    var claimsPrincipal = handler.ValidateToken(
        //        token,
        //        new TokenValidationParameters
        //        {
        //            ValidAudience = "you",
        //            ValidIssuer = "me",
        //            RequireSignedTokens = false,
        //            TokenDecryptionKey = new X509SecurityKey(new X509Certificate2("AxxxxPxxxxIxxxxx.pfx", _appSettings.Encryptkey))
        //        },
        //        out SecurityToken securityToken);
        //}
    }

}