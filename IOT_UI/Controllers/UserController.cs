﻿using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class UserController : BaseController
    {
        private readonly APIConnection _apiConnection;

        public UserController(HttpClient httpClient, IConfiguration configuration, APIConnection apiConnection)
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

        // Display all users for a specific customer
        public async Task<IActionResult> Index(Guid customerId)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            // Store customerId in session for later use
            HttpContext.Session.SetString("CustomerId", customerId.ToString());

            // Fetch users associated with the customer
            var users = await GetAllUsersByCustomer(customerId);
            ViewBag.CustomerId = customerId;
            return View(users);
        }

        // Fetch all users for a specific customer from the API
        [HttpGet]
        public async Task<List<UsersViewModel>> GetAllUsersByCustomer(Guid customerId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}User/GetAllUsersByCustomer/{customerId}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // Handle the API response
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<UsersViewModel>>>(data);

                // Return data if successful
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data;
                }
            }

            // Return an empty list if there was an error
            return new List<UsersViewModel>();
        }

        // Show the user creation form
        public async Task<IActionResult> Create()
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString) || !Guid.TryParse(customerIdString, out Guid customerId))
            {
                return RedirectToAction("Index", "User");
            }

            var model = new UsersViewModel
            {
                CustomerId = customerId
            };

            return View(model);
        }

        // Handle the creation of a new user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsersViewModel user, string ConfirmPassword)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            // Validate the model and confirm password
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            if (user.Password != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return View(user);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}User/Register";
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            // Redirect to Index if creation is successful
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index), new { customerId = user.CustomerId });
            }

            // Handle error response
            string errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);
            ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
            return View(user);
        }

        // Show the user edit form
        public async Task<IActionResult> Edit(Guid? id, Guid customerId)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (id == null || id == Guid.Empty)
            {
                return NotFound();
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}User/GetUserById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // Handle API response
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<EditUserViewModel>>(data);

                // Return the user data if successful
                if (apiResponse?.Success == true)
                {
                    ViewBag.CustomerId = customerId;
                    return View(apiResponse.Data);
                }
            }

            // Return NotFound if the user is not found
            return NotFound();
        }

        // Handle user updates
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel user, Guid customerId)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}User/UpdateUser";
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            // Redirect to Index if update is successful
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", new { customerId = customerId });
            }

            // Handle error response
            string errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);
            ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
            return View(user);
        }

        // Show the user delete confirmation view
        public async Task<IActionResult> Delete(Guid id, Guid customerId)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (id == Guid.Empty)
            {
                return NotFound();
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}User/GetUserById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // Handle API response
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<UsersViewModel>>(data);

                // Return the user data if successful
                if (apiResponse?.Success == true)
                {
                    ViewBag.CustomerId = customerId;
                    return View(apiResponse.Data);
                }
            }

            // Return NotFound if the user is not found
            return NotFound();
        }

        // Handle the deletion of a user
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, Guid customerId)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}User/DeleteUser/{id}";
            HttpResponseMessage response = await _httpClient.DeleteAsync(url);

            // Redirect to Index if deletion is successful
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", new { customerId = customerId });
            }

            // Return NotFound if deletion failed
            return NotFound();
        }

        // Show detailed information about a user
        public async Task<IActionResult> Details(Guid? id, Guid customerId)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (id == null || id == Guid.Empty)
            {
                return NotFound();
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}User/GetUserById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // Handle API response
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<UsersViewModel>>(data);

                // Return user details if successful
                if (apiResponse?.Success == true)
                {
                    ViewBag.CustomerId = customerId;
                    return View(apiResponse.Data);
                }
            }

            // Return NotFound if the user is not found
            return NotFound();
        }
    }
}
