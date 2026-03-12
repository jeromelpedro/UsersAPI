using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Users.Application.Interfaces;
using Users.Application.Validators;
using Users.Domain.Dto;
using Users.Domain.Entity;
using Users.Domain.Interfaces;
using Users.Domain.Interfaces.MessageBus;
using Users.Domain.Utils;

namespace Users.Application.Services
{
    public class UsuarioService(IUsuarioRepository _repository, IServiceBus _serviceBus, IConfiguration configuration, ILogger<UsuarioService>? _logger = null) : IUsuarioService
    {
        public async Task<(bool Success, string Message, Usuario? Result)> CriarAsync(UsuarioCadastroDto usuario, bool isAdmin)
		{
			_logger?.LogInformation("CriarAsync iniciada para email={email}", usuario.Email);
			try
			{
			var emailCheck = UsuarioValidator.ValidarEmail(usuario.Email);
			if (!emailCheck.IsValid)
				{
					_logger?.LogWarning("Validação de email falhou para email={email}: {error}", usuario.Email, emailCheck.ErrorMessage);
					return (false, emailCheck.ErrorMessage, null);
				}

			var senhaCheck = UsuarioValidator.ValidarSenha(usuario.Senha);
			if (!senhaCheck.IsValid)
			{
				_logger?.LogWarning("Validação de senha falhou para email={email}: {error}", usuario.Email, senhaCheck.ErrorMessage);
				return (false, senhaCheck.ErrorMessage, null);
			}

			if (await _repository.PossuiEmailAsync(usuario.Email))
			{
				_logger?.LogInformation("Tentativa de criar usuário com email já cadastrado: {email}", usuario.Email);
				return (false, "E-mail já cadastrado!", null);
			}

			var usuarioEntity = new Usuario
			{
				Nome = usuario.Nome,
				Email = usuario.Email,
				Senha = usuario.Senha.Encrypt(),
				Role = isAdmin ? "Admin" : "User",
				DataCriacaoUsuario = DateTime.Now,
				DataAlteracaoSenha = DateTime.Now,
			};

			await _repository.AdicionarAsync(usuarioEntity);

			_logger?.LogInformation("Usuário criado: Id={id} Email={email}", usuarioEntity.Id, usuarioEntity.Email);

			await _serviceBus.PublishAsync(configuration["ServiceBus:UserCreatedEvent"],
				new UserCreatedEventDto { Id = usuarioEntity.Id, Nome = usuarioEntity.Nome, Email = usuarioEntity.Email });

			_logger?.LogInformation("Evento UserCreated publicado para Id={id}", usuarioEntity.Id);

			return (true, "Usuário criado com sucesso.", usuarioEntity);
			}
			catch(Exception ex)
			{
				_logger?.LogError(ex, "Erro em CriarAsync para email={email}", usuario.Email);
				throw;
			}
		}

		public async Task<(bool Success, string Message)> AlterarSenha(AlterarSenhaInputDto input)
		{
			_logger?.LogInformation("AlterarSenha iniciada para IdUsuario={id}", input.IdUsuario);
			try
			{
				var senhaCheck = UsuarioValidator.ValidarSenha(input.SenhaNova);
				if (!senhaCheck.IsValid)
				{
					_logger?.LogWarning("Nova senha inválida para IdUsuario={id}: {error}", input.IdUsuario, senhaCheck.ErrorMessage);
					return (false, senhaCheck.ErrorMessage);
				}

				var user = await _repository.ObterPorIdAsync(input.IdUsuario);

				if (!user.Senha.Equals(input.SenhaAntiga))
				{
					_logger?.LogWarning("Senha antiga inválida para IdUsuario={id}", input.IdUsuario);
					return (false, "Senha invalida.");
				}

				var validSenha = UsuarioValidator.ValidarSenha(input.SenhaNova);

				if (!validSenha.IsValid)
					return validSenha;

				user.Senha = input.SenhaNova;
				user.DataAlteracaoSenha = DateTime.Now;

				await _repository.AtualizarAsync(user);

				_logger?.LogInformation("Senha alterada com sucesso para IdUsuario={id}", input.IdUsuario);

				return (true, "Senha alterada com sucesso.");
			}
			catch(Exception ex)
			{
				_logger?.LogError(ex, "Erro em AlterarSenha para IdUsuario={id}", input.IdUsuario);
				throw;
			}
		}

		public async Task<List<Usuario>> ListarAsync()
		{
			_logger?.LogInformation("ListarAsync iniciada");
			return await _repository.ListarAsync();
		}

		public async Task<Usuario?> ObterPorIdAsync(string id)
		{
			_logger?.LogInformation("ObterPorIdAsync iniciada para id={id}", id);
			return await _repository.ObterPorIdAsync(id);
		}

		public async Task<(bool Success, string Message)> ExcluirAsync(string id)
		{
			_logger?.LogInformation("ExcluirAsync iniciada para id={id}", id);
			var usuario = await _repository.ObterPorIdAsync(id);

			if (usuario is null)
			{
				_logger?.LogWarning("ExcluirAsync: usuário não encontrado id={id}", id);
				return (false, "Usuário não encontrado.");
			}

			await _repository.RemoverAsync(usuario);
			_logger?.LogInformation("Usuário removido id={id}", id);

			return (true, "Usuário excluído com sucesso.");
		}
	}
}
