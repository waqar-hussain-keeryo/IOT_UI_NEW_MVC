using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace IOT_UI.Controllers
{
    public class AdminController : BaseController
    {
        public AdminController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

        private IActionResult RedirectToLoginIfNeeded()
        {
            var token = HttpContext.Session.GetString("JWTtoken");
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("~/Login/Index");
            }
            return null;
        }

        
        public async Task<IActionResult> Index()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var users = await GetAllAdmin();
            return View(users);
        }

        [HttpGet]
        public async Task<List<UsersViewModel>> GetAllAdmin()
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/GetAllAdmin";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<UsersViewModel>>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    return apiResponse.Data;
                }
            }

            return new List<UsersViewModel>();
        }


        public IActionResult RegisterAdmin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAdmin(UsersViewModel user, string ConfirmPassword)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            // Check if the password and confirm password match
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
                return Redirect("~/Login/Index");
            }
            else
            {
                ViewBag.Message = "Error creating user. Please try again.";
                return View(user);
            }
        }


        public async Task<IActionResult> Edit(Guid id)
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
            var url = $"{_configuration["ApiBaseUrl"]}User/GetUserById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<UsersViewModel>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    return View(apiResponse.Data);
                }
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsersViewModel user)
        {
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
