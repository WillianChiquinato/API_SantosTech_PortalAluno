using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly TokenService _tokenService;

    public UserController(IUserService userService, TokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpPost]
    [EnableRateLimiting("loginPolicy")]
    [Route("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.GetUserByEmailAndPassword(request.Email, request.Password);

        if (user.Errors.Count > 0)
        {
            return BadRequest(user.Errors);
        }

        var token = _tokenService.GenerateToken(user.Result);

        return Ok(new
        {
            token,
            user = new
            {
                user.Result.Id,
                user.Result.Name,
                user.Result.Email,
                user.Result.Role,
                user.Result.ProfilePictureUrl
            }
        });
    }

    [HttpGet]
    [Route("GetAllUsers")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _userService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetUserById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _userService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetProfileData")]
    public async Task<IActionResult> GetProfileData([FromQuery] int userid)
    {
        var response = await _userService.GetProfileDataAsync(userid);
        
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPut]
    [Route("UpdateUser")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
    {
        var response = await _userService.UpdateUserAsync(request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [Route("SendEmailVerify")]
    public async Task<IActionResult> SendEmailVerify([FromBody] EmailRequest request)
    {
        var response = await _userService.SendEmailVerifyAsync(request.Email);
        return response.Success ? Ok(response) : BadRequest(response);
    }
}
