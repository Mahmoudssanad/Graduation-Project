namespace GreenEye.Dto.Simulation
{
    public class ExternalApiResponseDto
    {
        [JsonPropertyName("success")]
        public bool IsSuccess { get; set; }
        [JsonPropertyName("features")]
        public FeaturesDto Features { get; set; } = new FeaturesDto();
        [JsonPropertyName("metadata")]
        public MetaDataDto MetaData { get; set; } = new MetaDataDto();
    }
}
