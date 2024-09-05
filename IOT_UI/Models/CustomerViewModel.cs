using System.ComponentModel.DataAnnotations;

namespace IOT_UI.Models
{
    public class CustomerViewModel
    {
        public Guid CustomerID { get; set; }
        
        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string CustomerPhone { get; set; }

        [Required]
        public string CustomerEmail { get; set; }

        [Required]
        public string CustomerCity { get; set; }

        [Required]
        public string CustomerRegion { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; } = false;
        public List<Site> Sites { get; set; } = new List<Site>();
        public List<string> CustomerUsers { get; set; } = new List<string>();
        public List<DigitalService> DigitalServices { get; set; } = new List<DigitalService>();
    }

    public class Site
    {
        public Guid SiteID { get; set; }
        public string SiteName { get; set; }
        public string SiteLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<Device> Devices { get; set; } = new List<Device>();
    }

    public class Device
    {
        public Guid DeviceID { get; set; } = Guid.NewGuid();
        public string DeviceName { get; set; }
        public string ProductType { get; set; }
        public double ThreSholdValue { get; set; }
    }

    public class DigitalService
    {
        public Guid DigitalServiceID { get; set; } = Guid.NewGuid();
        public DateTime ServiceStartDate { get; set; }
        public DateTime ServiceEndDate { get; set; }
        public bool IsActive { get; set; }
        public List<string> NotificationUsers { get; set; } = new List<string>();
    }
}
