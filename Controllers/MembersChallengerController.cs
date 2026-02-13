using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersChallengerController : ControllerBase
{
    private readonly IMembersChallengerService _membersChallengerService;

    public MembersChallengerController(IMembersChallengerService membersChallengerService)
    {
        _membersChallengerService = membersChallengerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _membersChallengerService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _membersChallengerService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
