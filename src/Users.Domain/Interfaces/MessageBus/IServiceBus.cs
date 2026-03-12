namespace Users.Domain.Interfaces.MessageBus
{
	public interface IServiceBus
	{
		Task PublishAsync(string topic, object message);
	}
}
