using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionOptionController : ControllerBase
{
    private readonly IQuestionOptionService _questionOptionService;

    public QuestionOptionController(IQuestionOptionService questionOptionService)
    {
        _questionOptionService = questionOptionService;
    }

    [HttpGet]
    [Route("GetAllQuestionOptions")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _questionOptionService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetQuestionOptionById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _questionOptionService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
