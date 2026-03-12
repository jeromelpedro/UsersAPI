using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Users.Application.Interfaces;
using Users.Domain.Dto;

namespace Users.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class LoginController(IJwtService service, ILogger<LoginController>? _logger = null) : ControllerBase
	{
		[HttpPost]
		public async Task<IActionResult> Login([FromQuery] LoginDto login)
		{
			_logger?.LogInformation("Login attempt for email={email} | TraceId:{traceId}", login.Email, HttpContext.TraceIdentifier);

			var token = await service.Autenticar(login.Email, login.Senha);

			if (token is not null)
			{
				_logger?.LogInformation("Login successful for email={email} | TraceId:{traceId}", login.Email, HttpContext.TraceIdentifier);
				return Ok(token);
			}

			_logger?.LogWarning("Login failed for email={email} | TraceId:{traceId}", login.Email, HttpContext.TraceIdentifier);
			return BadRequest("Usuário ou senha incorretos.");
		}
	}
}
