namespace Users.Domain.Dto
{
	public class AlterarSenhaInputDto
	{
		public string IdUsuario { get; set; }
		public string SenhaAntiga { get; set; }
		public string SenhaNova { get; set; }
	}
}
