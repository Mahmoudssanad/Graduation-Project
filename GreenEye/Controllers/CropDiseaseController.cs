using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GreenEye.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CropDiseaseController(ICropDiseaseService _cropDiseaseService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> CropDisease(IFormFile image)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _cropDiseaseService.GetDiseaseFromModelByImage(image);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
