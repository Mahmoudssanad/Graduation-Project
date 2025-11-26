namespace GreenEye.Dto.Simulation
{
    public class FertilizerNeedsDto
    {
        [JsonPropertyName("nitrogen")]
        public double Nitrogen { get; set; }
        [JsonPropertyName("phosphorus")]
        public double Phosphorus { get; set; }
        [JsonPropertyName("potassium")]
        public double Potassium { get; set; }

    }
}
