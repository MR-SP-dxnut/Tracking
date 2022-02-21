using System.ComponentModel.DataAnnotations;

namespace TrackingDEMO.Models
{
    public class CreateTracking
    {
        public string id_Tracking { get; set; }

        public byte[] QRCode { get; set; }

        public string QRCodes { get; set; }

        // Product
        [Required]
        public string id_Product { get; set; }

        [Required]
        public string name_Product { get; set; }

        public string url_Product { get; set; }

        
    }
}
