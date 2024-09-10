using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class DeviceController : BaseController
    {
        public DeviceController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

        // Redirect to login page if JWT token is missing
        private IActionResult RedirectToLoginIfNeeded()
        {
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
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

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
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

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
            if (!ModelState.IsValid)
            {
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
			return View(device);
        }

        // Display form to edit an existing device
        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id, Guid siteId)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            if (!id.HasValue) return NotFound();

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetDeviceById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Device>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    ViewBag.SiteId = siteId;
                    return View(apiResponse.Data);
                }
            }

            return NotFound();
        }

        // Handle POST request to update an existing device
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Device device)
        {
            if (!ModelState.IsValid)
            {
                return View(device);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/UpdateDevice";
            var content = new StringContent(JsonConvert.SerializeObject(device), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index), new { siteId = device.SiteID });
            }

			string errorContent = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);

			ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
			return View(device);
        }
    }
}
