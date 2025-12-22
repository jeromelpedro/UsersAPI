using Users.Domain.Dto;
using Users.Domain.Entity;

namespace Users.Application.Interfaces
{
	public interface IUsuarioService
	{
        Task<Usuario?> ObterPorIdAsync(string id);
        Task<List<Usuario>> ListarAsync();
        Task<(bool Success, string Message, Usuario? Result)> CriarAsync(UsuarioCadastroDto usuario, bool isAdmin);
		Task<(bool Success, string Message)> AlterarSenha(AlterarSenhaInputDto input);
		Task<(bool Success, string Message)> ExcluirAsync(string id);
	}
}
