using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly APIConnection _apiConnection;

        public DashboardController(HttpClient httpClient, IConfiguration configuration, APIConnection apiConnection) : base(httpClient, configuration)
        {
            _apiConnection = apiConnection;
        }

        // Centralized method to check API connectivity and session status
        private async Task<IActionResult> CheckApiConnectionAndRedirectIfNeeded()
        {
            if (!await _apiConnection.IsApiConnected())
            {
                HttpContext.Session.Clear();
                return View("ApiError"); // Redirect to API error view
            }

            var token = HttpContext.Session.GetString("JWTtoken");
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("~/Login/Index"); // Redirect to login if session is missing
            }

            return null;
        }

        public async Task<IActionResult> Index()
        {
            // Check API connectivity and session status
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null) return checkResult;

            var customers = await GetAllCustomers();

            var model = new DashboardDropdown
            {
                Customers = customers
            };

            return View(model);
        }

        [HttpPost]
        public async Task<List<DataPoint>> GetChartData([FromBody] ChartRequest request)
        {
            // Check API connectivity and session status
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null) return new List<DataPoint>();

            SetAuthorizationHeader();

            var url = $"{_configuration["ApiBaseUrl"]}Dashboard/GetChartData";
            var json = JsonConvert.SerializeObject(request);
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
            // Check API connectivity and session status
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null) return new List<CustomerViewModel>();

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
            // Check API connectivity and session status
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null) return new List<Site>();

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
            // Check API connectivity and session status
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null) return new List<Device>();

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
