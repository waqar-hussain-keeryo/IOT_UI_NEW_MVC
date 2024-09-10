using IOT_UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IOT_UI.Controllers
{
    public class CustomerController : BaseController
    {
        public CustomerController(HttpClient httpClient, IConfiguration configuration) : base(httpClient, configuration) { }

        // Helper method to handle redirection to login if JWT token is missing
        private IActionResult RedirectToLoginIfNeeded()
        {
            var token = HttpContext.Session.GetString("JWTtoken");
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("~/Login/Index");
            }
            return null;
        }

        // Index action to fetch and display all customers
        public async Task<IActionResult> Index()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            var customers = await GetAllCustomers();
            return View(customers);
        }

        // Get all customers with API call
        [HttpGet]
        private async Task<List<CustomerViewModel>> GetAllCustomers()
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetAllCustomers";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<CustomerViewModel>>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data;
                }
            }
            return new List<CustomerViewModel>();
        }

        // Display the form to create a new customer
        public IActionResult Create()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            return View();
        }

        // Handle POST request to create a new customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerViewModel customer)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/RegisterCustomer";
            var content = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

			string errorContent = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);

			ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
			return View(customer);
        }

        // Display the form to edit an existing customer
        public async Task<IActionResult> Edit(Guid? id)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            if (!id.HasValue) return NotFound();

            var customer = await GetCustomerById(id.Value);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // Handle POST request to update an existing customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerViewModel customer)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/UpdateCustomer";
            var content = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

			string errorContent = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonConvert.DeserializeObject<ErrorResponseDTO>(errorContent);

			ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "An unknown error occurred.");
			return View(customer);
        }

        // Display the form to confirm deletion of a customer
        public async Task<IActionResult> Delete(Guid? id)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            if (!id.HasValue) return NotFound();

            var customer = await GetCustomerById(id.Value);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // Handle POST request to delete a customer
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/DeleteCustomer/{id}";
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

        // Display details of a customer
        public async Task<IActionResult> Details(Guid? id)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null) return redirectResult;

            if (!id.HasValue) return NotFound();

            var customer = await GetCustomerById(id.Value);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // Helper method to get a customer by ID
        private async Task<CustomerViewModel> GetCustomerById(Guid id)
        {
            SetAuthorizationHeader();
            var url = $"{_configuration["ApiBaseUrl"]}Customer/GetCustomerById/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CustomerViewModel>>(await response.Content.ReadAsStringAsync());
                if (apiResponse?.Success == true)
                {
                    return apiResponse.Data;
                }
            }
            return null;
        }
    }
}
