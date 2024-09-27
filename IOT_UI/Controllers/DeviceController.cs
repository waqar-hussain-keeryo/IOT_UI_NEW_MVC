using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class DeviceController : BaseController
    {
        private readonly APIConnection _apiConnection;

        public DeviceController(HttpClient httpClient, IConfiguration configuration, APIConnection apiConnection) : base(httpClient, configuration)
        {
            _apiConnection = apiConnection;
        }

        // Check API connection and redirect if necessary
        private async Task<IActionResult> CheckApiConnectionAndRedirectIfNeeded()
        {
            if (!await _apiConnection.IsApiConnected())
            {
                HttpContext.Session.Clear();
                return View("ApiError");
            }

            var token = HttpContext.Session.GetString("JWTtoken");
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("~/Login/Index");
            }

            return null;
        }

        // Display devices for a specific site
        public async Task<IActionResult> Index(Guid siteId)
        {
            // Check API connectivity and session status
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null) return checkResult;

            // Store siteId in session
            HttpContext.Session.SetString("SiteId", siteId.ToString());

            var devices = await GetDevicesBySiteId(siteId);

            ViewBag.SiteId = siteId;
            return View(devices);
        }

        // Retrieve devices for a site from API
        [HttpGet]
        private async Task<List<Device>> GetDevicesBySiteId(Guid siteId)
        {
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

        [HttpGet]
        private async Task<List<ProductTypeViewModel>> GetProductType()
        {
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null) return new List<ProductTypeViewModel>();

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}ProductType/GetAllProductTypes";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductTypeViewModel>>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data;
                }
            }
            return new List<ProductTypeViewModel>();
        }

        // Display form to create a new device
        public async Task<IActionResult> Create()
        {
            // Check API connectivity and session status
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null) return checkResult;

            var siteIdString = HttpContext.Session.GetString("SiteId");
            if (string.IsNullOrEmpty(siteIdString) || !Guid.TryParse(siteIdString, out Guid siteId))
            {
                return RedirectToAction(nameof(Index));
            }

            var productType = await GetProductType();
            var model = new Device { SiteID = siteId, ProductTypeList = productType };
            return View(model);
        }

        // Handle POST request to create a new device
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Device device)
        {
            // Check API connectivity and session status
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null) return checkResult;

            if (!ModelState.IsValid)
            {
                device.ProductTypeList = await GetProductType();
                return View(device);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/AddDevice?siteId={device.SiteID}";
            var content = new StringContent(JsonConvert.SerializeObject(device), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index), new { siteId = device.SiteID });
            }

            string errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);

            ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");

            device.ProductTypeList = await GetProductType();
            return View(device);
        }

        // Display form to edit an existing device
        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id, Guid siteId)
        {
            // Check API connectivity and session status
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null) return checkResult;

            if (!id.HasValue) return NotFound();

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetDeviceById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Device>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    // Load product types for the dropdown
                    var productType = await GetProductType();
                    var model = apiResponse.Data;
                    model.ProductTypeList = productType;

                    ViewBag.SiteId = siteId;
                    return View(model);
                }
            }

            return NotFound();
        }

        // Handle POST request to update an existing device
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Device device)
        {
            // Check API connectivity and session status
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null) return checkResult;

            if (!ModelState.IsValid)
            {
                // Reload product types to repopulate the dropdown in case of validation error
                device.ProductTypeList = await GetProductType();
                return View(device);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/UpdateDevice";
            var content = new StringContent(JsonConvert.SerializeObject(device), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                // Redirect to Index view with the current siteId
                return RedirectToAction(nameof(Index), new { siteId = device.SiteID });
            }

            // Handle errors
            string errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);

            // Repopulate product types and show the error message
            device.ProductTypeList = await GetProductType();
            ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
            return View(device);
        }
    }
}
