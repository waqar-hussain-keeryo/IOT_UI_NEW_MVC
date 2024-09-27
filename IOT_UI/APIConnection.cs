namespace IOT_UI
{
    public class APIConnection
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public APIConnection(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        // Method to check API connectivity
        public async Task<bool> IsApiConnected()
        {
            try
            {
                // Adjust endpoint as needed
                var response = await _httpClient.GetAsync($"{_configuration["ApiBaseUrl"]}Dashboard");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API connectivity check failed: {ex.Message}");
                return false;
            }
        }
    }
}