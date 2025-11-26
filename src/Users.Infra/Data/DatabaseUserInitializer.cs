using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Users.Infra.Data
{
	public static class DatabaseUserInitializer
	{
		public static void EnsureDatabaseUser(IConfiguration configuration)
		{
			// Obter a connection string de setup
			var setupConnectionString = configuration.GetConnectionString("SetupConnection");

			// Criar um DbContextOptionsBuilder para o setup
			var setupOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
			setupOptionsBuilder.UseSqlServer(setupConnectionString);

			// Criar uma instância temporária do DbContext para o setup
			using (var setupContext = new AppDbContext(setupOptionsBuilder.Options))
			{
				// Garante que o banco de dados exista. Se não existir, ele será criado.
				setupContext.Database.EnsureCreated();

				// Aplica as migrações pendentes (se houver)
				if (setupContext.Database.GetPendingMigrations().Any())
					setupContext.Database.Migrate();
				// Informações do login e usuário que queremos criar
				var loginName = "usuario_app";
				var loginPassword = "SenhaForte123!";
				var userName = "usuario_app";
				var databaseName = new SqlConnectionStringBuilder(setupConnectionString).InitialCatalog;

				// Comando para criar o LOGIN no nível do servidor
				var createLoginSql = $@"
                    IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{loginName}')
                    BEGIN
                        CREATE LOGIN [{loginName}] WITH PASSWORD = N'{loginPassword}', CHECK_POLICY = OFF;
                    END";

				// Comando para criar o USER no nível do banco de dados e adicionar à role db_owner
				var createUserSql = $@"
                    USE [{databaseName}];
                    IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'{userName}')
                    BEGIN
                        CREATE USER [{userName}] FOR LOGIN [{loginName}];
                        ALTER ROLE db_owner ADD MEMBER [{userName}];
                    END";

				try
				{
					setupContext.Database.ExecuteSqlRaw(createLoginSql);
					setupContext.Database.ExecuteSqlRaw(createUserSql);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Erro durante a criação do login/usuário do banco de dados: {ex.Message}");
				}
			}
		}
	}
}
