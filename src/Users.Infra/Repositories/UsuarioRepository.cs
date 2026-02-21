using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Users.Domain.Entity;
using Users.Domain.Interfaces;
using Users.Infra.Data;

namespace Users.Infra.Repositories
{
	public class UsuarioRepository(AppDbContext _db, ILogger<UsuarioRepository>? _logger = null) : IUsuarioRepository
	{
		public async Task<Usuario?> ObterPorIdAsync(string id)
		{
			_logger?.LogDebug("ObterPorIdAsync: buscando id={id}", id);
			return await _db.Usuarios.FindAsync(id);
		}

		public async Task<Usuario?> ObterPorEmailAsync(string email)
		{
			_logger?.LogDebug("ObterPorEmailAsync: buscando email={email}", email);
			return await _db.Usuarios.FirstOrDefaultAsync(x => x.Email == email);
		}

		public async Task<bool> PossuiEmailAsync(string email)
		{
			_logger?.LogDebug("PossuiEmailAsync: verificando email={email}", email);
			return await _db.Usuarios.AnyAsync(u => u.Email == email);
		}

		public async Task<List<Usuario>> ListarAsync()
		{
			_logger?.LogDebug("ListarAsync: obtendo lista de usuários");
			return await _db.Usuarios.ToListAsync();
		}

		public async Task AdicionarAsync(Usuario usuario)
		{
			_logger?.LogInformation("AdicionarAsync: adicionando usuário email={email}", usuario.Email);
			_db.Usuarios.Add(usuario);
			await _db.SaveChangesAsync();
		}

		public async Task AtualizarAsync(Usuario usuario)
		{
			_logger?.LogInformation("AtualizarAsync: atualizando usuário id={id}", usuario.Id);
			_db.Usuarios.Update(usuario);
			await _db.SaveChangesAsync();
		}

		public async Task RemoverAsync(Usuario usuario)
		{
			_logger?.LogInformation("RemoverAsync: removendo usuário id={id}", usuario.Id);
			_db.Usuarios.Remove(usuario);
			await _db.SaveChangesAsync();
		}
	}
}
