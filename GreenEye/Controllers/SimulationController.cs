using GreenEye.Dto.Responses;
using System.Threading.Tasks;

namespace GreenEye.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimulationController : ControllerBase
    {
        private readonly ISimulationService _simulationService;

        public SimulationController(ISimulationService simulationService)
        {
            _simulationService = simulationService;
        }
        [HttpGet]
        public async Task<ActionResult<GeneralResponse<SimulationModelResponseDto>>> GetSimulation(double longitude, double latitude, string CropName)
        {
            if (string.IsNullOrWhiteSpace(CropName))
            {
                return BadRequest(new GeneralResponse<SimulationModelResponseDto>
                {
                    IsSuccess = false,
                    Message = "Crop Name is required."
                });
            }

            try
            {
                var result = await _simulationService.GetSimulationForCropInLocation(longitude, latitude, CropName);

                if (result == null)
                {
                    return Ok(new GeneralResponse<SimulationModelResponseDto>
                    {
                        IsSuccess = false,
                        Message = "Failed to run simulation. Please check inputs or external data."
                    });
                }

                return Ok(new GeneralResponse<SimulationModelResponseDto>
                {
                    IsSuccess = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralResponse<SimulationModelResponseDto>
                {
                    IsSuccess = false,
                    Message = "An error occurred: " + ex.Message
                });
            }
        }
    }
}