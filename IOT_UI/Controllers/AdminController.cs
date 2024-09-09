using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class AdminController : BaseController
    {
        // Constructor to initialize HttpClient and IConfiguration
        public AdminController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

        // Helper method to redirect to login page if JWT token is missing
        private IActionResult RedirectToLoginIfNeeded()
        {
            var token = HttpContext.Session.GetString("JWTtoken");
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("~/Login/Index");
            }
            return null;
        }

        // Action to display all admins
        public async Task<IActionResult> Index()
        {
            // Check if user needs to be redirected to login
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
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
            // Validate model state
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            // Check if passwords match
            if (user.Password != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return View(user);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/RegisterAdmin";
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                // Set a success message and redirect to login page
                ViewBag.Message = "Admin registered successfully. Please login.";
                return Redirect("~/Login/Index");
            }
            else
            {
                // Set error message to view if registration fails
                ViewBag.Message = "Error creating user. Please try again.";
                return View(user);
            }
        }

        // Action to display the edit page for a specific user
        public async Task<IActionResult> Edit(Guid id)
        {
            // Check if user needs to be redirected to login
            var redirectResult = RedirectToLoginIfNeeded();
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

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<UsersViewModel>>(data);
                return apiResponse?.Success == true ? View(apiResponse.Data) : NotFound();
            }

            return NotFound();
        }

        // Action to handle user updates
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsersViewModel user)
        {
            // Check if user needs to be redirected to login
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}User/UpdateUser";
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
