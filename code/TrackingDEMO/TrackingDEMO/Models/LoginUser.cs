using System.ComponentModel.DataAnnotations;

namespace TrackingDEMO.Models
{
    public class LoginUser
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? PassWord { get; set; }
    }
}
