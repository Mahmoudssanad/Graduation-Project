namespace GreenEye.Dto.Disease
{
    public class CropDiseaseHistoryDto
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("Image Url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("Predicted Disease")]
        public string? PredicatedDisease { get; set; }

        [JsonPropertyName("Cause")]
        public string?  Cause { get; set; }

        [JsonPropertyName("Peak Season")]
        public string? PeakSeason { get; set; }

        [JsonPropertyName("Remedy")]
        public string? Remedy { get; set; }

        [JsonPropertyName("Confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("Sent At")]
        public DateTime SentAt { get; set; }
    }
}
