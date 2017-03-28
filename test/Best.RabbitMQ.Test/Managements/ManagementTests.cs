using Best.RabbitMQ.Test;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Best.RabbitMQ.Managements.Tests
{
    [TestFixture]
    public class ManagementTests
    {
        [Test]
        public void ManagementTest()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl);
            var management = new Management(haConnFactory);
            Assert.That(management, Is.Not.Null);
        }

        [Test]
        public void ExchangeDeclareTest()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl);
            var management = new Management(haConnFactory);
            management.ExchangeDeclare("unittest.topic", "topic");
        }

        [Test]
        public void QueueDeclareTest()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl);
            var management = new Management(haConnFactory);
            management.QueueDeclare("unittest.queue");
        }

        [Test]
        public void QueueBindTest()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl);
            var management = new Management(haConnFactory);
            management.ExchangeDeclare("unittest.topic2", "topic");
            management.QueueDeclare("unittest.queuebind");
            management.QueueBind("unittest.queuebind", "unittest.topic2", "unittest.topic2.#");
        }

        [Test]
        public void CloseTest()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl);
            var management = new Management(haConnFactory);
            management.Close();
        }

        [Test]
        public void DisposeTest()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl);
            var management = new Management(haConnFactory);
            management.Dispose();
        }
    }
}