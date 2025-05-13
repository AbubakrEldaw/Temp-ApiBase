using System.ComponentModel.DataAnnotations;
#nullable disable
namespace APIBase.PublicAPIModels.OAuth
{

    public class TokenRequest
    {
        /// <summary>
        /// ClientId
        /// </summary>
        /// <example>adFAg56JvxvAm90</example>
        [Required]
        public string ClientId { get; set; }

        /// <summary>
        /// Client Secert
        /// </summary>
        /// <example>M0s$kdaU7vL#mc)f0A5m!9vlD9$0as</example>
        [Required]
        public string ClientSecret { get; set; }
    }

    public class TokenResponse
    {
        public string Token { get; set; }
        public string tokenType { get; set; } = "Bearer";
        public string ExpireAtUTC { get; set; }
        public string RefreshToken { get; set; }
        public string RefreshTokenExpireAtUTC { get; set; }
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string ClientId { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
