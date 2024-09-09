using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace IOT_UI.Controllers
{
    /// <summary>
    /// Base controller class that provides common functionality
    /// for derived controllers, such as HTTP client management and
    /// setting authorization headers.
    /// </summary>
    public abstract class BaseController : Controller
    {
        // Read-only fields to store HTTP client and configuration settings
        protected readonly HttpClient _httpClient;
        protected readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client used for making requests.</param>
        /// <param name="configuration">The application configuration settings.</param>
        protected BaseController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        /// <summary>
        /// Sets the authorization header for the HTTP client using the JWT token
        /// stored in the session. This method ensures that all outgoing requests
        /// include the proper authentication credentials.
        /// </summary>
        protected void SetAuthorizationHeader()
        {
            // Retrieve the JWT token from the session
            var token = HttpContext.Session.GetString("JWTtoken");

            // Check if token exists and set the authorization header
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                // Optional: Handle cases where token is missing (e.g., log a warning)
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}
