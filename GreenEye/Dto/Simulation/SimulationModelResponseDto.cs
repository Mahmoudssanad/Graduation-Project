namespace GreenEye.Dto.Simulation
{
    public class SimulationModelResponseDto
    {
        [JsonPropertyName("crop")]
        public string? CropName { get; set; }
        [JsonPropertyName("predicted_yield")]
        public double? PredictedYield { get; set; }
        [JsonPropertyName("biomass")]
        public List<double> Biomass { get; set; } = new List<double>();
        [JsonPropertyName("swc")]
        public List<double> SoilWaterContent { get; set; } = new List<double>();
        [JsonPropertyName("et0_daily")]
        public List<double> et0_daily { get; set; } = new List<double>();
        [JsonPropertyName("water_stress")]
        public List<double> WaterStress { get; set; } = new List<double>();
        [JsonPropertyName("temperature_stress")]
        public List<double> TemperatureStress { get; set; } = new List<double>();
        [JsonPropertyName("days")]
        public List<double> Days { get; set; } = new List<double>();
        
        [JsonPropertyName("growth_stages")]
        public List<double> GrowthStages { get; set; } = new List<double>();
        [JsonPropertyName("stage_summary")]
        public Dictionary<string, StageSummaryDto> StageSummary { get; set; } = new Dictionary<string, StageSummaryDto>();
        [JsonPropertyName("soil_assessment")]
        public SoilAssessmentDto SoilAssessment { get; set; } = new SoilAssessmentDto();
        [JsonPropertyName("irrigation_schedule")]
        public List<double> IrrigationSchedule { get; set; } = new List<double>();
        [JsonPropertyName("total_irrigation")]
        public double TotalIrrigation { get; set; }
        [JsonPropertyName("total_rainfall")]
        public double TotalRainfall { get; set; }
        [JsonPropertyName("fertilizer_needs")]
        public FertilizerNeedsDto FertilizerNeeds { get; set; } = new FertilizerNeedsDto();
        public StressFactorsDto StressFactors { get; set; } = new StressFactorsDto();
        public List<double> GrowingDegreeDays { get; set; } = new List<double>();

    }
}
