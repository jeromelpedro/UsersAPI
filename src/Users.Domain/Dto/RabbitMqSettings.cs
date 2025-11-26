namespace Users.Domain.Dto
{
	public class RabbitMqSettings
	{
		public string HostName { get; set; }
		public int Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string ExchangeName { get; set; }
	}
}
