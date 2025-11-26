namespace Users.Domain.Interfaces.MessageBus
{
	public interface IRabbitMqPublisher
	{
		Task PublishAsync<T>(string topic, T message);
	}	
}
