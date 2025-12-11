namespace GreenEye.Dto.Disease
{
    public class CropDiseaseModelResponseDto
    {
        [JsonPropertyName("predicateddisease")]
        public string? PredicatedDisease { get; set; }

        [JsonPropertyName("cause")]
        public string? Cause { get; set; }

        [JsonPropertyName("peakseason")]
        public string? PeakSeason { get; set; }

        [JsonPropertyName("remedy")]
        public string? Remedy { get; set; }
    }
}
