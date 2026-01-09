using System.ComponentModel.DataAnnotations;

namespace Users.Domain.Dto
{
	public class LoginDto
	{
		public string Email { get; set; }

		[DataType(DataType.Password)]
		public string Senha { get; set; }
	}
}
