using Microsoft.AspNetCore.Mvc;
using RAGChatbot.API.Models;
using RAGChatbot.API.Services;

namespace RAGChatbot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request.Username, request.Password);
            
            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return Unauthorized(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { success = false, message = "Username and password are required" });
            }

            var success = await _authService.RegisterAsync(request.Username, request.Password, request.Email);
            
            if (success)
            {
                return Ok(new { success = true, message = "Registration successful. Please login." });
            }
            else
            {
                return BadRequest(new { success = false, message = "Username already exists" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { success = false, message = "An error occurred during registration" });
        }
    }

    [HttpGet("test")]
    public ActionResult Test()
    {
        return Ok(new { message = "Auth API is working", defaultUser = "admin", defaultPassword = "admin123" });
    }
}
