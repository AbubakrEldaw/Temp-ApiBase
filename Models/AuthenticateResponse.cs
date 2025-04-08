using System;
using System.Text.Json.Serialization;
using APIBase.Models.Master;

namespace APIBase.Models
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }
        public DateTime JwtTokenExpireAtUTC { get; set; }

        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpireAtUTC { get; set; }
        public string CompanyId { get; set; }
        public string EmployeeId { get; set; }
        public AuthenticateResponse()
        {

        }

        public AuthenticateResponse(User user, string jwtToken, DateTime jwtExpireAt, string refreshToken, DateTime refreshTokenExpireAtUTC)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Username = user.Username;
            CompanyId = user.CompanyId;
            EmployeeId = user.EmployeeId;
            JwtToken = jwtToken;
            JwtTokenExpireAtUTC = jwtExpireAt;
            RefreshToken = refreshToken;
            RefreshTokenExpireAtUTC = refreshTokenExpireAtUTC;
        }
    }
}