using System.Text.Json;
using WinterWay.Models.DTOs.Responses;

namespace WinterWay.Services
{
    public class BackgroundImageService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        private readonly string? _backgroundServerURL;
        public BackgroundFullDataDTO? BackgroundData { get; private set; }
            
        public BackgroundImageService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            
            _backgroundServerURL = _config.GetValue<string>("BackgroundServerURL");
        }
        
        public async Task GetBackgroundData()
        {
            if (_backgroundServerURL == null)
            {
                throw new Exception("ERROR: No image server URL provided");
            }

            try
            {
                var response = await _httpClient.GetAsync(_backgroundServerURL);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"ERROR: Image server returned {(int)response.StatusCode}: {response.ReasonPhrase}"
                    );
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var backgroundPartData = JsonSerializer.Deserialize<BackgroundResponseDTO>(responseData);

                if (backgroundPartData == null || backgroundPartData.Name != "WinterWay-Images")
                {
                    throw new Exception($"ERROR: Incorrect image server response");
                }

                BackgroundData = new BackgroundFullDataDTO(backgroundPartData, _backgroundServerURL);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data: {ex.Message}");
                throw new Exception($"ERROR: Image server request error");
            }
        }
    }
}