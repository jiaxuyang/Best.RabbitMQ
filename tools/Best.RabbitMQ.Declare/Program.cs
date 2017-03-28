using Best.RabbitMQ.Managements;
using System;
using CommandLine;

namespace Best.RabbitMQ.Declare
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLineOptions options = null;
            var parseResult = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithNotParsed(result=>{Environment.Exit(1);})
                .WithParsed(result =>
                {
                    options = result;
                });

            if(parseResult.Tag == ParserResultType.Parsed)
            {
                var haConnectionFactory = new HAConnectionFactory(options.RMQUrls);
                var management = new Management(haConnectionFactory);
                management.ExchangeDeclare(options.Exchange, options.ExchangeType);
                management.QueueDeclare(options.Queue);
                management.QueueBind(options.Queue, options.Exchange, options.RoutingKey);
                management.Close();
            }
            else
            {
                Environment.Exit(1);
            }
        }
    }
}
