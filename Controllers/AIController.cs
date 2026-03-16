using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly AIService _aiService;

    public AIController(AIService aiService)
    {
        _aiService = aiService;
    }

    [HttpGet]
    [Route("GenerateMotivationalMessage")]
    public async Task<IActionResult> GenerateMotivationalMessage()
    {
        var message = await _aiService.GenerateMotivationalMessage();
        
        return message != null ? Ok(new { message }) : StatusCode(500, "Falha ao gerar a mensagem motivacional."); 
    }

    [HttpGet]
    [Route("GenerateExerciseRepeat")]
    public async Task<IActionResult> GenerateExerciseRepeat([FromQuery] int exerciseId, [FromQuery] int userId, [FromQuery] int? phaseId)
    {
        var response = await _aiService.GenerateExerciseRepeatAsync(exerciseId, userId, phaseId);
        
        return response != null ? Ok(response) : BadRequest(response);
    }
}