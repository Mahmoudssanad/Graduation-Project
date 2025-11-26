namespace GreenEye.Service
{
    public class SimulationService : ISimulationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SimulationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        // needs to make the json property names of response dto similar as Model api (waiting Model api from data team)
        public async Task<SimulationModelResponseDto> GetSimulationForCropInLocation(double longitude, double latitude, string cropName)
        {
            var ExternalSoilData = await  GetDataFromExternalAPi(longitude, latitude);

            if (ExternalSoilData == null) return null!;

            var simulationRequest = new SimulationModelRequestDto
            {
                CropName = cropName,
                Features = ExternalSoilData.Features
            };

            //call model api
            var response = await _httpClient.PostAsJsonAsync(_configuration["ExternalApis:SimulationModelApi"], simulationRequest);

            if (!response.IsSuccessStatusCode) return null!;

            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<SimulationModelResponseDto>(content)!;
        }

        private async Task<ExternalApiResponseDto?> GetDataFromExternalAPi( double longitude , double latitude)
        {

            var response = await _httpClient.PostAsJsonAsync(_configuration["ExternalApis:RealTimeDataApi"], new { longitude, latitude });

            if(!response.IsSuccessStatusCode)  return null;

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ExternalApiResponseDto>(content);

            return result;
        }
    }
}
