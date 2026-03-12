using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
		private Mock<IServiceBus> _serviceBusMock;
				private IConfiguration _configuration;
				private Mock<ILogger<UsuarioService>> _loggerMock;
		private UsuarioService _service;

		public UsuarioServiceTests()
		{
			// Define uma chave padrão para os testes (32 bytes em Base64)
			Environment.SetEnvironmentVariable("Secrets__Password", Convert.ToBase64String(new byte[32]));
			_repositoryMock = new Mock<IUsuarioRepository>();
			_serviceBusMock = new Mock<IServiceBus>();
			// Evita side-effects de publicação durante os testes
			_serviceBusMock.Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);

			// Mock configuration with ServiceBus:UserCreatedEvent
			var inMemorySettings = new Dictionary<string, string> {
				{"ServiceBus:UserCreatedEvent", "UserCreatedEvent"}
			};
			_configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

			_loggerMock = new Mock<ILogger<UsuarioService>>();

			_service = new UsuarioService(_repositoryMock.Object, _serviceBusMock.Object, _configuration, _loggerMock.Object);
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
			var created = Assert.IsType<Users.Domain.Entity.Usuario>(result.Result);
			Assert.Equal(usuarioDto.Email, created.Email);
			Assert.Equal("User", created.Role);
			Assert.Equal("Usuário criado com sucesso.", result.Message);

            _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>()), Times.Once);
			_serviceBusMock.Verify(m => m.PublishAsync("UserCreatedEvent", It.IsAny<UserCreatedEventDto>()), Times.Once);
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
			var created = Assert.IsType<Users.Domain.Entity.Usuario>(result.Result);
			Assert.Equal(usuarioDto.Email, created.Email);
			Assert.Equal("Admin", created.Role);
			Assert.Equal("Usuário criado com sucesso.", result.Message);

            _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>()), Times.Once);
			_serviceBusMock.Verify(m => m.PublishAsync("UserCreatedEvent", It.IsAny<UserCreatedEventDto>()), Times.Once);
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
            var id = Guid.NewGuid();

			var input = new AlterarSenhaInputDto
			{
				IdUsuario = id.ToString(),
				SenhaAntiga = "SenhaAntiga1!",
				SenhaNova = "NovaSenha1!"
			};

			var usuario = new Usuario
			{
				Id = id,
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
            var id = Guid.NewGuid();

			var input = new AlterarSenhaInputDto
			{
				IdUsuario = id.ToString(),
				SenhaAntiga = "NovaSenha1!",
				SenhaNova = "OutraNova1!"
			};

			var usuario = new Usuario
			{
				Id = id,
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
				new Usuario { Id = Guid.NewGuid(), Nome = "A", Email = "a@a.com", Senha = "Senha1!", Role = "User" },
				new Usuario { Id = Guid.NewGuid(), Nome = "B", Email = "b@b.com", Senha = "Senha2!", Role = "User" }
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
				Id = Guid.NewGuid(),
				Nome = "C",
				Email = "c@c.com",
				Senha = "Senha3!",
				Role = "User"
			};

			var id = usuario.Id.ToString();

			_repositoryMock.Setup(r => r.ObterPorIdAsync(id))
				.ReturnsAsync(usuario);

			var encontrado = await _service.ObterPorIdAsync(id);

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
				Id = Guid.NewGuid(),
				Nome = "D",
				Email = "d@d.com",
				Senha = "Senha4!",
				Role = "User"
			};

			var id = usuario.Id.ToString();

			_repositoryMock.Setup(r => r.ObterPorIdAsync(id))
				.ReturnsAsync(usuario);

			_repositoryMock.Setup(r => r.RemoverAsync(usuario))
				.Returns(Task.CompletedTask);

			var result = await _service.ExcluirAsync(id);

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
