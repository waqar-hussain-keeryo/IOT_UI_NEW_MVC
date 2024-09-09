using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class DigitalServiceController : BaseController
    {
        public DigitalServiceController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

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

        // Display digital services for a specific customer
        public async Task<IActionResult> Index(Guid customerId)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            // Store customerId in session
            HttpContext.Session.SetString("CustomerId", customerId.ToString());

            var digitalServices = await GetDigitalServicesByCustomerId(customerId);
            ViewBag.CustomerId = customerId;
            return View(digitalServices);
        }

        // Retrieve digital services for a customer from API
        [HttpGet]
        private async Task<List<DigitalService>> GetDigitalServicesByCustomerId(Guid customerId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetAllDigitalService?customerId={customerId}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<DigitalService>>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data;
                }
            }
            return new List<DigitalService>(); // Return empty list if an error occurs
        }

        // Display form to create a new digital service
        public IActionResult Create()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString) || !Guid.TryParse(customerIdString, out Guid customerId))
            {
                return RedirectToAction(nameof(Index));
            }

            var model = new DigitalService { CustomerID = customerId };
            return View(model);
        }

        // Handle POST request to create a new digital service
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

            ModelState.AddModelError(string.Empty, "An error occurred while creating the digital service.");
            return View(service);
        }

        // Display form to edit an existing digital service
        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id, Guid customerId)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            if (id == null) return NotFound();

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetDigitalServiceById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DigitalService>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    ViewBag.CustomerId = customerId;
                    return View(apiResponse.Data);
                }
            }

            return NotFound();
        }

        // Handle POST request to update an existing digital service
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
                return RedirectToAction(nameof(Index), new { customerId = customerId });
            }

            ModelState.AddModelError(string.Empty, "An error occurred while updating the digital service.");
            return View(service);
        }

        // Display confirmation page for deleting a digital service
        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id, Guid customerId)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            if (id == null) return NotFound();

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetDigitalServiceById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DigitalService>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    ViewBag.CustomerId = customerId;
                    return View(apiResponse.Data);
                }
            }

            return NotFound();
        }

        // Handle POST request to confirm deletion of a digital service
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid digitalServiceId, Guid customerId)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/DeleteDigitalService/{digitalServiceId}";
            HttpResponseMessage response = await _httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index), new { customerId = customerId });
            }

            return NotFound();
        }
    }
}
