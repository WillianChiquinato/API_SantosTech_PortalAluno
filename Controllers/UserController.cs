using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Services;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // [SEC] all authenticated endpoints require JWT token
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
    [AllowAnonymous]
    [EnableRateLimiting("loginPolicy")]
    [Route("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.GetUserByEmailAndPassword(request.Email!, request.Password!);

        if (user.Errors.Count > 0)
            return BadRequest(user.Errors);

        var token = _tokenService.GenerateToken(user.Result);
        _tokenService.AppendAuthCookie(Response, token, Request.IsHttps);

        return Ok(new
        {
            token,
            user = user.Result
        });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")] // [SEC] only admins can view all users
    [Route("GetAllUsers")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _userService.GetAllAsync();
        if (!response.Success || response.Result == null)
            return NotFound(response);

        return Ok(new
        {
            response.Success,
            response.Errors,
            Result = response.Result.Select(user => user.ToSafeDto()),
            response.TotalRows
        });
    }

    [HttpGet]
    [Route("GetUserById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        if (authenticatedUserId.Value != id && !User.IsInRole("Admin"))
            return Forbid();

        var response = await _userService.GetByIdAsync(id);
        if (!response.Success || response.Result == null)
            return NotFound(response);

        return Ok(new
        {
            response.Success,
            response.Errors,
            Result = response.Result.ToSafeDto()
        });
    }

    [HttpGet]
    [Route("GetProfileData")]
    public async Task<IActionResult> GetProfileData([FromQuery] int enrollmentId)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        // [SEC] IDOR fix: validate authenticated user matches requested ID or is admin
        // if (authenticatedUserId is null || !User.IsInRole("Admin"))
        //     return Forbid();

        var response = await _userService.GetProfileDataAsync(authenticatedUserId.Value, enrollmentId);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPut]
    [Route("UpdateUser")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        // [SEC] extract authenticated user ID from JWT; don't trust client input
        var response = await _userService.UpdateUserAsync(request, authenticatedUserId.Value);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [AllowAnonymous] // [SEC] email verification is public
    [Route("SendEmailVerify")]
    public async Task<IActionResult> SendEmailVerify([FromBody] EmailRequest request)
    {
        var response = await _userService.SendEmailVerifyAsync(request.Email);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [AllowAnonymous] // [SEC] email confirmation is public
    [Route("ConfirmEmailVerify")]
    public async Task<IActionResult> ConfirmEmailVerify([FromBody] ConfirmEmailRequest request)
    {
        var response = await _userService.ConfirmEmailVerifyAsync(request.Email, request.Code);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [AllowAnonymous] // [SEC] password recovery is public
    [Route("SendPasswordRecoveryEmail")]
    public async Task<IActionResult> SendPasswordRecoveryEmail([FromBody] EmailRequest request)
    {
        var response = await _userService.SendPasswordRecoveryEmailAsync(request.Email);
        return response.Success ? Ok(response) : BadRequest(response);
    }
}
