using System.ComponentModel.DataAnnotations;

namespace Users.Domain.Entity
{
	public class EntityBase
	{
		[Key]
		public string Id { get; set; } = Guid.NewGuid().ToString();
	}
}
