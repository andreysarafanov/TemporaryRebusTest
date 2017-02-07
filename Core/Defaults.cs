using RabbitMQ.Client;

namespace Core
{
	public static class Defaults
	{
		static Defaults()
		{
			var model = new ConnectionFactory { Uri = ConnectionString }.CreateConnection().CreateModel();

			model.ExchangeDelete(DefaultTopicExchangeName);
			model.ExchangeDelete(NumbersExchangeName);
			model.QueueDelete(NegativeQueueName);
			model.QueueDelete(PositiveQueueName);

			model.ExchangeDeclare(DefaultTopicExchangeName, ExchangeType.Topic, true);
			model.ExchangeDeclare(NumbersExchangeName, ExchangeType.Topic, true);
			model.QueueDeclare(NegativeQueueName);//, true, true, false);
			model.QueueDeclare(PositiveQueueName);//, true, true, false);

			model.ExchangeBind(NumbersExchangeName, DefaultTopicExchangeName, NumbersTopic);
			model.QueueBind(PositiveQueueName, NumbersExchangeName, PositiveTopic);
			model.QueueBind(NegativeQueueName, NumbersExchangeName, NegativeTopic);
		}

		public const string ConnectionString = "amqp://localhost";
		public const string DefaultTopicExchangeName = "TopicExchange";
		public const string NumbersExchangeName = "NumbersExchange";
		public const string DefaultDirectExchangeName = "DirectExchange";
		public const string NumbersTopic = "numbers.#";
		public const string PositiveTopic = "numbers.pos";
		public const string NegativeTopic = "numbers.neg";
		public const string PositiveQueueName = "PositiveQueue";
		public const string NegativeQueueName = "NegativeQueue";
	}
}
