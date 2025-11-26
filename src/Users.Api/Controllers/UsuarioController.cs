using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Interfaces;
using Users.Domain.Dto;

namespace Users.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsuarioController(IUsuarioService service) : BaseController
	{
		[HttpPost("CadastrarUsuarioAdmin")]
		[Authorize(Policy = "Admin")]
		public async Task<IActionResult> CadastrarUsuarioAdmin([FromBody] UsuarioCadastroDto usuario)
		{
			var result = await service.CriarAsync(usuario, true);

			return result.Success
				? Ok(result.Result)
				: BadRequest(result.Message);
		}

		[HttpPost("CadastrarUsuario")]
		public async Task<IActionResult> CadastrarUsuario([FromBody] UsuarioCadastroDto usuario)
		{
			var result = await service.CriarAsync(usuario, false);

			return result.Success
				? Ok(result.Result)
				: BadRequest(result.Message);
		}

		[HttpPost("AlterarSenha")]
		[AllowAnonymous]
		public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaInputDto input)
		{
			var result = await service.AlterarSenha(input);

			return result.Success
				? Ok(result.Message)
				: BadRequest(result.Message);
		}

		[AllowAnonymous]
		[HttpGet("Listar")]
		public async Task<IActionResult> Listar()
		{
			var usuarios = await service.ListarAsync();
			return Ok(usuarios);
		}

		[HttpGet("ListarPorId/{id}")]
		public async Task<IActionResult> ListarPorId(string id)
		{
			var usuario = await service.ObterPorIdAsync(id);
			return usuario is not null
				? Ok(usuario)
				: NotFound("Usuário não encontrado.");
		}		

		[HttpDelete("Excluir/{id}")]
		[Authorize(Policy = "Admin")]
		public async Task<IActionResult> Excluir(string id)
		{
			var result = await service.ExcluirAsync(id);
			return result.Success
				? Ok(result.Message)
				: NotFound(result.Message);
		}
	}
}
