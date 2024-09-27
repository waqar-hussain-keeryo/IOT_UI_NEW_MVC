using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class AdminController : BaseController
    {
        private readonly APIConnection _apiConnection;

        // Constructor to initialize HttpClient, IConfiguration, and APIConnection
        public AdminController(HttpClient httpClient, IConfiguration configuration, APIConnection apiConnection) : base(httpClient, configuration)
        {
            _apiConnection = apiConnection;
        }

        // Centralized method to check API connectivity and session status
        private async Task<IActionResult> CheckApiConnectionAndRedirectIfNeeded()
        {
            if (!await _apiConnection.IsApiConnected())
            {
                HttpContext.Session.Clear();
                return View("ApiError");
            }

            return null;
        }

        // Action to display all admins
        public async Task<IActionResult> Index()
        {
            // Check API connectivity and session validity
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null)
            {
                return checkResult;
            }

            var users = await GetAllAdminsAsync();
            return View(users);
        }

        // Method to retrieve all admin users
        private async Task<List<UsersViewModel>> GetAllAdminsAsync()
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/GetAllAdmin";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<UsersViewModel>>>(data);
                return apiResponse?.Success == true ? apiResponse.Data : new List<UsersViewModel>();
            }

            return new List<UsersViewModel>();
        }

        // Action to show the registration page
        public IActionResult RegisterAdmin()
        {
            return View();
        }

        // Action to handle admin registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAdmin(UsersViewModel user, string ConfirmPassword)
        {
            // Check API connectivity and session validity
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null)
            {
                return checkResult;
            }

            // Validate model state and passwords
            if (!ModelState.IsValid || user.Password != ConfirmPassword)
            {
                if (user.Password != ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                }
                return View(user);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/RegisterAdmin";
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "Admin registered successfully. Please login.";
                return Redirect("~/Login/Index");
            }

            ViewBag.Message = "Error creating user. Please try again.";
            return View(user);
        }

        // Action to display the edit page for a specific user
        public async Task<IActionResult> Edit(Guid id)
        {
            // Check API connectivity and session validity
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null)
            {
                return checkResult;
            }

            if (id == Guid.Empty)
            {
                return NotFound();
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}User/GetUserById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<EditUserViewModel>>(data);
                return apiResponse?.Success == true ? View(apiResponse.Data) : NotFound();
            }

            return NotFound();
        }

        // Action to handle user updates
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel user)
        {
            // Check API connectivity and session validity
            var checkResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (checkResult != null)
            {
                return checkResult;
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/UpdateAdmin";
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }
    }
}
