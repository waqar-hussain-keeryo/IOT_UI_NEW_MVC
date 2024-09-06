using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class DeviceController : BaseController
    {
        public DeviceController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

        private IActionResult RedirectToLoginIfNeeded()
        {
            var token = HttpContext.Session.GetString("JWTtoken");
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("~/Login/Index");
            }
            return null;
        }

        public async Task<IActionResult> Index(Guid siteId)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            // Store customerId in session
            HttpContext.Session.SetString("SiteId", siteId.ToString());

            var device = await GetDeviceBySiteId(siteId);
            ViewBag.SiteId = siteId;
            return View(device);
        }

        [HttpGet]
        public async Task<List<Device>> GetDeviceBySiteId(Guid siteId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetSiteDevices?siteId={siteId}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<Device>>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    return apiResponse.Data;
                }
            }

            return new List<Device>();
        }

        public IActionResult Create()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var siteIdString = HttpContext.Session.GetString("SiteId");
            if (string.IsNullOrEmpty(siteIdString) || !Guid.TryParse(siteIdString, out Guid siteId))
            {
                return RedirectToAction("Index", "Device");
            }

            var model = new Device
            {
                SiteID = siteId
            };

            return View(model);
        }

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

            ModelState.AddModelError(string.Empty, "An error occurred while creating the site.");
            return View(device);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id, Guid siteId)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (id == null)
            {
                return NotFound();
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetDeviceById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Device>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    ViewBag.SiteId = siteId;
                    return View(apiResponse.Data);
                }
            }

            return NotFound();
        }

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
                // Redirect to the Index action for the customer
                return RedirectToAction("Index", "Device", new { siteId = device.SiteID });
            }

            ModelState.AddModelError(string.Empty, "An error occurred while updating the site.");
            return View(device);
        }
    }
}
