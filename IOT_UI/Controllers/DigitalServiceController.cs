using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class DigitalServiceController : BaseController
    {
        public DigitalServiceController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

        private IActionResult RedirectToLoginIfNeeded()
        {
            var token = HttpContext.Session.GetString("JWTtoken");
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("~/Login/Index");
            }
            return null;
        }

        public async Task<IActionResult> Index(Guid customerId)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            // Store customerId in session
            HttpContext.Session.SetString("CustomerId", customerId.ToString());

            var digitalService = await GetDigitalServiceByCustomerId(customerId);
            ViewBag.CustomerId = customerId;
            return View(digitalService);
        }

        [HttpGet]
        public async Task<List<DigitalService>> GetDigitalServiceByCustomerId(Guid customerId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetAllDigitalService?customerId={customerId}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<DigitalService>>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    return apiResponse.Data;
                }
            }

            return new List<DigitalService>();
        }

        public IActionResult Create()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString) || !Guid.TryParse(customerIdString, out Guid customerId))
            {
                return RedirectToAction("Index", "DigitalService");
            }

            var model = new DigitalService
            {
                CustomerID = customerId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DigitalService service)
        {
            if (!ModelState.IsValid)
            {
                return View(service);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/AddDigitalService?customerId={service.CustomerID}";
            var content = new StringContent(JsonConvert.SerializeObject(service), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index), new { customerId = service.CustomerID });
            }

            ModelState.AddModelError(string.Empty, "An error occurred while creating the site.");
            return View(service);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id, Guid customerId)
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
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetDigitalServiceById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DigitalService>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    ViewBag.CustomerId = customerId;
                    return View(apiResponse.Data);
                }
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DigitalService service, Guid customerId)
        {
            if (!ModelState.IsValid)
            {
                return View(service);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/UpdateDigitalService";
            var content = new StringContent(JsonConvert.SerializeObject(service), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                // Redirect to the Index action for the customer
                return RedirectToAction("Index", "DigitalService", new { customerId = customerId });
            }

            ModelState.AddModelError(string.Empty, "An error occurred while updating the site.");
            return View(service);
        }

        public async Task<IActionResult> Delete(Guid? id, Guid customerId)
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
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetDigitalServiceById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DigitalService>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    ViewBag.CustomerId = customerId;
                    return View(apiResponse.Data);
                }
            }

            return NotFound();
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid digitalServiceId, Guid customerId)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/DeleteDigitalService/{digitalServiceId}";
            HttpResponseMessage response = await _httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", new { customerId = customerId });
            }

            return NotFound();
        }
    }
}
