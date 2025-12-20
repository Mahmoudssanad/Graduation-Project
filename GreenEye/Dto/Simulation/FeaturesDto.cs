namespace GreenEye.Dto.Simulation
{
    public class FeaturesDto
    {
        [JsonPropertyName("sand")]
        public double? Sand { get; set; }
        [JsonPropertyName("silt")]
        public double? Silt { get; set; }
        [JsonPropertyName("clay")]
        public double? Clay { get; set; }
        [JsonPropertyName("soc")]
        public double? Soc { get; set; }
        [JsonPropertyName("ph")]
        public double? Ph { get; set; }
        [JsonPropertyName("bdod")]
        public double? Bdod { get; set; }
        [JsonPropertyName("cec")]
        public double? Cec { get; set; }
        [JsonPropertyName("ndvi")]
        public double? NDVI { get; set; }
        [JsonPropertyName("t2m_c")]
        public double? T2m_c { get; set; }
        [JsonPropertyName("td2m_c")]
        public double? Td2m_c { get; set; }
        [JsonPropertyName("rh_pct")]
        public double? Rh_pct { get; set; }
        [JsonPropertyName("tp_m")]
        public double? Tp_m { get; set; }
        [JsonPropertyName("ssrd_jm2")]
        public double? Ssrd_jm2 { get; set; }
        [JsonPropertyName("lc_type1")]
        public double? Lc_type1 { get; set; }
        [JsonPropertyName("nitrogen")]
        public double? Nitrogen { get; set; }
        [JsonPropertyName("phosphorus")]
        public double? Phosphorus { get; set; }
        [JsonPropertyName("potassium")]
        public double? Potassium { get; set; }
        //[JsonPropertyName("temperature")]
        //public double? Temperature { get; set; }
        //[JsonPropertyName("humidity")]
        //public double? Humidity { get; set; }
        //[JsonPropertyName("precipitation")]
        //public double? Precipitation { get; set; }
        //[JsonPropertyName("solar_radiation")]
        //public double? SolarRadiation { get; set; }
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }
}
