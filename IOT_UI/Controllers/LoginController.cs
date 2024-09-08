using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class LoginController : BaseController
    {
        public LoginController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

        private IActionResult RedirectToLoginIfNeeded()
        {
            var token = HttpContext.Session.GetString("JWTtoken");
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("Index");
            }
            return null;
        }

        public ActionResult Index()
        {
            var token = HttpContext.Session.GetString("JWTtoken");
            if (string.IsNullOrEmpty(token))
            {
                return View();
            }
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> LoginUser(UsersViewModel user)
        {
            try
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                var url = $"{_configuration["ApiBaseUrl"]}User/Login";
                HttpResponseMessage response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<UserLoginResponse>>(responseData);

                    if (apiResponse != null && apiResponse.Success)
                    {
                        var token = apiResponse.Data.Token;
                        var userEmail = apiResponse.Data.Email;
                        var roleName = apiResponse.Data.Roles;

                        HttpContext.Session.SetString("JWTtoken", token);
                        HttpContext.Session.SetString("UserEmail", userEmail);
                        HttpContext.Session.SetString("UserRole", roleName);

                        ViewBag.Message = "Login successful!";
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        ViewBag.Message = "Incorrect Email or Password";
                        return View("Index");
                    }
                }
                else
                {
                    ViewBag.Message = "Login failed. Please try again.";
                    return View("Index");
                }
            }
            catch
            {
                ViewBag.Message = "An error occurred. Please try again.";
                return View("Index");
            }
        }

        public IActionResult LogOff()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
