namespace Users.Application.Interfaces
{
	public interface IJwtService
	{
		Task<string?> Autenticar(string email, string senha);
	}
}
