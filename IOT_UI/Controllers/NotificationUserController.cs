using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Policy;
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

            var digitalService = await GetNotificationUsers(digitalServiceId);
            var allUsers = await GetAllUsersByCustomer(customerId);
            ViewBag.CustomerId = customerId;
            return View(digitalService);
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

        [HttpGet]
        public async Task<List<DigitalService>> GetNotificationUsers(Guid digitalServiceId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetNotificationUserById/{digitalServiceId}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<DigitalService>>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    return apiResponse.Data;
                }
            }

            return new List<DigitalService>();
        }

        public IActionResult Create()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var digitalServiceId = HttpContext.Session.GetString("DigitalServiceId");
            if (string.IsNullOrEmpty(digitalServiceId) || !Guid.TryParse(digitalServiceId, out Guid serviceId))
            {
                return RedirectToAction("Index", "NotificationUser");
            }

            var model = new DigitalService
            {
                DigitalServiceID = serviceId
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

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/AddNotificationUser?digitalServiceId={digitalService.DigitalServiceID}";
            var content = new StringContent(JsonConvert.SerializeObject(digitalService), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index), new { digitalServiceId = digitalService.DigitalServiceID });
            }

            ModelState.AddModelError(string.Empty, "An error occurred while creating the site.");
            return View(digitalService);
        }
    }
}
