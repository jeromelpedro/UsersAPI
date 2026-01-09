using Microsoft.AspNetCore.Mvc;
using Users.Application.Interfaces;
using Users.Domain.Dto;

namespace Users.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class LoginController(IJwtService service) : ControllerBase
	{
		[HttpPost]
		public async Task<IActionResult> Login([FromQuery] LoginDto login)
		{
			var token = await service.Autenticar(login.Email, login.Senha);

			return token is not null
				? Ok(token)
				: BadRequest("Usuário ou senha incorretos.");
		}
	}
}
