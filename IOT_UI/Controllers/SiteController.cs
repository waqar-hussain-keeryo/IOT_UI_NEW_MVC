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

            var sites = await GetSitesByCustomerId(customerId);
            ViewBag.CustomerId = customerId;
            return View(sites);
        }

        [HttpGet]
        public async Task<List<Site>> GetSitesByCustomerId(Guid customerId)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetCustomerSites?customerId={customerId}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<Site>>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    return apiResponse.Data;
                }
            }

            return new List<Site>();
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
                return RedirectToAction("Index", "Home");
            }

            var model = new Site
            {
                CustomerID = customerId
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Site site)
        {
            if (!ModelState.IsValid)
            {
                return View(site);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/AddSite?customerId={site.CustomerID}";
            var content = new StringContent(JsonConvert.SerializeObject(site), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index), new { customerId = site.CustomerID });
            }

            ModelState.AddModelError(string.Empty, "An error occurred while creating the site.");
            return View(site);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, Guid customerId)
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

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Site>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    ViewBag.CustomerId = customerId;
                    return View(apiResponse.Data);
                }
            }

            return NotFound();
        }

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

            if (response.IsSuccessStatusCode)
            {
                // Redirect to the Index action for the customer
                return RedirectToAction("Index", "Site", new { customerId = customerId });
            }

            ModelState.AddModelError(string.Empty, "An error occurred while updating the site.");
            return View(site);
        }


    }
}
