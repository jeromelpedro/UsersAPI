using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Users.Domain.Utils
{
	public static class Security
	{
		private static IConfiguration _configuration;

		public static void Configure(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		private static byte[] GetKeyBytes()
		{
			var base64 = _configuration.GetConfigValue("Secrets:Password");

			if (string.IsNullOrWhiteSpace(base64))
				throw new InvalidOperationException(
					"Variável de ambiente 'Secrets__Password' não está definida."
				);

			byte[] key;

			try
			{
				key = Convert.FromBase64String(base64);
			}
			catch (FormatException ex)
			{
				throw new InvalidOperationException(
					"Secrets__Password não é uma string Base64 válida.",
					ex
				);
			}

			if (key.Length != 32) // 32 bytes = 256 bits
				throw new InvalidOperationException(
					$"Secrets__Password deve ser uma chave Base64 de 32 bytes. Tamanho atual: {key.Length} bytes."
				);

			return key;
		}

		// 16 caracteres ASCII = 16 bytes = 128 bits. NÃO pode ser null.
		private static readonly byte[] FixedIv = Encoding.UTF8.GetBytes("FixedInitVector1");
		//            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		// Certifique-se de que esta linha está exatamente assim no seu código.

		public static string Encrypt(this string plainText)
		{
			if (string.IsNullOrEmpty(plainText))
				return plainText;

			using var aes = Aes.Create();
			aes.Key = GetKeyBytes();
			aes.IV = FixedIv; // <-- aqui não será null se a linha acima estiver certa

			using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
			using var ms = new MemoryStream();
			using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
			using (var sw = new StreamWriter(cs))
			{
				sw.Write(plainText);
			}

			var cipherBytes = ms.ToArray();
			return Convert.ToBase64String(cipherBytes);
		}

		public static string Decrypt(this string cipherText)
		{
			if (string.IsNullOrEmpty(cipherText))
				return cipherText;

			var cipherBytes = Convert.FromBase64String(cipherText);

			using var aes = Aes.Create();
			aes.Key = GetKeyBytes();
			aes.IV = FixedIv;

			using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
			using var ms = new MemoryStream(cipherBytes);
			using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
			using var sr = new StreamReader(cs);

			return sr.ReadToEnd();
		}
	}
}

