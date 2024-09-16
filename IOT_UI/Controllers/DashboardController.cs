using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOT_UI.Controllers
{
    public class DashboardController : BaseController
    {
        public DashboardController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

        // Helper method to handle redirection to login if JWT token is missing
        private IActionResult RedirectToLoginIfNeeded()
        {
            var token = HttpContext.Session.GetString("JWTtoken");
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("~/Login/Index");
            }
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            var customers = await GetAllCustomers();
            var recentData = await GetRecentData();

            var model = new DashboardDropdown
            {
                Customers = customers,
                RecentData = recentData
            };

            return View(model);
        }

        private async Task<List<DataPoint>> GetRecentData()
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Dashboard/GetRecentData";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<DataPoint>>(jsonResponse);
            }
            return new List<DataPoint>();
        }

        [HttpGet]
        private async Task<List<CustomerViewModel>> GetAllCustomers()
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetAllCustomers";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<CustomerViewModel>>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data;
                }
            }
            return new List<CustomerViewModel>();
        }

        [HttpGet("GetSitesByCustomerId")]
        public async Task<IActionResult> GetSitesByCustomerId(Guid customerId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetCustomerSites?customerId={customerId}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<Site>>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    return Ok(apiResponse.Data);
                }
            }
            return Ok(new List<Site>());
        }

        [HttpGet("GetDevicesBySiteId")]
        public async Task<IActionResult> GetDevicesBySiteId(Guid siteId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetSiteDevices?siteId={siteId}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<Device>>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    return Ok(apiResponse.Data);
                }
            }
            return Ok(new List<Device>());
        }
    }
}
