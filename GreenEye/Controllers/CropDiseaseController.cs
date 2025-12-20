using GreenEye.Dto.Disease;
using GreenEye.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GreenEye.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    [Authorize] // حماية جميع الـ Endpoints
    public class CropDiseaseController : ControllerBase
    {
        private readonly ICropDiseaseService _cropDiseaseService;

        public CropDiseaseController(ICropDiseaseService cropDiseaseService)
        {
            _cropDiseaseService = cropDiseaseService;
        }

        // 1️. رفع صورة وتشخيص المرض
        // POST /api/CropDisease
        [HttpPost]
        public async Task<IActionResult> CropDisease(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { Message = "Please upload a valid image." });

            try
            {
                // UserId من الـ Token
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var result = await _cropDiseaseService.GetDiseaseFromModelByImage(image, userId);

                if (!result.IsSuccess)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }


        }

        // 2️. جلب History لكل User
        // GET /api/CropDisease/history
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var result = await _cropDiseaseService.GetUserHistoryAsync(userId);

                if (!result.IsSuccess)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }

        // 3️. حذف عنصر من History (Soft Delete)
        // DELETE /api/CropDisease/history/{id}
        [HttpDelete("history/{id}")]
        public async Task<IActionResult> DeleteHistory(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var result = await _cropDiseaseService.DeleteHistoryAsync(id, userId);

                if (!result.IsSuccess)
                    return BadRequest(result);

                return Ok(new { Message = "History item deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
