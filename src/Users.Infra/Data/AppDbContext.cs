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
			// adicionar aqui a connectionString  para executar o migrations
			var connectionString = "";

			if (string.IsNullOrWhiteSpace(connectionString))
				throw new InvalidOperationException(
					"Variável de ambiente 'ConnectionStrings__DefaultConnection' não definida.");

			var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
			optionsBuilder.UseSqlServer(connectionString);

			return new AppDbContext(optionsBuilder.Options);
		}
	}
}
