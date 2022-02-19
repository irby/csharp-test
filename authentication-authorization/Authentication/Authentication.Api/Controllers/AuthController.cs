using System.Threading.Tasks;
using Authentication.Core.Models.Authentication;
using Authentication.Core.Models.Dto;
using Authentication.Core.Utilities;
using Authentication.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly ILogger<AuthController> _logger;
        private readonly AuthenticationService _authService;
        private readonly AuthenticationConfiguration _authConfig;

        public AuthController(ILogger<AuthController> logger, AuthenticationService authService, AuthenticationConfiguration authConfig)
        {
            _logger = logger;
            _authService = authService;
            _authConfig = authConfig;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var user = await _authService.LoginUserAsync(dto.Email, dto.Password);
            var tokenResponse = AuthTokenUtil.CreateToken(user, new SigningCredentials(_authConfig.SigningCredentials.Key, SecurityAlgorithms.HmacSha256Signature));
            return Ok(tokenResponse);
        }
    }
}