using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Domain.Entity;
using Users.Domain.Utils;

namespace Users.Infra.Data
{
	public static class SeedUsuario
	{
		public static void Seed(IServiceProvider serviceProvider)
		{
			using var scope = serviceProvider.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			if (!context.Usuarios.Any(u => u.Email == "teste@cloudgames.com.br"))
			{
				var senha = "SenhaForte123!".Encrypt();

				context.Usuarios.Add(new Usuario
				{
					Email = "teste@cloudgames.com.br",
					Senha = senha,
					Nome = "Administrador",
					Role = "Admin",
					DataCriacaoUsuario = DateTime.Now,
					DataAlteracaoSenha = DateTime.Now,
				});
				context.SaveChanges();
			}
		}
	}
}
