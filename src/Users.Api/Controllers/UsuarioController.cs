using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Users.Application.Interfaces;
using Users.Domain.Dto;

namespace Users.Api.Controllers
{
	[ApiController]
	[Route("api/Users")]
	public class UsuarioController(IUsuarioService service, ILogger<UsuarioController>? _logger = null) : BaseController
	{
		[HttpPost("RegisterUserAdmin")]
		[Authorize(Policy = "Admin")]
		public async Task<IActionResult> CadastrarUsuarioAdmin([FromBody] UsuarioCadastroDto usuario)
		{
			_logger?.LogInformation("CadastrarUsuarioAdmin iniciada para email={email} | TraceId:{traceId}", usuario.Email, HttpContext.TraceIdentifier);

			var result = await service.CriarAsync(usuario, true);

			if (result.Success)
				_logger?.LogInformation("CadastrarUsuarioAdmin sucesso Id={id} | TraceId:{traceId}", result.Result?.Id, HttpContext.TraceIdentifier);
			else
				_logger?.LogWarning("CadastrarUsuarioAdmin falhou para email={email}: {message} | TraceId:{traceId}", usuario.Email, result.Message, HttpContext.TraceIdentifier);

			return result.Success
				? Ok(result.Result)
				: BadRequest(result.Message);
		}

		[HttpPost("RegisterUser")]
		[AllowAnonymous]
		public async Task<IActionResult> CadastrarUsuario([FromBody] UsuarioCadastroDto usuario)
		{
			_logger?.LogInformation("CadastrarUsuario iniciada para email={email} | TraceId:{traceId}", usuario.Email, HttpContext.TraceIdentifier);

			var result = await service.CriarAsync(usuario, false);

			if (result.Success)
				_logger?.LogInformation("CadastrarUsuario sucesso Id={id} | TraceId:{traceId}", result.Result?.Id, HttpContext.TraceIdentifier);
			else
				_logger?.LogWarning("CadastrarUsuario falhou para email={email}: {message} | TraceId:{traceId}", usuario.Email, result.Message, HttpContext.TraceIdentifier);

			return result.Success
				? Ok(result.Result)
				: BadRequest(result.Message);
		}

		[HttpPost("ChangePassword")]
		[AllowAnonymous]
		public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaInputDto input)
		{
			_logger?.LogInformation("AlterarSenha iniciada para IdUsuario={id} | TraceId:{traceId}", input.IdUsuario, HttpContext.TraceIdentifier);

			var result = await service.AlterarSenha(input);

			if (result.Success)
				_logger?.LogInformation("AlterarSenha sucesso para IdUsuario={id} | TraceId:{traceId}", input.IdUsuario, HttpContext.TraceIdentifier);
			else
				_logger?.LogWarning("AlterarSenha falhou para IdUsuario={id}: {message} | TraceId:{traceId}", input.IdUsuario, result.Message, HttpContext.TraceIdentifier);

			return result.Success
				? Ok(result.Message)
				: BadRequest(result.Message);
		}

		[HttpGet("List")]
		public async Task<IActionResult> Listar()
		{
			_logger?.LogInformation("Listar iniciada | TraceId:{traceId}", HttpContext.TraceIdentifier);
			var usuarios = await service.ListarAsync();
			_logger?.LogInformation("Listar finalizada. Count={count} | TraceId:{traceId}", usuarios?.Count, HttpContext.TraceIdentifier);
			return Ok(usuarios);
		}

		[HttpGet("ListById/{id}")]
		public async Task<IActionResult> ListarPorId(string id)
		{
			_logger?.LogInformation("ListarPorId iniciada para id={id} | TraceId:{traceId}", id, HttpContext.TraceIdentifier);
			var usuario = await service.ObterPorIdAsync(id);
			if (usuario is not null)
				_logger?.LogInformation("ListarPorId encontrado id={id} | TraceId:{traceId}", id, HttpContext.TraceIdentifier);
			else
				_logger?.LogWarning("ListarPorId não encontrado id={id} | TraceId:{traceId}", id, HttpContext.TraceIdentifier);

			return usuario is not null
				? Ok(usuario)
				: NotFound("Usuário não encontrado.");
		}        

		[HttpDelete("Delete/{id}")]
		[Authorize(Policy = "Admin")]
		public async Task<IActionResult> Excluir(string id)
		{
			_logger?.LogInformation("Excluir iniciada para id={id} | TraceId:{traceId}", id, HttpContext.TraceIdentifier);
			var result = await service.ExcluirAsync(id);
			if (result.Success)
				_logger?.LogInformation("Excluir sucesso id={id} | TraceId:{traceId}", id, HttpContext.TraceIdentifier);
			else
				_logger?.LogWarning("Excluir falhou id={id}: {message} | TraceId:{traceId}", id, result.Message, HttpContext.TraceIdentifier);

			return result.Success
				? Ok(result.Message)
				: NotFound(result.Message);
		}
	}
}
