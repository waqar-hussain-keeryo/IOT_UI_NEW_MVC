using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class ProductTypeController : BaseController
    {
        public ProductTypeController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

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

        public async Task<IActionResult> Index()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var productType = await GetProductType();
            return View(productType);
        }

        [HttpGet]
        private async Task<List<ProductTypeViewModel>> GetProductType()
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}ProductType/GetAllProductTypes";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductTypeViewModel>>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data;
                }
            }
            return new List<ProductTypeViewModel>();
        }

        public IActionResult Create()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductTypeViewModel productType)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}ProductType/CreateProductType";
            var content = new StringContent(JsonConvert.SerializeObject(productType), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            // Handle error (e.g., add error message to model state)
            ModelState.AddModelError(string.Empty, "Error creating customer.");
            return View(productType);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            var redirectResult = RedirectToLoginIfNeeded();
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
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}ProductType/UpdateProductType";
            var content = new StringContent(JsonConvert.SerializeObject(productType), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Error updating customer.");
            return View(productType);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            var redirectResult = RedirectToLoginIfNeeded();
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
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}ProductType/DeleteProductType/{id}";
            HttpResponseMessage response = await _httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Error deleting customer.");
            return NotFound();
        }


        // Helper method to get a customer by ID
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
