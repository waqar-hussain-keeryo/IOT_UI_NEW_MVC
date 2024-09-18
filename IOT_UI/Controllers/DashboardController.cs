using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class DashboardController : BaseController
    {
        public DashboardController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

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
            var recentData = await GetChartData(new DataPoint { Duration = "24h" });

            var model = new DashboardDropdown
            {
                Customers = customers,
                RecentData = recentData
            };

            return View(model);
        }

        [HttpPost]
        public async Task<List<DataPoint>> GetChartData([FromBody] DataPoint requestBody)
        {
            SetAuthorizationHeader();
            string duration = requestBody.Duration;

            var url = $"{_configuration["ApiBaseUrl"]}Dashboard/GetChartData";

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

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

        [HttpGet]
        public async Task<List<Site>> GetSitesByCustomerId(Guid customerId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetCustomerSites?customerId={customerId}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // Handle API response
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<Site>>>(data);

                // Return data if successful
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data;
                }
            }

            // Return an empty list if the API call failed
            return new List<Site>();
        }

        [HttpGet]
        public async Task<List<Device>> GetDevicesBySiteId(Guid siteId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetSiteDevices?siteId={siteId}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<Device>>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data;
                }
            }
            return new List<Device>();
        }
    }
}
