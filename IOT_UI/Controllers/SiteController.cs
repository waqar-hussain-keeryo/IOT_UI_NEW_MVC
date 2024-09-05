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

        public IActionResult Create(Guid customerId)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var model = new SiteViewModel
            {
                CustomerID = customerId,
                Site = new Site()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SiteViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/AddSite?customerId={viewModel.CustomerID}";
            var content = new StringContent(JsonConvert.SerializeObject(viewModel.Site), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index), new { customerId = viewModel.CustomerID });
            }

            ModelState.AddModelError(string.Empty, "An error occurred while creating the site.");
            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, Guid customerId)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetSiteById?siteId={id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Site>>(data);

                if (apiResponse != null && apiResponse.Success)
                {
                    var model = new SiteViewModel
                    {
                        CustomerID = customerId,
                        Site = apiResponse.Data
                    };

                    return View(model);
                }
            }

            return NotFound();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SiteViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/UpdateSite?siteId={viewModel.Site.SiteID}";
            var content = new StringContent(JsonConvert.SerializeObject(viewModel.Site), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index), new { customerId = viewModel.CustomerID });
            }

            ModelState.AddModelError(string.Empty, "An error occurred while updating the site.");
            return View(viewModel);
        }

    }
}
