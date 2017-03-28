using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Best.RabbitMQ.Test
{
    public static class RabbitMQServers
    {
        public static string MasterServerHostName => "127.0.0.1";
        public static int MasterServerPort => 5672;
        public static string MasterServerUserName => "unittest";
        public static string MasterServerPassword => "unittest";
        public static string MasterServerVHost => "unittest";

        public static string MasterAMQPUrl => $"amqp://{MasterServerUserName}:{MasterServerPassword}@{MasterServerHostName}:{MasterServerPort}/{MasterServerVHost}";

        public static string MasterServerUrlErrorPort= $"amqp://{MasterServerUserName}:{MasterServerPassword}@{MasterServerHostName}:{5670}/{MasterServerVHost}";

        public static string MasterServerUrlErrorHost = $"amqp://{MasterServerUserName}:{MasterServerPassword}@err-{MasterServerHostName}:{MasterServerPort}/{MasterServerVHost}";

        public static string SlaveServerHostName => "127.0.0.1";
        public static int SlaveServerPort => 5672;
        public static string SlaveServerUserName => "unittest";
        public static string SlaveServerPassword => "unittest";
        public static string SlaveServerVHost => "unittest";

        public static string SlaveAMQPUrl => $"amqp://{SlaveServerUserName}:{SlaveServerPassword}@{SlaveServerHostName}:{SlaveServerPort}/{SlaveServerVHost}";

        public static string SlaveServerUrlErrorPort = "";

        public static string MasterAndSlaveAMQPUrl => $"{MasterAMQPUrl};{SlaveAMQPUrl}";

        public static string MasterErrorPortAndSlaveUrl => $"{MasterAMQPUrl};{SlaveAMQPUrl}";

        public static string MasterErrorHostAndSlaveUrl => $"{MasterServerUrlErrorHost};{SlaveAMQPUrl}";
    }
}
