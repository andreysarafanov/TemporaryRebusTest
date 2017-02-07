using System;
using Core;
using Publisher.Models;
using RabbitMQ.Client;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Logging;

namespace Publisher
{
	class Program
	{
		private const LogLevel MinimumLogLevel = LogLevel.Warn;
		private static Random _random = new Random();

		static void Main(string[] args)
		{
			using (var positivePublisher = new BuiltinHandlerActivator())
			using (var negativePublisher = new BuiltinHandlerActivator())
			{
				var posBus = ConfigurePositiveActivator(positivePublisher);
				var negBus = ConfigureNegativeActivator(negativePublisher);

				Console.WriteLine(@"1 — Send random positive number
2 — Send random negative number
q — Exit");

				bool keepRunning = true;
				while (keepRunning)
				{
					var key = char.ToLower(Console.ReadKey(true).KeyChar);

					switch (key)
					{
						case '1':
							PublishPositive(posBus);
							break;
						case '2':
							PublishNegative(negBus);
							break;
						case 'q':
							Console.WriteLine("Quitting");
							keepRunning = false;
							break;
					}
				}
			}
		}

		private static void PublishNegative(IBus publisher)
		{
			var num = -1 * _random.Next(100);
			Console.WriteLine($"Sending negative number {num}");
			publisher.Advanced.Topics.Publish(Defaults.NegativeTopic, new NegativeSendModel(num));
		}

		private static void PublishPositive(IBus publisher)
		{
			var num = _random.Next(100);
			Console.WriteLine($"Sending positive number {num}");
			publisher.Advanced.Topics.Publish(Defaults.PositiveTopic, new PositiveSendModel(num));
		}

		private static IBus ConfigureNegativeActivator(IHandlerActivator publisher)
		{
			return Configure.With(publisher)
				.Logging(l => l.ColoredConsole(MinimumLogLevel))
				.Transport(t => t.UseRabbitMqAsOneWayClient(Defaults.ConnectionString))
				.Serialization(s => s.Register(_ => new TypedJsonSerializer<NegativeSendModel>()))
				.Start();
		}

		private static IBus ConfigurePositiveActivator(IHandlerActivator publisher)
		{
			return Configure.With(publisher)
				.Logging(l => l.ColoredConsole(MinimumLogLevel))
				.Transport(t => t.UseRabbitMqAsOneWayClient(Defaults.ConnectionString))
				.Serialization(s => s.Register(_ => new TypedJsonSerializer<PositiveSendModel>()))
				.Start();
		}
	}
}
