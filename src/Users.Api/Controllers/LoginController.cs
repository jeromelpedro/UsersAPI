using Microsoft.AspNetCore.Mvc;
using Users.Application.Interfaces;

namespace Users.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class LoginController(IJwtService service) : ControllerBase
	{
		[HttpPost]
		public async Task<IActionResult> Login(string Email, string Senha)
		{
			var token = await service.Autenticar(Email, Senha);

			return token is not null
				? Ok(token)
				: BadRequest("Usuário ou senha incorretos.");
		}
	}
}
