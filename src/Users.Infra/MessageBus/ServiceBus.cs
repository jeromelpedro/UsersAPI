using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Users.Domain.Interfaces.MessageBus;

namespace Users.Infra.MessageBus
{
	public class ServiceBus(ServiceBusClient _serviceBusClient, ILogger<ServiceBus> _logger) : IServiceBus
	{
		public async Task PublishAsync(string topic, object message)
		{
			var sender = _serviceBusClient.CreateSender(topic);

			try
			{
				var messageBody = JsonSerializer.Serialize(message);
				var sMessage = new ServiceBusMessage(messageBody)
				{
					ContentType = "application/json"
				};
				
				await sender.SendMessageAsync(sMessage);

				_logger.LogInformation($"Mensagem enviada para fila {topic}.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Erro ao enviar mensagem para Service Bus fila {topic}.");
				throw;
			}
			finally
			{
				await sender.DisposeAsync();
			}
		}
	}
}
