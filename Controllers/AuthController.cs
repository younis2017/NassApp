using Microsoft.AspNetCore.Mvc;
using Nass.Helpers;

[ApiController]
[Route("api/[controller]")]
public class AuthController: ControllerBase
    {
    private readonly JwtService _jwtService;

    public AuthController (JwtService jwtService)
        {
        _jwtService = jwtService;
        }

    [HttpPost("login")]
    public IActionResult Login ([FromBody] LoginRequest model)
        {
        // Replace with your real user validation
        if (model.Username == "Wael2026" && model.Password == "Younis2017")
            {
            var token = _jwtService.GenerateTokenSwagger(model.Username);
            return Ok(new { token });
            }

        return Unauthorized("Invalid credentials");
        }
    }

public class LoginRequest
    {
    public string Username { get; set; }
    public string Password { get; set; }
    }
