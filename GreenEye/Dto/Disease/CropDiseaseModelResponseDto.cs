namespace GreenEye.Dto.Disease
{
    public class CropDiseaseModelResponseDto
    {
        [JsonPropertyName("Predicted Disease")]
        public string? PredicatedDisease { get; set; }

        [JsonPropertyName("Cause")]
        public string? Cause { get; set; }

        [JsonPropertyName("Peak Season")]
        public string? PeakSeason { get; set; }

        [JsonPropertyName("Remedy")]
        public string? Remedy { get; set; }

        [JsonPropertyName("Confidence")]
        public string? Confidence { get; set; }
    }
}
