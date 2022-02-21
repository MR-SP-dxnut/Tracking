using System;
using System.ComponentModel.DataAnnotations;

namespace TrackingDEMO.Models
{
    public class TrackingProduct
    {
        
        public string id { get; set; }

        public string name_Product { get; set; }

        public string name_Location { get; set; }

        [Required]
        public string name_Status { get; set; }


        public string date_time_Status { get; set; }
    }
}
