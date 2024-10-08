﻿using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class DigitalServiceController : BaseController
    {
        private readonly APIConnection _apiConnection;

        public DigitalServiceController(HttpClient httpClient, IConfiguration configuration, APIConnection apiConnection)
            : base(httpClient, configuration)
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

        // Display digital services for a specific customer
        public async Task<IActionResult> Index(Guid customerId)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
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
        public async Task<IActionResult> Create()
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
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
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null) return redirectResult;

            if (!ModelState.IsValid)
            {
                return View(service);
            }

            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString) || !Guid.TryParse(customerIdString, out Guid customerId))
            {
                return RedirectToAction(nameof(Index));
            }

            // Retrieve existing services for the customer
            var existingServices = await GetDigitalServicesByCustomerId(customerId);

            // Check for overlapping dates
            foreach (var existingService in existingServices)
            {
                if (DatesOverlap(service.ServiceStartDate, service.ServiceEndDate, existingService.ServiceStartDate, existingService.ServiceEndDate))
                {
                    ModelState.AddModelError(string.Empty, "The selected dates overlap with an existing digital service. Please choose non-overlapping dates.");
                    return View(service);
                }
            }

            // If no overlap, proceed to call API to create the service
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/AddDigitalService?customerId={service.CustomerID}";
            var content = new StringContent(JsonConvert.SerializeObject(service), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index), new { customerId = service.CustomerID });
            }

            string errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);

            ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
            return View(service);
        }

        // Display form to edit an existing digital service
        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id, Guid customerId)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
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
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null) return redirectResult;

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

            string errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);

            ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
            return View(service);
        }

        // Display confirmation page for deleting a digital service
        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id, Guid customerId)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
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
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
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

        private bool DatesOverlap(DateTime newStart, DateTime newEnd, DateTime existingStart, DateTime existingEnd)
        {
            return newStart <= existingEnd && newEnd >= existingStart;
        }
    }
}
