using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace IOT_UI.Models
{
    public class ProductTypeViewModel
    {
        public Guid ProductTypeID { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductTypeName { get; set; }
        public double MinVal { get; set; }
        public double MaxVal { get; set; }

        [Required]
        [StringLength(20)]
        public string UOM { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
