using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Users.Application.Interfaces;
using Users.Domain.Entity;
using Users.Domain.Interfaces;
using Users.Domain.Utils;
using Users.Infra.Data;

namespace Users.Application.Services
{
	public class JwtService(
		IConfiguration _config,
		IUsuarioRepository _usuarioRepository,
		RedisConnection? _redisConnection = null,
		ILogger<JwtService>? _logger = null) : IJwtService
	{
		public async Task<string?> Autenticar(string email, string senha)
		{
			_logger?.LogInformation("Autenticar iniciada para email={email} | TraceId:{traceId}", email, Environment.NewLine);

			var encryptedSenha = senha.Encrypt();
			var cacheKey = $"{email}_{encryptedSenha}";
			var redisDb = GetRedisDatabase();

			if (redisDb != null)
			{
				var cachedUser = await GetUsuarioFromCacheAsync(redisDb, cacheKey);
				if (cachedUser != null)
				{
					_logger?.LogInformation("Autenticar via cache para email={email} Id={id}", email, cachedUser.Id);
					return GerarToken(cachedUser);
				}
			}

			var user = await _usuarioRepository.ObterPorEmailAsync(email);

			if (user == null)
			{
				_logger?.LogWarning("Autenticar: usuário não encontrado para email={email}", email);
				return null;
			}

			if (!encryptedSenha.Equals(user.Senha))
			{
				_logger?.LogWarning("Autenticar: senha inválida para email={email}", email);
				return null;
			}

			if (redisDb != null)
			{
				await SaveUsuarioInCacheAsync(redisDb, cacheKey, user);
			}

			_logger?.LogInformation("Autenticar sucesso para email={email} Id={id}", email, user.Id);
			return GerarToken(user);
		}

		private IDatabase? GetRedisDatabase()
		{
			try
			{
				return _redisConnection?.GetConnection().GetDatabase();
			}
			catch (Exception ex)
			{
				_logger?.LogWarning(ex, "Falha ao obter conexão Redis. Fluxo seguirá sem cache.");
				return null;
			}
		}

		private async Task<Usuario?> GetUsuarioFromCacheAsync(IDatabase redisDb, string cacheKey)
		{
			try
			{
				var cachedValue = await redisDb.StringGetAsync(cacheKey);
				if (cachedValue.IsNullOrEmpty)
				{
					return null;
				}

				return JsonSerializer.Deserialize<Usuario>(cachedValue!);
			}
			catch (Exception ex)
			{
				_logger?.LogWarning(ex, "Falha ao ler cache Redis para chave={cacheKey}", cacheKey);
				return null;
			}
		}

		private async Task SaveUsuarioInCacheAsync(IDatabase redisDb, string cacheKey, Usuario user)
		{
			try
			{
				var serializedUser = JsonSerializer.Serialize(user);
				await redisDb.StringSetAsync(cacheKey, serializedUser, TimeSpan.FromHours(1));
			}
			catch (Exception ex)
			{
				_logger?.LogWarning(ex, "Falha ao salvar usuário em cache Redis para chave={cacheKey}", cacheKey);
			}
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
