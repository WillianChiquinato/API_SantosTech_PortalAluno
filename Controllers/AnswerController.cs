using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // [SEC] all endpoints require authenticated user
public class AnswerController : ControllerBase
{
    private readonly IAnswerService _answerService;

    public AnswerController(IAnswerService answerService)
    {
        _answerService = answerService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")] // [SEC] full answer listings are restricted to privileged roles
    [Route("GetAllAnswers")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _answerService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")] // [SEC] direct answer lookup is restricted to privileged roles
    [Route("GetAnswerById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _answerService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetAnswersByUserId")]
    public async Task<IActionResult> GetByUserId()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _answerService.GetByUserIdAsync(authenticatedUserId.Value);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
