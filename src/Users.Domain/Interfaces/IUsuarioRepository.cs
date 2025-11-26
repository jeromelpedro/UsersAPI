using Users.Domain.Entity;

namespace Users.Domain.Interfaces
{
	public interface IUsuarioRepository
	{
		Task<Usuario?> ObterPorIdAsync(string id);
		Task<List<Usuario>> ListarAsync();
		Task AdicionarAsync(Usuario usuario);
		Task AtualizarAsync(Usuario usuario);
		Task RemoverAsync(Usuario usuario);
		Task<bool> PossuiEmailAsync(string email);
		Task<Usuario?> ObterPorEmailAsync(string email);
	}
}
