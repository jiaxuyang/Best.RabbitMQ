using Best.RabbitMQ.Test;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable once CheckNamespace
namespace Best.RabbitMQ.Managements.Tests
{
    [TestFixture]
    public class RabbitMQConnectionTests
    {
        private RabbitMQConnectionPool _rabbitMQConnection;
        private int _channelNumnber;

        [OneTimeSetUp]
        public void RabbitMQConnectionPoolTest()
        {
            var amqpUrl = RabbitMQServers.MasterAMQPUrl;
            var connFactory = new ConnectionFactory
            {
                Uri = amqpUrl,
                RequestedChannelMax = 10000,
                RequestedConnectionTimeout = 3000,
                RequestedFrameMax = 1024000,
                RequestedHeartbeat = 60
            };
            _rabbitMQConnection = new RabbitMQConnectionPool(
                connFactory,
                amqpUrl,
                100);
            Assert.That(_rabbitMQConnection, Is.Not.Null);

            using (var conn = _rabbitMQConnection.Dequeue())
            {
                Assert.That(conn, Is.Not.Null);
                Assert.That(conn.Channel,Is.Not.Null);
                Assert.That(conn.ChannelNumber,Is.Not.Null);
                Assert.That(conn.AMQPUrl, Is.Not.Null);
                Assert.That(conn.HostName, Is.Not.Null);
                Assert.That(conn.UserName, Is.Not.Null);
                Assert.That(conn.VHost, Is.Not.Null);
                _channelNumnber = conn.ChannelNumber;
            }
        }

        [Test]
        public void DisposeTest()
        {
            using (var conn = _rabbitMQConnection.Dequeue())
            {
                Assert.That(conn, Is.Not.Null);
                Assert.That(conn.ChannelNumber, Is.EqualTo(_channelNumnber));
                conn.Dispose();
            }
        }

        [Test]
        public void CloseTest()
        {
            using (var conn = _rabbitMQConnection.Dequeue())
            {
                Assert.That(conn, Is.Not.Null);
                Assert.That(conn.ChannelNumber, Is.EqualTo(_channelNumnber));
                conn.Close();
            }
        }

        [Test]
        public void ForceClose()
        {
            var conn1 = _rabbitMQConnection.Dequeue();
            var channelNumer1 = conn1.ChannelNumber;
            conn1.ForceClose();
            var conn2 = _rabbitMQConnection.Dequeue();
            var channelNumer2 = conn2.ChannelNumber;
            conn2.ForceClose();

            Assert.That(channelNumer2, Is.EqualTo(channelNumer1));
        }
    }
}