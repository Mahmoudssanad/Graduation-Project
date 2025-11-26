namespace GreenEye.Service.IService
{
    public interface ISimulationService
    {
        Task<SimulationModelResponseDto> GetSimulationForCropInLocation(double longtiude , double lat, string cropName);
    }
}
