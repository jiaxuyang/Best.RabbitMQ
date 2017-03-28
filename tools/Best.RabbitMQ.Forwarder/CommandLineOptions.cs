using CommandLine;
using CommandLine.Text;

namespace Best.RabbitMQ.Forwarder
{
    public class CommandLineOptions
    {
        public CommandLineOptions()
        {
            Threads = 1;
        }

        [Option("source", Required = true, HelpText = @"source amqp url")]
        public string SourceAMQP { get; set; }

        [Option("dest", Required = true, HelpText = @"dest amqp url")]
        public string DestAMQP { get; set; }

        [Option("fromqueue", Required = true, HelpText = @"from queue")]
        public string FromQueue { get; set; }

        [Option("toqueue", Required = true, HelpText = @"to queue")]
        public string ToQueue { get; set; }

        [Option("thread", HelpText = @"Parallel thread")]
        public int Threads { get; set; }

        [Option('v', "verbose", HelpText = @"Prints all messages to standard output. Example: -v")]
        public bool Verbose { get; set; }
    }
}
