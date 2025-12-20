using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GreenEye.Service
{
    public class SimulationService : ISimulationService
    {

        #region Code_for_PaythonFileLocal
        //private readonly HttpClient _httpClient;
        //private readonly IConfiguration _configuration;
        //private readonly string _pythonInterpreter;
        //private readonly string _scriptPath;

        //public SimulationService(HttpClient httpClient, IConfiguration configuration)
        //{
        //    _httpClient = httpClient;
        //    _configuration = configuration;

        //    _pythonInterpreter = _configuration["PythonSettings:InterpreterPath"] ?? "python";

        //    var scriptName = _configuration["PythonSettings:ScriptPath"] ?? "CropSimulationEngine.py";
        //    _scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, scriptName);
        //}

        //private async Task<ExternalApiResponseDto?> GetDataFromExternalAPi(double longitude, double latitude)
        //{
        //    try
        //    {
        //        var response = await _httpClient.PostAsJsonAsync(
        //            _configuration["ExternalApis:RealTimeDataApi"],
        //            new { longitude, latitude }
        //        );

        //        if (!response.IsSuccessStatusCode) return null;

        //        var content = await response.Content.ReadAsStringAsync();

        //        var jsonRoot = JsonNode.Parse(content);
        //        var dataNode = jsonRoot?["data"];

        //        if (dataNode == null) return null;

        //        var result = dataNode.Deserialize<ExternalApiResponseDto>(
        //            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        //        );

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"External API Error: {ex.Message}");
        //        return null;
        //    }
        //}

        //public async Task<SimulationModelResponseDto> GetSimulationForCropInLocation(double longitude, double latitude, string cropName)
        //{
        //    var externalData = await GetDataFromExternalAPi(longitude, latitude);

        //    object? features = null;
        //    string locationName = "Unknown Location";
        //    string queryTimestamp = DateTime.UtcNow.ToString("o");

        //    if (externalData != null)
        //    {
        //        features = externalData.Features; 

        //        if (externalData.MetaData != null)
        //        {
        //            locationName = externalData.MetaData.LocationName ?? locationName;
        //            queryTimestamp = externalData.MetaData.QueryTimeStamp ?? queryTimestamp;
        //        }
        //    }

        //    var inputData = new
        //    {
        //        cropName = cropName,
        //        latitude = latitude,
        //        longitude = longitude,
        //        features = features
        //    };

        //    var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        //    var jsonInput = JsonSerializer.Serialize(inputData, jsonOptions);

        //    var jsonOutput = RunPythonScript(jsonInput);

        //    try
        //    {
        //        var result = JsonSerializer.Deserialize<SimulationModelResponseDto>(
        //            jsonOutput,
        //            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        //        );

        //        if (result != null)
        //        {
        //            result.LocationName = locationName;
        //            result.QueryTimestamp = queryTimestamp;
        //        }

        //        return result!;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Filed to convert pyhton data {jsonOutput}. Error message{ex.Message}");
        //    }
        //}

        //private string RunPythonScript(string jsonInput)
        //{
        //    if (!File.Exists(_scriptPath))
        //    {
        //        throw new FileNotFoundException($"Python file not found : {_scriptPath}");
        //    }

        //    var startInfo = new ProcessStartInfo
        //    {
        //        FileName = _pythonInterpreter,
        //        Arguments = $"\"{_scriptPath}\"",
        //        RedirectStandardInput = true,
        //        RedirectStandardOutput = true,
        //        RedirectStandardError = true,
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };

        //    using (var process = new Process { StartInfo = startInfo })
        //    {
        //        process.Start();

        //        using (var writer = process.StandardInput)
        //        {
        //            writer.Write(jsonInput);
        //        }

        //        string output = process.StandardOutput.ReadToEnd();
        //        string error = process.StandardError.ReadToEnd();

        //        process.WaitForExit();

        //        if (!string.IsNullOrEmpty(error))
        //        {
        //            throw new Exception($"Python script error: {error}");
        //        }

        //        return output;
        //    }
        //}
        #endregion

        #region Code_For_api
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
            var ExternalSoilData = await GetDataFromExternalAPi(longitude, latitude);

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

        private async Task<ExternalApiResponseDto?> GetDataFromExternalAPi(double longitude, double latitude)
        {

            var response = await _httpClient.PostAsJsonAsync(_configuration["ExternalApis:RealTimeDataApi"], new { longitude, latitude });

            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ExternalApiResponseDto>(content);

            return result;
        }
        #endregion
    }
}
