namespace GreenEye.Dto.Simulation
{
    public class SoilAssessmentDto
    {
        [JsonPropertyName("overall_quality")]
        public double OverAllQuality { get; set; }
        [JsonPropertyName("texture_score")]
        public double TextureScore { get; set; }
        [JsonPropertyName("ph_score")]
        public double PhScore { get; set; }
        [JsonPropertyName("nutrient_score")]
        public double NutrientScore { get; set; }
        [JsonPropertyName("n_adequacy")]
        public double N_Adequacy { get; set; }
        [JsonPropertyName("p_adequacy")]
        public double P_Adequacy { get; set; }
        [JsonPropertyName("k_adequacy")]
        public double K_Adequacy { get; set; }
        [JsonPropertyName("ph_value")]
        public double PhValue { get; set; }
    }
}
