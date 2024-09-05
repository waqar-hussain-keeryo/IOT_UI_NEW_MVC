using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class RoleController : BaseController
    {
        public RoleController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

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

            var role = await GetAllRoles();
            return View(role);
        }

        [HttpGet]
        public async Task<List<RoleViewModel>> GetAllRoles()
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/GetAllRoles";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RoleViewModel>>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    return apiResponse.Data;
                }
            }

            return new List<RoleViewModel>();
        }

        public IActionResult Create()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel customer)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/CreateRole";
            var content = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid? id)
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
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/GetRoleById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RoleViewModel>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    return View(apiResponse.Data);
                }
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleViewModel customer)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/UpdateRole";
            var content = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        public async Task<IActionResult> Delete(Guid? id)
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
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/GetRoleById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RoleViewModel>>(data);

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
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/DeleteRole/{id}";
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
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/GetRoleById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RoleViewModel>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    return View(apiResponse.Data);
                }
            }

            return NotFound();
        }
    }
}
