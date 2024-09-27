using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class ProductTypeController : BaseController
    {
        private readonly APIConnection _apiConnection;

        public ProductTypeController(HttpClient httpClient, IConfiguration configuration, APIConnection apiConnection)
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

        public async Task<IActionResult> Index()
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var productTypes = await GetProductTypes();
            return View(productTypes);
        }

        [HttpGet]
        private async Task<List<ProductTypeViewModel>> GetProductTypes()
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}ProductType/GetAllProductTypes";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductTypeViewModel>>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data ?? new List<ProductTypeViewModel>();
                }
            }
            return new List<ProductTypeViewModel>();
        }

        public async Task<IActionResult> Create()
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null) return redirectResult;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductTypeViewModel productType)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null) return redirectResult;

            if (!ModelState.IsValid)
            {
                return View(productType);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}ProductType/CreateProductType";
            var content = new StringContent(JsonConvert.SerializeObject(productType), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            string errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);
            ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
            return View(productType);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null) return redirectResult;

            if (!id.HasValue) return NotFound();

            var productType = await GetProductTypeById(id.Value);
            if (productType == null) return NotFound();

            return View(productType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductTypeViewModel productType)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null) return redirectResult;

            if (!ModelState.IsValid)
            {
                return View(productType);
            }

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}ProductType/UpdateProductType";
            var content = new StringContent(JsonConvert.SerializeObject(productType), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            string errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);
            ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
            return View(productType);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null) return redirectResult;

            if (!id.HasValue) return NotFound();

            var productType = await GetProductTypeById(id.Value);
            if (productType == null) return NotFound();

            return View(productType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var redirectResult = await CheckApiConnectionAndRedirectIfNeeded();
            if (redirectResult != null) return redirectResult;

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}ProductType/DeleteProductType/{id}";
            HttpResponseMessage response = await _httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            string errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);
            ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
            return NotFound();
        }

        // Helper method to get a product type by ID
        private async Task<ProductTypeViewModel> GetProductTypeById(Guid id)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}ProductType/GetProductTypeById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ProductTypeViewModel>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data;
                }
            }
            return null;
        }
    }
}
