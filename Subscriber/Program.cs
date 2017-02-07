using System;
using Core;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Logging;
using Subscriber.Models;

namespace Subscriber
{
	class Program
	{
		private const LogLevel MinimumLogLevel = LogLevel.Warn;
		static void Main(string[] args)
		{
			using (var subscriberPos = new BuiltinHandlerActivator())
			using (var subscriberNeg = new BuiltinHandlerActivator())
			{
				var posBus = ConfigurePositiveSubscriber(subscriberPos);
				var negBus = ConfigureNegativeSubscriber(subscriberNeg);

				posBus.Advanced.Topics.Subscribe(Defaults.PositiveTopic).Wait();
				negBus.Advanced.Topics.Subscribe(Defaults.NegativeTopic).Wait();

				Console.WriteLine("Press ENTER to quit");
				Console.ReadLine();
			}
		}

		private static IBus ConfigurePositiveSubscriber(BuiltinHandlerActivator subscriber)
		{
			subscriber.Handle<PositiveReceiveModel>(
				async model => { Console.WriteLine($"Received positive number {model.Number}"); });

			return Configure.With(subscriber)
				.Logging(l => l.ColoredConsole(MinimumLogLevel))
				.Transport(t => t.UseRabbitMq(Defaults.ConnectionString, Defaults.PositiveQueueName))
				.Serialization(s => s.Register(_ => new TypedJsonSerializer<PositiveReceiveModel>()))
				.Start();
		}

		private static IBus ConfigureNegativeSubscriber(BuiltinHandlerActivator subscriber)
		{
			subscriber.Handle<NegativeReceiveModel>(
				async model => { Console.WriteLine($"Received negative number {model.Number}"); });

			return Configure.With(subscriber)
				.Logging(l => l.ColoredConsole(MinimumLogLevel))
				.Transport(t => t.UseRabbitMq(Defaults.ConnectionString, Defaults.NegativeQueueName))
				.Serialization(s => s.Register(_ => new TypedJsonSerializer<NegativeReceiveModel>()))
				.Start();
		}
	}
}
