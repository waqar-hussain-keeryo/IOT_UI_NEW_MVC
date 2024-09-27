using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class RoleController : BaseController
    {
        private readonly APIConnection _apiConnection;

        public RoleController(HttpClient httpClient, IConfiguration configuration, APIConnection apiConnection)
            : base(httpClient, configuration)
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

        // Display the list of roles
        public async Task<IActionResult> Index()
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            // Fetch all roles
            var roles = await GetAllRoles();
            return View(roles);
        }

        // Fetch all roles from the API
        private async Task<List<RoleViewModel>> GetAllRoles()
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/GetAllRoles";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // Handle API response
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RoleViewModel>>>(data);

                // Return data if the API call was successful
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data ?? new List<RoleViewModel>();
                }
            }

            // Return an empty list if the API call failed
            return new List<RoleViewModel>();
        }

        // Show the role creation view
        public async Task<IActionResult> Create()
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            return View();
        }

        // Handle role creation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel role)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (!ModelState.IsValid)
            {
                return View(role);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/CreateRole";
            var content = new StringContent(JsonConvert.SerializeObject(role), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            // Redirect to Index on successful creation
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            string errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);
            ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
            return View(role);
        }

        // Show the edit view for a specific role
        public async Task<IActionResult> Edit(Guid? id)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (!id.HasValue)
            {
                return NotFound();
            }

            var role = await GetRoleById(id.Value);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // Handle role updates
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleViewModel role)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (!ModelState.IsValid)
            {
                return View(role);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/UpdateRole";
            var content = new StringContent(JsonConvert.SerializeObject(role), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            // Redirect to Index on successful update
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            string errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);
            ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
            return View(role);
        }

        // Show the delete view for a specific role
        public async Task<IActionResult> Delete(Guid? id)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (!id.HasValue)
            {
                return NotFound();
            }

            var role = await GetRoleById(id.Value);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // Handle role deletion
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/DeleteRole/{id}";
            HttpResponseMessage response = await _httpClient.DeleteAsync(url);

            // Redirect to Index on successful deletion
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            // Return NotFound if deletion failed
            return NotFound();
        }

        // Show details for a specific role
        public async Task<IActionResult> Details(Guid? id)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (!id.HasValue)
            {
                return NotFound();
            }

            var role = await GetRoleById(id.Value);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // Helper method to get a role by ID
        private async Task<RoleViewModel> GetRoleById(Guid id)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}GlobalAdmin/GetRoleById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // Handle API response
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RoleViewModel>>(data);

                // Return the role data if the API call was successful
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data;
                }
            }

            // Return null if the role is not found
            return null;
        }
    }
}
