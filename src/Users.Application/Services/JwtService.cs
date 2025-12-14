using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Users.Application.Interfaces;
using Users.Domain.Entity;
using Users.Domain.Interfaces;
using Users.Application.Utils;

namespace Users.Application.Services
{
	public class JwtService(IConfiguration _config, IUsuarioRepository _usuarioRepository) : IJwtService
	{
		public async Task<string?> Autenticar(string email, string senha)
		{
			var user = await _usuarioRepository.ObterPorEmailAsync(email);

			if (user == null || !senha.Encrypt().Equals(user.Senha))
				return null;

			return GerarToken(user);
		}

		private string GerarToken(Usuario user)
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(ClaimTypes.Role, user.Role),
				new Claim(ClaimTypes.Name, user.Nome),
				new Claim("Id", user.Id.ToString())
			};

			var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddHours(2),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
