using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnswerController : ControllerBase
{
    private readonly IAnswerService _answerService;

    public AnswerController(IAnswerService answerService)
    {
        _answerService = answerService;
    }

    [HttpGet]
    [Route("GetAllAnswers")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _answerService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetAnswerById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _answerService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}