namespace GreenEye.Dto.Simulation
{
    public class MetaDataDto
    {
        [JsonPropertyName("location_name")]
        public string? LocationName { get; set; }
        [JsonPropertyName("query_timestamp")]
        public string QueryTimeStamp { get; set; } = string.Empty;
    }
}
