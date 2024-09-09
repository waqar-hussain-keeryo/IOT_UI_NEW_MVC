using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class LoginController : BaseController
    {
        public LoginController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

        // Redirect to dashboard if the user is already logged in
        private IActionResult RedirectToDashboardIfLoggedIn()
        {
            var token = HttpContext.Session.GetString("JWTtoken");
            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return null;
        }

        // Display login page if user is not logged in, otherwise redirect to dashboard
        public IActionResult Index()
        {
            var redirectResult = RedirectToDashboardIfLoggedIn();
            if (redirectResult != null) return redirectResult;

            return View();
        }

        // Handle user login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginUser(UsersViewModel user)
        {
            try
            {
                // Prepare HTTP content for the request
                var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                var url = $"{_configuration["ApiBaseUrl"]}User/Login";

                // Send login request
                var response = await _httpClient.PostAsync(url, content);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<UserLoginResponse>>(responseData);

                    if (apiResponse?.Success == true)
                    {
                        // Store token and user details in session
                        var token = apiResponse.Data.Token;
                        var userEmail = apiResponse.Data.Email;
                        var roleName = apiResponse.Data.Roles;

                        HttpContext.Session.SetString("JWTtoken", token);
                        HttpContext.Session.SetString("UserEmail", userEmail);
                        HttpContext.Session.SetString("UserRole", roleName);

                        ViewBag.Message = "Login successful!";
                        return RedirectToAction("Index", "Dashboard");
                    }

                    ViewBag.Message = "Incorrect Email or Password.";
                }
                else
                {
                    ViewBag.Message = "Login failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                // Log exception details (consider using a logging framework)
                ViewBag.Message = $"An error occurred: {ex.Message}. Please try again.";
            }

            return View("Index");
        }

        // Log off the user and clear session
        public IActionResult LogOff()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
