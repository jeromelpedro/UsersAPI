namespace Users.Domain.Dto
{
	public class UserCreatedEventDto
	{
		public Guid Id { get; set; }
		public string Nome { get; set; }
		public string Email { get; set; }
	}
}
