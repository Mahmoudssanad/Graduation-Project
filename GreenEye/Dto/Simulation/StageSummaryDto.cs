namespace GreenEye.Dto.Simulation
{
    public class StageSummaryDto
    {
        [JsonPropertyName("days")]
        public int Days { get; set; }
        [JsonPropertyName("irrigation")]
        public double Irrigation { get; set; }
        [JsonPropertyName("rainfall")]
        public double Rainfall { get; set; }
        [JsonPropertyName("avg_biomass")]
        public double AvgBioMass { get; set; }
        [JsonPropertyName("avg_temp")]
        public double AvgTemp { get; set; }
    }
}
