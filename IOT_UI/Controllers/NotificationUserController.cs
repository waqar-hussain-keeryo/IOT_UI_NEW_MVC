using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class NotificationUserController : BaseController
    {
        private readonly APIConnection _apiConnection;

        public NotificationUserController(HttpClient httpClient, IConfiguration configuration, APIConnection apiConnection) : base(httpClient, configuration)
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

        // Check if user is logged in; this method can be merged with CheckApiConnectionIfNeeded if desired.
        private IActionResult RedirectToLoginIfNeeded()
        {
            var token = HttpContext.Session.GetString("JWTtoken");
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("~/Login/Index");
            }
            return null;
        }

        public async Task<IActionResult> Index(Guid digitalServiceId)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            HttpContext.Session.SetString("DigitalServiceId", digitalServiceId.ToString());

            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString) || !Guid.TryParse(customerIdString, out Guid customerId))
            {
                return RedirectToAction("Index", "NotificationUser");
            }

            var notificationUsers = await GetNotificationUsers(digitalServiceId);
            var allUsers = await GetAllUsersByCustomer(customerId);

            var model = new DigitalService
            {
                DigitalServiceID = digitalServiceId,
                NotificationUsers = notificationUsers,
                CustomerID = customerId,
                Users = allUsers
            };

            return View(model);
        }

        [HttpGet]
        public async Task<List<UsersViewModel>> GetAllUsersByCustomer(Guid customerId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}User/GetAllUsersByCustomer/{customerId}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<UsersViewModel>>>(data);

                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data ?? new List<UsersViewModel>();
                }
            }

            return new List<UsersViewModel>();
        }

        [HttpGet]
        public async Task<List<string>> GetNotificationUsers(Guid digitalServiceId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetNotificationUserById/{digitalServiceId}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<string>>>(data);

                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data ?? new List<string>();
                }
            }

            return new List<string>();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] DigitalService digitalService)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data." });
            }

            try
            {
                SetAuthorizationHeader();
                var url = $"{_configuration["ApiBaseUrl"]}Customer/AddNotificationUser";
                var content = new StringContent(JsonConvert.SerializeObject(digitalService), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "An error occurred while creating the notification user." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
