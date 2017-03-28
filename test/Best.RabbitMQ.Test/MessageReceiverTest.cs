using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Best.RabbitMQ.Managements;
using Best.RabbitMQ.Test;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Best.RabbitMQ.Tests
{
    [TestFixture]
    public class MessageReceiverTest
    {
        [Test]
        public void ReceiveTest()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl);

            var haConnFactoryMaster = new HAConnectionFactory(RabbitMQServers.MasterAMQPUrl);
            var haConnFactorySlave = new HAConnectionFactory(RabbitMQServers.SlaveAMQPUrl);

            var receiverList = new List<MessageReceiver>();
            for (int i = 0; i < 40; i++)
            {
                var receiver = new MessageReceiver(haConnFactory, "hatest");
                receiver.OnMessageReceived += receiver_OnMessageReceived;
                receiver.StartConsuming();
                receiverList.Add(receiver);
            }

            var parallelResult = Parallel.For(0, 100000, i =>
            {
                using (var senderMaster = new MessageSender(haConnFactoryMaster))
                {
                    senderMaster.Publish("amq.direct", "hatest", "receive test msg:" + Guid.NewGuid());
                }
                using (var senderSlave = new MessageSender(haConnFactorySlave))
                {
                    senderSlave.Publish("amq.direct", "hatest", "receive test msg:" + Guid.NewGuid());
                }
                Thread.Sleep(1);
            });
            Console.WriteLine(parallelResult.IsCompleted);

            Thread.Sleep(180000);
            Console.WriteLine("Consume messages:{0}", _receiveCount);
            Assert.GreaterOrEqual(_receiveCount, 200000);

            foreach (var receiver in receiverList)
            {
                receiver.Dispose();
            }
        }


        [Test]
        public void Receive_SendAll_First_Test()
        {
            var haConnFactory = new HAConnectionFactory(RabbitMQServers.MasterAndSlaveAMQPUrl);

            var haConnFactoryMaster = new HAConnectionFactory(RabbitMQServers.MasterAMQPUrl);
            var haConnFactorySlave = new HAConnectionFactory(RabbitMQServers.SlaveAMQPUrl);

            var parallelResult = Parallel.For(0, 100000, i =>
            {
                using (var senderMaster = new MessageSender(haConnFactoryMaster))
                {
                    senderMaster.Publish("amq.direct", "hatest", "receive test msg:" + Guid.NewGuid());
                }
                using (var senderSlave = new MessageSender(haConnFactorySlave))
                {
                    senderSlave.Publish("amq.direct", "hatest", "receive test msg:" + Guid.NewGuid());
                }
            });
            Console.WriteLine(parallelResult.IsCompleted);


            var receiverList = new List<MessageReceiver>();
            for (int i = 0; i < 40; i++)
            {
                var receiver = new MessageReceiver(haConnFactory, "hatest");
                receiver.OnMessageReceived += receiver_OnMessageReceived;
                receiver.StartConsuming();
                receiverList.Add(receiver);
            }
          

            Thread.Sleep(180000);
            Console.WriteLine("Consume messages:{0}", _receiveCount);
            Assert.GreaterOrEqual(_receiveCount, 200000);

            foreach (var receiver in receiverList)
            {
                receiver.Dispose();
            }
        }

        private int _receiveCount;

        void receiver_OnMessageReceived(object sender, MessageReceiveEventArgs e)
        {
            //Console.WriteLine("Tag:{0} Message:{1}", e.ConsumerTag, e.Message);
            //Thread.Sleep(100);
            Interlocked.Increment(ref _receiveCount);
        }
    }
}
