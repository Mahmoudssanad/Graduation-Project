namespace GreenEye.Dto.Simulation
{
    public class StressFactorsDto
    {
        [JsonPropertyName("soil")]
        public double Soil { get; set; }
        [JsonPropertyName("water")]
        public double Water { get; set; }
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }
    }
}
