using Microsoft.EntityFrameworkCore;
using Users.Domain.Entity;
using Users.Domain.Interfaces;
using Users.Infra.Data;

namespace Users.Infra.Repositories
{
	public class UsuarioRepository(AppDbContext _db) : IUsuarioRepository
	{
		public async Task<Usuario?> ObterPorIdAsync(string id)
		{
			return await _db.Usuarios.FindAsync(id);
		}

		public async Task<Usuario?> ObterPorEmailAsync(string email)
		{
			return await _db.Usuarios.FirstOrDefaultAsync(x => x.Email == email);
		}

		public async Task<bool> PossuiEmailAsync(string email)
		{
			return await _db.Usuarios.AnyAsync(u => u.Email == email);
		}

		public async Task<List<Usuario>> ListarAsync()
		{
			return await _db.Usuarios.ToListAsync();
		}

		public async Task AdicionarAsync(Usuario usuario)
		{
			_db.Usuarios.Add(usuario);
			await _db.SaveChangesAsync();
		}

		public async Task AtualizarAsync(Usuario usuario)
		{
			_db.Usuarios.Update(usuario);
			await _db.SaveChangesAsync();
		}

		public async Task RemoverAsync(Usuario usuario)
		{
			_db.Usuarios.Remove(usuario);
			await _db.SaveChangesAsync();
		}
	}
}
