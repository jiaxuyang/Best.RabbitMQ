using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Best.RabbitMQ.Test;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Best.RabbitMQ.Managements.Tests
{
    [TestFixture]
    public class HAConnectionFactoryTest
    {
        [Test]
        public void ConnectionOneCorrect()
        {
            var haConnFactory =
                new HAConnectionFactory(RabbitMQServers.MasterAMQPUrl);
            var conn = haConnFactory.CreateConnection();
            Assert.That(conn.Channel.IsOpen,Is.True);
            Assert.That(conn.HostName,Is.EqualTo(RabbitMQServers.MasterServerHostName));
        }

        [Test]
        public void ConnectionTwoCorrect()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl);
            var conn = haConnFactory.CreateConnection();
            Assert.That(conn.Channel.IsOpen, Is.True);
            Assert.That(conn.HostName, Is.EqualTo(RabbitMQServers.MasterServerHostName).Or.EqualTo(RabbitMQServers.SlaveServerHostName));
        }

        [Test]
        public void ConnectionTwoFirstInCorrect()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterErrorPortAndSlaveUrl);
            var conn = haConnFactory.CreateConnection();
            Assert.That(conn.Channel.IsOpen, Is.True);
            Assert.That(conn.HostName, Is.EqualTo(RabbitMQServers.SlaveServerHostName));
        }

        [Test]
        public void ConnectionTwoFirstDown()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterErrorHostAndSlaveUrl);
            var conn = haConnFactory.CreateConnection();
            Assert.That(conn.Channel.IsOpen, Is.True);
            Assert.That(conn.HostName, Is.EqualTo(RabbitMQServers.SlaveServerHostName));
        }

        [Test]
        public void NullAmqpUrlTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var haConnFactory = new HAConnectionFactory(null);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var haConnFactory = new HAConnectionFactory("");
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var haConnFactory = new HAConnectionFactory("  ");
            });
        }

        [Test]
        public void ErrorChannelMaxTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAMQPUrl, 0, 0, 0, 0);
            });
        }

        [Test]
        public void ErrorConnectionTimeoutTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAMQPUrl, 0, 0, 0, 0);
            });
        }

        [Test]
        public void ErrorFrameMaxTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAMQPUrl, 0, 0, 0, 0);
            });
        }

        [Test]
        public void ErrorHeartBeatTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAMQPUrl, 0, 0, 0, 0);
            });
        }

        [Test]
        [Repeat(10)]
        public void ConnectionTwoCorrect_LoadBalance()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl, HAConnectionFactory.HAMode.LoadBalance);

            var connectedEndpointList = new ConcurrentBag<string>();
            Parallel.For(0, 1000, i =>
            {
                var conn = haConnFactory.CreateConnection();
                Assert.IsTrue(conn.Channel.IsOpen);
                if (string.IsNullOrEmpty(conn.HostName))
                    Debugger.Launch();
                connectedEndpointList.Add(conn.HostName);
            });

            Assert.IsTrue(connectedEndpointList.Any(t => t.Contains(RabbitMQServers.MasterServerHostName)));
            Assert.Greater(connectedEndpointList.Count(t => t.Contains(RabbitMQServers.MasterServerHostName)), 400);
            Assert.IsTrue(connectedEndpointList.Any(t => t.Contains(RabbitMQServers.SlaveServerHostName)));
            Assert.Greater(connectedEndpointList.Count(t => t.Contains(RabbitMQServers.SlaveServerHostName)), 400);

            Thread.Sleep(10);
        }
    }
}
