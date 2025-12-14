using System.ComponentModel.DataAnnotations;

namespace Users.Domain.Entity
{
	public class EntityBase
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();
	}
}
