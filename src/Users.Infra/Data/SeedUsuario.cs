using Microsoft.Extensions.DependencyInjection;
using Users.Domain.Entity;

namespace Users.Infra.Data
{
	public static class SeedUsuario
	{
		public static void Seed(IServiceProvider serviceProvider)
		{
			using var scope = serviceProvider.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			if (!context.Usuarios.Any(u => u.Email == "teste@teste.com"))
			{
				context.Usuarios.Add(new Usuario
				{
					Email = "teste@teste.com",
					Senha = "SenhaForte123!",
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
