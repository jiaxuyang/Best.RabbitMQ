using System;
using System.Diagnostics;
using Best.RabbitMQ.Exceptions;
using Best.RabbitMQ.Test;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable once CheckNamespace
namespace Best.RabbitMQ.Managements.Tests
{
    [TestFixture]
    public class RabbitMQConnectionPoolTests
    {
        private RabbitMQConnectionPool _rabbitMQConnection;
        private int _channelNumnber;
        private IConnection _connection1;

        [OneTimeSetUp]
        public void RabbitMQConnectionPoolTest()
        {
            var amqpUrl = RabbitMQServers.MasterAMQPUrl;
            var connFactory = new ConnectionFactory
            {
                Uri = new Uri(amqpUrl),
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

            _connection1 = _rabbitMQConnection.Dequeue();
            using (var conn = _rabbitMQConnection.Dequeue())
            {
                Assert.That(conn, Is.Not.Null);
                _channelNumnber = conn.ChannelNumber;
            }
        }

        [Test]
        public void DequeueTest()
        {
            using (var conn = _rabbitMQConnection.Dequeue())
            {
                Assert.That(conn, Is.Not.Null);
                Assert.That(conn.ChannelNumber, Is.EqualTo(_channelNumnber));
            }
        }

        [Test]
        public void EnqueueTest()
        {
            using (var conn = _rabbitMQConnection.Dequeue())
            {
                Assert.That(conn, Is.Not.Null);
                Assert.That(conn.ChannelNumber, Is.EqualTo(_channelNumnber));
                _rabbitMQConnection.Enqueue(conn);
                var conn2 = _rabbitMQConnection.Dequeue();
                Assert.That(conn, Is.Not.Null);
                Assert.That(conn.ChannelNumber, Is.EqualTo(_channelNumnber));
                conn2.Dispose();
            }
        }

        [OneTimeTearDown]
        public void DisposeTest()
        {
            _rabbitMQConnection.Dispose();
        }

        [Test]
        public void ReConnectTest()
        {
            _rabbitMQConnection.ReConnect();
            var conn = _rabbitMQConnection.Dequeue();
            Assert.That(conn, Is.Not.Null);
            Assert.That(conn.ChannelNumber, Is.Not.EqualTo(_channelNumnber));
        }

        [Test]
        public void MaxPoolSizeTest()
        {
            var amqpUrl = RabbitMQServers.MasterAMQPUrl;
            var connFactory = new ConnectionFactory
            {
                Uri = new Uri(amqpUrl),
                RequestedChannelMax = 10000,
                RequestedConnectionTimeout = 3000,
                RequestedFrameMax = 1024000,
                RequestedHeartbeat = 60
            };
            var rabbitMQConnection = new RabbitMQConnectionPool(
                connFactory,
                amqpUrl,
                10);
            for (int i = 0; i < 10; i++)
            {
                rabbitMQConnection.Dequeue();
            }
            Assert.Throws<ConnectionPoolException>(() => rabbitMQConnection.Dequeue());
        }

        [TestCase(10000)]
        public void PerformanceTest(int times)
        {
            var swForceClose = Stopwatch.StartNew();
            for (int i = 0; i < times; i++)
            {
                using (var conn = _rabbitMQConnection.Dequeue())
                {
                    conn.ForceClose();
                }
            }
            swForceClose.Stop();
            Console.WriteLine("ForceClose TotalTime:{0}\t\tOneTime:{1}", swForceClose.ElapsedMilliseconds, swForceClose.ElapsedMilliseconds / times);

            var swDispose = Stopwatch.StartNew();
            for (int i = 0; i < times; i++)
            {
                using (var conn = _rabbitMQConnection.Dequeue())
                {

                }
            }
            swDispose.Stop();
            Console.WriteLine("Dispose TotalTime:{0}\t\tOneTime:{1}", swDispose.ElapsedMilliseconds, swDispose.ElapsedMilliseconds / times);
        }
    }
}