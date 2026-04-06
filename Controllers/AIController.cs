using API_PortalSantosTech.Services;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // [SEC] AI endpoints use authenticated user context
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
    public async Task<IActionResult> GenerateExerciseRepeat([FromQuery] int exerciseId, [FromQuery] int? phaseId)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _aiService.GenerateExerciseRepeatAsync(exerciseId, authenticatedUserId.Value, phaseId);
        return response != null ? Ok(response) : BadRequest(response);
    }
}
