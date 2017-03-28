using System;
using Best.RabbitMQ.Managements;
using Best.RabbitMQ.Test;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Best.RabbitMQ.Tests
{
    [TestFixture]
    public class MessageSenderTest
    {
        [Test]
        public void SendDefaultTest()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl);

            var sender = new MessageSender(haConnFactory);
            sender.Publish(string.Empty, "hatest", "sender test message:" + Guid.NewGuid().ToString());
            sender.Dispose();
        }

        [Test]
        public void SendTwoCorrectTest()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl);

            var sender = new MessageSender(haConnFactory);
            sender.Publish("amq.direct", "hatest", "sender test message:" + Guid.NewGuid().ToString());
            sender.Dispose();
        }


        [Test]
        public void SendTwoCorrect_Manual_Block_Test()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl,HAConnectionFactory.HAMode.MasterSlave);

            using (var sender = new MessageSender(haConnFactory))
            {
                for (int i = 0; i < 100000; i++)
                {
                    sender.Publish("amq.direct", "hatest", "sender test message:" + Guid.NewGuid().ToString());
                }
            }
        }

        [Test]
        public void SendTwoFirstDownTest()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterErrorHostAndSlaveUrl);

            var sender = new MessageSender(haConnFactory);
            sender.Publish("amq.direct", "hatest", "test message");
            sender.Dispose();
        }

        [Test]
        public void SendTwoFirstReachLimitTest()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl);

            var sender = new MessageSender(haConnFactory);
            for (int i = 0; i < 2000; i++)
            {
                sender.Publish("amq.direct", "hatest", i.ToString());
            }
            sender.Dispose();
        }

        [Ignore("temp test only")]
        [Test]
        public void OneBlockTest()
        {
            for (int i = 0; i < 10000; i++)
            {
                var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl);

                var sender = new MessageSender(haConnFactory);
                sender.Publish(string.Empty, "test", "test message");
                sender.Dispose();
            }
        }

        [Ignore("temp test only")]
        [Test]
        public void OneBlock_Only_Test()
        {
            var bytes = new byte[102400];
            new Random().NextBytes(bytes);
            for (int i = 0; i < 10000; i++)
            {
                var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAMQPUrl);

                var sender = new MessageSender(haConnFactory);
                sender.Publish(string.Empty, "test", bytes);
                sender.Dispose();
            }
        }
    }
}
