using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Users.Application.Validators
{
	public static class UsuarioValidator
	{
		public static (bool IsValid, string ErrorMessage) ValidarEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return (false, "O e-mail é obrigatório.");

			var emailAttr = new EmailAddressAttribute();
			if (!emailAttr.IsValid(email))
				return (false, "Formato de e-mail inválido.");

			return (true, string.Empty);
		}

		public static (bool IsValid, string ErrorMessage) ValidarSenha(string senha)
		{
			if (string.IsNullOrWhiteSpace(senha))
				return (false, "A senha é obrigatória.");

			if (senha.Length < 8)
				return (false, "A senha deve ter no mínimo 8 caracteres.");

			if (!Regex.IsMatch(senha, @"[A-Z]"))
				return (false, "A senha deve conter pelo menos uma letra maiúscula.");

			if (!Regex.IsMatch(senha, @"[a-z]"))
				return (false, "A senha deve conter pelo menos uma letra minúscula.");

			if (!Regex.IsMatch(senha, @"[0-9]"))
				return (false, "A senha deve conter pelo menos um número.");

			if (!Regex.IsMatch(senha, @"[\W_]"))
				return (false, "A senha deve conter pelo menos um caractere especial.");

			return (true, string.Empty);
		}
	}
}
