namespace GreenEye.Dto.Simulation
{
    public class SimulationModelRequestDto
    {
        public string CropName { get; set; } = string.Empty;
        public FeaturesDto Features { get; set; } = new FeaturesDto();
    }
}
