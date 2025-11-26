using Microsoft.EntityFrameworkCore;
using Users.Domain.Entity;

namespace Users.Infra.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Usuario> Usuarios { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Usuario>();				
		}
	}
}
