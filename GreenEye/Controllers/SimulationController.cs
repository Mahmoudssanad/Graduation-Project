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
            try
            {
                var result = await _simulationService.GetSimulationForCropInLocation(longitude, latitude, CropName);

                if (result == null)
                    return Ok(new GeneralResponse<SimulationModelResponseDto>
                    {
                        IsSuccess = false,
                        Message = "Invalid Operation Please Try Again!"
                    });

                return Ok(new GeneralResponse<SimulationModelResponseDto>
                {
                    IsSuccess = true,
                    Data = result
                });
            }
            catch(Exception ex)
            {
                return BadRequest(new GeneralResponse<SimulationModelResponseDto>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }
    }
}
