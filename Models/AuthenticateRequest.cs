using System.ComponentModel.DataAnnotations;

namespace APIBase.Models
{
    public class AuthenticateRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string PosDeviceId { get; set; } = "";
    }
}