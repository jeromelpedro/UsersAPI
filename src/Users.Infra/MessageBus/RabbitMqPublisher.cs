using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Users.Domain.Dto;
using Users.Domain.Interfaces.MessageBus;

namespace Users.Infra.MessageBus
{
	public class RabbitMqPublisher(IOptions<RabbitMqSettings> options) : IRabbitMqPublisher
	{
		private readonly RabbitMqSettings _settings = options.Value;

		public async Task PublishAsync<T>(string topic, T message)
		{
			var factory = new ConnectionFactory()
			{
				HostName = _settings.HostName,
				Port = _settings.Port,
				UserName = _settings.Username,
				Password = _settings.Password
			};

			using (var connection = await factory.CreateConnectionAsync())
			using (var channel = await connection.CreateChannelAsync())
			{
				await channel.ExchangeDeclareAsync(
					exchange: _settings.ExchangeName,
					type: ExchangeType.Topic,
					durable: true,
					autoDelete: false
				);

				// Garante que a fila existe
				await channel.QueueDeclareAsync(
					queue: topic,
					durable: true,
					exclusive: false,
					autoDelete: false,
					arguments: null);

				// Garante que o binding existe:
				// fila queueName recebe mensagens com routingKey = topic
				await channel.QueueBindAsync(
					queue: topic,
					exchange: _settings.ExchangeName,
					routingKey: topic);

				var json = JsonSerializer.Serialize(message);
				var body = Encoding.UTF8.GetBytes(json);

				var properties = new BasicProperties
				{
					Persistent = true,
					ContentType = "application/json"
				};

				await channel.BasicPublishAsync(
					exchange: _settings.ExchangeName,
					routingKey: topic,
					mandatory: false,
					basicProperties: properties,
					body: body
				);
			}
		}
	}
}