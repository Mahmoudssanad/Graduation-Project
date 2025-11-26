namespace GreenEye.Dto.Simulation
{
    public class FeaturesDto
    {
        [JsonPropertyName("sand")]
        public double? Sand { get; set; }
        [JsonPropertyName("clay")]
        public double? Clay { get; set; }
        [JsonPropertyName("soc")]
        public double? Soc { get; set; }
        [JsonPropertyName("ph")]
        public double? Ph { get; set; }
        [JsonPropertyName("ces")]
        public double? Cec { get; set; }
        [JsonPropertyName("nitrogen")]

        public double? Nitrogen { get; set; }
        [JsonPropertyName("phosphorus")]
        public double? Phosphorus { get; set; }
        [JsonPropertyName("potassium")]
        public double? Potassium { get; set; }
        [JsonPropertyName("ndvi")]
        public double? NDVI { get; set; }
        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }
        [JsonPropertyName("humidity")]
        public double? Humidity { get; set; }
        [JsonPropertyName("precipitation")]
        public double? Precipitation { get; set; }
        [JsonPropertyName("solar_radiation")]
        public double? SolarRadiation { get; set; }
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }
}
