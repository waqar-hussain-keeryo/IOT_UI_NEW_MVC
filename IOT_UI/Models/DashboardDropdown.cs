namespace IOT_UI.Models
{
    public class DashboardDropdown
    {
        public List<CustomerViewModel> Customers { get; set; }
        public List<Site> Sites { get; set; }
        public List<Device> Devices { get; set; }
        public List<DataPoint> RecentData { get; set; }
    }

    public class DataPoint
    {
        public string? Duration { get; set; }
        public DateTime? Time { get; set; }
        public double? Value { get; set; }
    }

    public class ChartRequest
    {
        public Guid DeviceId { get; set; }
        public string Duration { get; set; }
    }
}
