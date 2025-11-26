using System.ComponentModel.DataAnnotations;

namespace Users.Domain.Entity
{
	public class Usuario : EntityBase
	{
		[Required, MaxLength(150)]
		public string Nome { get; set; } = string.Empty;

		[Required, EmailAddress, MaxLength(256)]
		public string Email { get; set; } = string.Empty;

		[Required]
		public string Senha { get; set; } = string.Empty;

		[Required, MaxLength(5)]
		public string Role { get; set; } = "User";

		public DateTime DataCriacaoUsuario { get; set; }

		public DateTime DataAlteracaoSenha { get; set; }
	}
}
