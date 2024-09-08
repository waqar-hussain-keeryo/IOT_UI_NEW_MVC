using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Policy;
using System.Text;

namespace IOT_UI.Controllers
{
    public class UserController : BaseController
    {
        public UserController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

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

            var users = await GetAllUsersByCustomer(customerId);
            ViewBag.CustomerId = customerId;
            return View(users);
        }

        [HttpGet]
        public async Task<List<UsersViewModel>> GetAllUsersByCustomer(Guid customerId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}User/GetAllUsersByCustomer/{customerId}";
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
                return RedirectToAction("Index", "User");
            }

            var model = new UsersViewModel()
            {
                CustomerId = customerId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsersViewModel user, string ConfirmPassword)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

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
            var url = $"{_configuration["ApiBaseUrl"]}User/Register";
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index), new { customerId = user.CustomerId });
            }

            ModelState.AddModelError(string.Empty, "An error occurred while creating the site.");
            return View(user);
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


        public async Task<IActionResult> Delete(Guid id)
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}User/DeleteUser/{id}";
            HttpResponseMessage response = await _httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            return NotFound();
        }


        public async Task<IActionResult> Details(Guid? id)
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
    }
}