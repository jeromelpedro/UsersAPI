using System;
using Moq;
using Users.Application.Services;
using Users.Domain.Dto;
using Users.Domain.Entity;
using Users.Domain.Interfaces;
using Users.Domain.Interfaces.MessageBus;

namespace Users.Tests.Services
{
	public class UsuarioServiceTests
	{
		private Mock<IUsuarioRepository> _repositoryMock;
		private Mock<IRabbitMqPublisher> _rabbitMqPublisher;
		private UsuarioService _service;

		public UsuarioServiceTests()
		{
			// Define uma chave padrão para os testes (32 bytes em Base64)
			Environment.SetEnvironmentVariable("Secrets__Password", Convert.ToBase64String(new byte[32]));
			_repositoryMock = new Mock<IUsuarioRepository>();
			_rabbitMqPublisher = new Mock<IRabbitMqPublisher>();
			_service = new UsuarioService(_repositoryMock.Object, _rabbitMqPublisher.Object);
		}

		[Fact]
		public async Task CriarAsync_DeveCriarUsuario_QuandoEmailNaoExiste()
		{
			var usuarioDto = new UsuarioCadastroDto
			{
				Nome = "Novo Usuário",
				Email = "novo@teste.com",
				Senha = "SenhaForte1!"
			};

			_repositoryMock.Setup(r => r.PossuiEmailAsync(usuarioDto.Email))
				.ReturnsAsync(false);

			_repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>()))
				.Returns(Task.CompletedTask);

			var result = await _service.CriarAsync(usuarioDto, false);

			Assert.True(result.Success);
			Assert.NotNull(result.Result);
			Assert.Equal(usuarioDto.Email, result.Result.Email);
			Assert.Equal("User", result.Result.Role);
			Assert.Equal("Usuário criado com sucesso.", result.Message);

			_repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>()), Times.Once);
		}

		[Fact]
		public async Task CriarAsync_DeveCriarUsuarioAdmin_QuandoIsAdminEhTrue()
		{
			var usuarioDto = new UsuarioCadastroDto
			{
				Nome = "Admin Usuário",
				Email = "admin@teste.com",
				Senha = "SenhaForte1!"
			};

			_repositoryMock.Setup(r => r.PossuiEmailAsync(usuarioDto.Email))
				.ReturnsAsync(false);

			_repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>()))
				.Returns(Task.CompletedTask);

			var result = await _service.CriarAsync(usuarioDto, true);

			Assert.True(result.Success);
			Assert.NotNull(result.Result);
			Assert.Equal(usuarioDto.Email, result.Result.Email);
			Assert.Equal("Admin", result.Result.Role);
			Assert.Equal("Usuário criado com sucesso.", result.Message);

			_repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>()), Times.Once);
		}

		[Fact]
		public async Task CriarAsync_DeveFalhar_QuandoEmailDuplicado()
		{
			var usuarioDto = new UsuarioCadastroDto
			{
				Nome = "Outro Usuário",
				Email = "existente@teste.com",
				Senha = "SenhaForte1!"
			};

			_repositoryMock.Setup(r => r.PossuiEmailAsync(usuarioDto.Email))
				.ReturnsAsync(true);

			var result = await _service.CriarAsync(usuarioDto, false);

			Assert.False(result.Success);
			Assert.Null(result.Result);
			Assert.Equal("E-mail já cadastrado!", result.Message);

			_repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>()), Times.Never);
		}

		[Fact]
		public async Task AlterarSenha_DeveAlterarSenha_QuandoDadosValidos()
		{
			var input = new AlterarSenhaInputDto
			{
				IdUsuario = "1",
				SenhaAntiga = "SenhaAntiga1!",
				SenhaNova = "NovaSenha1!"
			};

			var usuario = new Usuario
			{
				Id = "1",
				Nome = "Usuário",
				Email = "usuario@teste.com",
				Senha = "SenhaAntiga1!",
				Role = "User"
			};

			_repositoryMock.Setup(r => r.ObterPorIdAsync(input.IdUsuario))
				.ReturnsAsync(usuario);

			_repositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<Usuario>()))
				.Returns(Task.CompletedTask);

			var result = await _service.AlterarSenha(input);

			Assert.True(result.Success);
			Assert.Equal("Senha alterada com sucesso.", result.Message);

			_repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Usuario>()), Times.Once);
		}

		[Fact]
		public async Task AlterarSenha_DeveFalhar_QuandoSenhaAntigaInvalida()
		{
			var input = new AlterarSenhaInputDto
			{
				IdUsuario = "1",
				SenhaAntiga = "NovaSenha1!",
				SenhaNova = "OutraNova1!"
			};

			var usuario = new Usuario
			{
				Id = "1",
				Nome = "Usuário",
				Email = "usuario@teste.com",
				Senha = "SenhaDiferente1!",
				Role = "User"
			};

			_repositoryMock.Setup(r => r.ObterPorIdAsync(input.IdUsuario))
				.ReturnsAsync(usuario);

			var result = await _service.AlterarSenha(input);

			Assert.False(result.Success);
			Assert.Equal("Senha invalida.", result.Message);

			_repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Usuario>()), Times.Never);
		}

		[Fact]
		public async Task ListarAsync_DeveRetornarTodosUsuarios()
		{
			var usuarios = new List<Usuario>
			{
				new Usuario { Id = "1", Nome = "A", Email = "a@a.com", Senha = "Senha1!", Role = "User" },
				new Usuario { Id = "2", Nome = "B", Email = "b@b.com", Senha = "Senha2!", Role = "User" }
			};

			_repositoryMock.Setup(r => r.ListarAsync())
				.ReturnsAsync(usuarios);

			var lista = await _service.ListarAsync();

			Assert.Equal(2, lista.Count);
			Assert.Contains(lista, u => u.Email == "a@a.com");
			Assert.Contains(lista, u => u.Email == "b@b.com");
		}

		[Fact]
		public async Task ObterPorIdAsync_DeveRetornarUsuario_QuandoIdExiste()
		{
			var usuario = new Usuario
			{
				Id = "3",
				Nome = "C",
				Email = "c@c.com",
				Senha = "Senha3!",
				Role = "User"
			};

			_repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id))
				.ReturnsAsync(usuario);

			var encontrado = await _service.ObterPorIdAsync(usuario.Id);

			Assert.NotNull(encontrado);
			Assert.Equal(usuario.Email, encontrado.Email);
		}

		[Fact]
		public async Task ObterPorIdAsync_DeveRetornarNull_QuandoIdNaoExiste()
		{
			_repositoryMock.Setup(r => r.ObterPorIdAsync("id-inexistente"))
				.ReturnsAsync((Usuario?)null);

			var encontrado = await _service.ObterPorIdAsync("id-inexistente");

			Assert.Null(encontrado);
		}

		[Fact]
		public async Task ExcluirAsync_DeveExcluirUsuario_QuandoIdExiste()
		{
			var usuario = new Usuario
			{
				Id = "4",
				Nome = "D",
				Email = "d@d.com",
				Senha = "Senha4!",
				Role = "User"
			};

			_repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id))
				.ReturnsAsync(usuario);

			_repositoryMock.Setup(r => r.RemoverAsync(usuario))
				.Returns(Task.CompletedTask);

			var result = await _service.ExcluirAsync(usuario.Id);

			Assert.True(result.Success);
			Assert.Equal("Usuário excluído com sucesso.", result.Message);

			_repositoryMock.Verify(r => r.RemoverAsync(usuario), Times.Once);
		}

		[Fact]
		public async Task ExcluirAsync_DeveFalhar_QuandoIdNaoExiste()
		{
			_repositoryMock.Setup(r => r.ObterPorIdAsync("id-inexistente"))
				.ReturnsAsync((Usuario?)null);

			var result = await _service.ExcluirAsync("id-inexistente");

			Assert.False(result.Success);
			Assert.Equal("Usuário não encontrado.", result.Message);

			_repositoryMock.Verify(r => r.RemoverAsync(It.IsAny<Usuario>()), Times.Never);
		}
	}
}
