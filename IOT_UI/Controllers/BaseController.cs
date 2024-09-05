using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace IOT_UI.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly HttpClient _httpClient;
        protected readonly IConfiguration _configuration;

        protected BaseController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        protected void SetAuthorizationHeader()
        {
            var token = HttpContext.Session.GetString("JWTtoken");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
