using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Users.Domain.Entity;

namespace Users.Infra.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Usuario>();
			modelBuilder.Entity<AuditLog>(b =>
			{
				b.Property(a => a.Timestamp).HasDefaultValueSql("GETUTCDATE()");
				b.Property(a => a.TableName).IsRequired();
				b.Property(a => a.Action).IsRequired();
			});
		}
	}

	public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
	{
		public AppDbContext CreateDbContext(string[] args)
		{
			// Aqui vamos pegar a connection string de variável de ambiente,
			// que é simples, seguro e comum no mercado.

			var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

			if (string.IsNullOrWhiteSpace(connectionString))
				throw new InvalidOperationException(
					"Variável de ambiente 'ConnectionStrings__DefaultConnection' não definida.");

			var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
			optionsBuilder.UseSqlServer("Server=(LocalDB)\\MSSQLLocalDB;Database=dbUser;User Id=usuario_app;Password=SenhaForte123!;TrustServerCertificate=True;");

			return new AppDbContext(optionsBuilder.Options);
		}
	}
}
