using CommandLine;
using CommandLine.Text;

namespace Best.RabbitMQ.Declare
{
    public class CommandLineOptions
    {
        [Option('u', "urls", Required = true, HelpText = @"RabbitMQ amqp urls.")]
        public string RMQUrls { get; set; }

        [Option('e', "exchange", Required = true, HelpText = @"RabbitMQ exchange.")]
        public string Exchange { get; set; }

        [Option('t', "exchangeType", Required = true, HelpText = @"RabbitMQ exchange type.")]
        public string ExchangeType { get; set; }

        [Option('r', "routingKey", Required = true, HelpText = @"RabbitMQ routingKey.")]
        public string RoutingKey { get; set; }

        [Option('q', "queue", Required = true, HelpText = @"RabbitMQ queue.")]
        public string Queue { get; set; }

        [Option('v', "verbose", HelpText = @"Prints all messages to standard output. Example: -v")]
        public bool Verbose { get; set; }
    }
}
