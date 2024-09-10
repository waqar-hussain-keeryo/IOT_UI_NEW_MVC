using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace IOT_UI.Controllers
{
    public class SiteController : BaseController
    {
        public SiteController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

        // Redirect to login if the user is not authenticated
        private IActionResult RedirectToLoginIfNeeded()
        {
            var token = HttpContext.Session.GetString("JWTtoken");
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("~/Login/Index");
            }
            return null;
        }

        // Display the list of sites for a customer
        public async Task<IActionResult> Index(Guid customerId)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            // Store customerId in session
            HttpContext.Session.SetString("CustomerId", customerId.ToString());

            // Fetch sites for the given customerId
            var sites = await GetSitesByCustomerId(customerId);
            ViewBag.CustomerId = customerId;
            return View(sites);
        }

        // Fetch sites from API based on customerId
        [HttpGet]
        public async Task<List<Site>> GetSitesByCustomerId(Guid customerId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetCustomerSites?customerId={customerId}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // Handle API response
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<Site>>>(data);

                // Return data if successful
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data;
                }
            }

            // Return an empty list if the API call failed
            return new List<Site>();
        }

        // Show the site creation view
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
                return RedirectToAction("Index", "Site");
            }

            var model = new Site
            {
                CustomerID = customerId
            };

            return View(model);
        }

        // Handle site creation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Site site)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (!ModelState.IsValid)
            {
                return View(site);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/AddSite?customerId={site.CustomerID}";
            var content = new StringContent(JsonConvert.SerializeObject(site), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            // Redirect to Index on successful creation
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index), new { customerId = site.CustomerID });
            }

			string errorContent = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);

			ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
			return View(site);
        }

        // Show the edit view for a specific site
        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id, Guid customerId)
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
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetSiteById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // Handle API response
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Site>>(data);

                // Return the site data if successful
                if (apiResponse?.Success == true)
                {
                    ViewBag.CustomerId = customerId;
                    return View(apiResponse.Data);
                }
            }

            // Return NotFound if the site is not found
            return NotFound();
        }

        // Handle site updates
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Site site, Guid customerId)
        {
            if (!ModelState.IsValid)
            {
                return View(site);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/UpdateSite";
            var content = new StringContent(JsonConvert.SerializeObject(site), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            // Redirect to Index on successful update
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", new { customerId = customerId });
            }

			string errorContent = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);

			ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
			return View(site);
        }

        // Show the delete view for a specific site
        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id, Guid customerId)
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
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetSiteById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // Handle API response
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Site>>(data);

                // Return the site data if successful
                if (apiResponse?.Success == true)
                {
                    ViewBag.CustomerId = customerId;
                    return View(apiResponse.Data);
                }
            }

            // Return NotFound if the site is not found
            return NotFound();
        }

        // Handle site deletion
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid siteId, Guid customerId)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/DeleteSite/{siteId}";
            HttpResponseMessage response = await _httpClient.DeleteAsync(url);

            // Redirect to Index on successful deletion
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", new { customerId = customerId });
            }

            // Return NotFound if deletion failed
            return NotFound();
        }
    }
}
