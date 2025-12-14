using Users.Application.Interfaces;
using Users.Application.Utils;
using Users.Application.Validators;
using Users.Domain.Dto;
using Users.Domain.Entity;
using Users.Domain.Interfaces;
using Users.Domain.Interfaces.MessageBus;

namespace Users.Application.Services
{
	public class UsuarioService(IUsuarioRepository _repository, IRabbitMqPublisher _mqPublisher) : IUsuarioService
	{
		public async Task<(bool Success, string Message, object Result)> CriarAsync(UsuarioCadastroDto usuario, bool isAdmin)
		{
			var emailCheck = UsuarioValidator.ValidarEmail(usuario.Email);
			if (!emailCheck.IsValid)
				return (false, emailCheck.ErrorMessage, null);

			var senhaCheck = UsuarioValidator.ValidarSenha(usuario.Senha);
			if (!senhaCheck.IsValid)
				return (false, senhaCheck.ErrorMessage, null);

			if (await _repository.PossuiEmailAsync(usuario.Email))
				return (false, "E-mail já cadastrado!", null);

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

			await _mqPublisher.PublishAsync("UserCreatedEvent", 
				new UserCreatedEventDto {  Id = usuarioEntity.Id, Nome = usuarioEntity.Nome, Email = usuarioEntity.Email });

			return (true, "Usuário criado com sucesso.", new { usuarioEntity.Id });
		}

		public async Task<(bool Success, string Message)> AlterarSenha(AlterarSenhaInputDto input)
		{
			var senhaCheck = UsuarioValidator.ValidarSenha(input.SenhaNova);
			if (!senhaCheck.IsValid)
				return (false, senhaCheck.ErrorMessage);

			var user = await _repository.ObterPorIdAsync(input.IdUsuario);

			if (!user.Senha.Equals(input.SenhaAntiga))
				return (false, "Senha invalida.");

			var validSenha = UsuarioValidator.ValidarSenha(input.SenhaNova);

			if (!validSenha.IsValid)
				return validSenha;

			user.Senha = input.SenhaNova;
			user.DataAlteracaoSenha = DateTime.Now;

			await _repository.AtualizarAsync(user);

			return (true, "Senha alterada com sucesso.");
		}

		public async Task<List<Usuario>> ListarAsync() =>
			await _repository.ListarAsync();

		public async Task<Usuario?> ObterPorIdAsync(string id) =>
			await _repository.ObterPorIdAsync(id);

		public async Task<(bool Success, string Message)> ExcluirAsync(string id)
		{
			var usuario = await _repository.ObterPorIdAsync(id);

			if (usuario is null)
				return (false, "Usuário não encontrado.");

			await _repository.RemoverAsync(usuario);

			return (true, "Usuário excluído com sucesso.");
		}
	}
}
