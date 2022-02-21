using System.ComponentModel.DataAnnotations;

namespace TrackingDEMO.Models
{
    public class RegisterUser
    {
        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        public string? Phone { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? PassWord { get; set; }

        [Required]
        [Compare("PassWord", ErrorMessage = "รหัสผ่านไม่ตรงกัน กรุณาใส่อีกครั้ง!")]
        public string? ConfirmPassWord { get; set; }
    }
}
