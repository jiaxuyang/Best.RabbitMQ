using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Best.RabbitMQ.Managements;
using CommandLine;

namespace Best.RabbitMQ.Forwarder
{
    class Program
    {
        private static HAConnectionFactory _desConnectionFactory;
        private static string _destQueue;

        static void Main(string[] args)
        {
            CommandLineOptions options = null;
            var parseResult = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithNotParsed(result => { Environment.Exit(1); })
                .WithParsed(result =>
                {
                    options = result;
                });

            if (parseResult.Tag == ParserResultType.Parsed)
            {
                var sourceConnectionFactory = new HAConnectionFactory(options.SourceAMQP);
                _desConnectionFactory = new HAConnectionFactory(options.DestAMQP);
                _destQueue = options.ToQueue;

                var receiverList = new List<MessageReceiver>();
                for (int i = 0; i < options.Threads; i++)
                {
                    var receiver = new MessageReceiver(sourceConnectionFactory, options.FromQueue);
                    receiver.OnMessageReceived += Receiver_OnMessageReceived;
                    receiver.StartConsuming();
                    receiverList.Add(receiver);
                }

                Console.WriteLine("hint q exit");

                while (Console.ReadKey().Key != ConsoleKey.Q)
                {
                    Console.WriteLine("hint Q exit");
                }
                Environment.Exit(0);
            }
            else
            {
                Environment.Exit(1);
            }
        }

        private static void Receiver_OnMessageReceived(object sender, MessageReceiveEventArgs e)
        {
            Task.Run(() =>
            {
                using (var msgSender = new MessageSender(_desConnectionFactory))
                {
                    msgSender.Publish(string.Empty, _destQueue, e.RawData, false);
                }
            });
        }
    }
}
