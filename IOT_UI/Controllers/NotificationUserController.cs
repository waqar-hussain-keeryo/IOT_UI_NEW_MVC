using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class NotificationUserController : BaseController
    {
        public NotificationUserController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

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
            var redirectResult = RedirectToLoginIfNeeded();
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
            ViewBag.DigitalServiceId = digitalServiceId;
            return View(notificationUsers);
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
                    return apiResponse.Data;
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

                try
                {
                    // Assuming the API returns a simple list of strings (user emails)
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<string>>>(data);

                    if (apiResponse?.Success == true)
                    {
                        return apiResponse.Data ?? new List<string>();
                    }
                }
                catch (JsonException ex)
                {
                    // Handle JSON deserialization errors
                    // Log the exception if needed
                    // _logger.LogError("Deserialization error: " + ex.Message);
                }
            }

            return new List<string>();
        }


        public async Task<IActionResult> Create()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var digitalServiceIdString = HttpContext.Session.GetString("DigitalServiceId");
            if (string.IsNullOrEmpty(digitalServiceIdString) || !Guid.TryParse(digitalServiceIdString, out Guid digitalServiceId))
            {
                return RedirectToAction("Index", "NotificationUser");
            }

            var customerIdString = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdString) || !Guid.TryParse(customerIdString, out Guid customerId))
            {
                return RedirectToAction("Index", "NotificationUser");
            }

            var allUsers = await GetAllUsersByCustomer(customerId);
            var model = new DigitalService
            {
                DigitalServiceID = digitalServiceId,
                CustomerID = customerId,
                Users = allUsers
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DigitalService digitalService)
        {
            if (!ModelState.IsValid)
            {
                return View(digitalService);
            }

            try
            {
                SetAuthorizationHeader();
                var url = $"{_configuration["ApiBaseUrl"]}Customer/AddNotificationUser";
                var content = new StringContent(JsonConvert.SerializeObject(digitalService), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index), new { digitalServiceId = digitalService.DigitalServiceID });
                }

                ModelState.AddModelError(string.Empty, "An error occurred while creating the notification user.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }

            return View(digitalService);
        }
    }
}
