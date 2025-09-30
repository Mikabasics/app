using Microsoft.AspNetCore.Mvc;
using WebadminBackend.Models;
using WebadminBackend.Services;

namespace WebadminBackend.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new ApiResponse 
                { 
                    Success = false, 
                    Error = "Benutzername und Passwort sind erforderlich" 
                });
            }

            try
            {
                var loginResult = await _userService.HandleLoginAsync(request.Username, request.Password);
                
                if (loginResult)
                {
                    return Ok(new ApiResponse 
                    { 
                        Success = true 
                    });
                }
                else
                {
                    return Unauthorized(new ApiResponse 
                    { 
                        Success = false, 
                        Error = "Ungültige Anmeldedaten" 
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Fehler: {e.Message}");
                return StatusCode(500, new ApiResponse 
                { 
                    Success = false, 
                    Error = $"Interner Server-Fehler: {e.Message}" 
                });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Givenname) || string.IsNullOrEmpty(request.Surname))
            {
                return BadRequest(new ApiResponse 
                { 
                    Success = false, 
                    Error = "Alle Pflichtfelder müssen ausgefüllt werden" 
                });
            }

            try
            {
                await _userService.HandleRegistrationAsync(request);
                return Ok(new ApiResponse 
                { 
                    Success = true, 
                    Message = "Benutzer erfolgreich registriert" 
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Fehler: {e.Message}");
                return StatusCode(500, new ApiResponse 
                { 
                    Success = false, 
                    Error = e.Message 
                });
            }
        }

        [HttpPost("massregister")]
        public async Task<ActionResult<ApiResponse>> MassRegister([FromBody] MassRegisterRequest request)
        {
            if (request.Mass <= 0 || string.IsNullOrEmpty(request.Right))
            {
                return BadRequest(new ApiResponse 
                { 
                    Success = false, 
                    Error = "Menge und Rechte sind erforderlich" 
                });
            }

            try
            {
                await _userService.HandleMassRegistrationAsync(
                    request.Mass, 
                    request.Right, 
                    request.Password, 
                    request.GenerateEmail
                );

                return Ok(new ApiResponse 
                { 
                    Success = true, 
                    Message = $"{request.Mass} Benutzer erfolgreich registriert" 
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Fehler: {e.Message}");
                return StatusCode(500, new ApiResponse 
                { 
                    Success = false, 
                    Error = e.Message 
                });
            }
        }
    }
}
