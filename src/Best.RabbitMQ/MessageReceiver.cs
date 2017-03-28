using System;
using System.Collections.Generic;
using Best.RabbitMQ.Managements;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Best.RabbitMQ
{
    /// <summary>
    /// Receive messages from all available server in the HAConnectionFactory provided.
    /// </summary>
    public class MessageReceiver : IDisposable
    {
        private HAConnectionFactory HAConnectionFactory { get; }

        private volatile IList<RabbitMQConnectionPool> _mqConnectionPoolList;

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public string QueueName { get; }


        /// <summary>
        /// Default value: 10000ms
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public int RetryInterval { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionFactory"></param>
        /// <param name="queueName"></param>
        /// <param name="retryInterval"></param>
        public MessageReceiver(HAConnectionFactory connectionFactory, string queueName, int retryInterval = 120000)
        {
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException(nameof(queueName));

            HAConnectionFactory = connectionFactory;
            QueueName = queueName;
            RetryInterval = retryInterval;

            _mqConnectionPoolList = HAConnectionFactory.CreateAllConnectionPool();
        }

        private volatile bool _isConsuming;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<MessageReceiveEventArgs> OnMessageReceived;

        /// <summary>
        /// Start to consume queue from all server
        /// </summary>
        /// <return>
        /// true:   start consume success
        /// false:  already started
        /// </return>
        // ReSharper disable once UnusedMethodReturnValue.Global
        public bool StartConsuming()
        {
            if (OnMessageReceived == null)
                throw new Exception("OnMessageReceived Not Bind");

            if (_isConsuming)
                return false;

            _isConsuming = true;

            foreach (var connectionPool in _mqConnectionPoolList)
            {
                // auto recovery interval time
                connectionPool.RMQConnectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(RetryInterval);

                var connection = connectionPool.Dequeue();
                var consumer = new EventingBasicConsumer(connection.Channel);
                consumer.Received += Consumer_Received;
                consumer.Registered += Consumer_Registered;
                consumer.Shutdown += Consumer_Shutdown;
                consumer.Unregistered += Consumer_Unregistered;
                consumer.ConsumerCancelled += Consumer_ConsumerCancelled;
                // limit flow
#if DEBUG
                ushort prefetchCount = 10;
#else
                ushort prefetchCount = 1000;
#endif
                consumer.Model.BasicQos(0, prefetchCount, false);
                // start consume
                var consumerTag = consumer.Model.BasicConsume(
                   QueueName,
                   false,
                   consumer);
                EntLib.Log.Logger.LogInfo($"{connection.UserName}:{connection.HostName}/{connection.VHost} ChannelNumber:{consumer.Model.ChannelNumber} ConsumerTag:{consumerTag}");
                //var thread = new Thread(Consume)
                //{
                //    Name = "RMQ_Recv_" + connectionPool.AMQPUrl,
                //    IsBackground = false
                //};
                //thread.Start(connectionPool);
                //Best.EntLib.Log.Logger.LogInfo($"Start consume on queue:{0}");
            }
            return true;
        }

        private void Consumer_ConsumerCancelled(object sender, ConsumerEventArgs e)
        {
            var channel = sender as IModel;
            EntLib.Log.Logger.LogTrace($"Consumer_ConsumerCancelled ChannelNumber: {channel?.ChannelNumber}");
        }

        private void Consumer_Unregistered(object sender, ConsumerEventArgs e)
        {
            var channel = sender as IModel;
            EntLib.Log.Logger.LogTrace($"Consumer_Unregistered ChannelNumber: {channel?.ChannelNumber}");
        }

        private void Consumer_Shutdown(object sender, ShutdownEventArgs e)
        {
            var channel = sender as IModel;
            EntLib.Log.Logger.LogTrace($"Consumer_Shutdown ChannelNumber: {channel?.ChannelNumber}");
        }

        private void Consumer_Registered(object sender, ConsumerEventArgs e)
        {
            var channel = sender as IModel;
            EntLib.Log.Logger.LogTrace($"Consumer_Registered ChannelNumber: {channel?.ChannelNumber}");
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var consumer = sender as EventingBasicConsumer;
            var channel = consumer?.Model;
            //Best.EntLib.Log.Logger.LogTrace($"Consumer_Received ChannelNumber: {channel?.ChannelNumber}");

            // process the message
            if (OnMessageReceived != null)
            {
                try
                {
                    OnMessageReceived(this, new MessageReceiveEventArgs(e.Body, null, e));
                    // ack
                    channel?.BasicAck(e.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    EntLib.Log.Logger.LogError("OnMessageReceived Exception:", ex);
                    // Requeue if exception
                    channel?.BasicReject(e.DeliveryTag, false);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Close()
        {
            _isConsuming = false;

            if (_mqConnectionPoolList != null)
            {
                foreach (var connectionPool in _mqConnectionPoolList)
                {
                    connectionPool.Dispose();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~MessageReceiver()
        {
            Close();
        }
    }
}
