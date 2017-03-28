using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Best.RabbitMQ.Managements;
using RabbitMQ.Client;

namespace Best.RabbitMQ
{
    /// <summary>
    /// Send messages to first available server in the HAConnectionFactory provided.
    /// </summary>
    public class MessageSender : IDisposable
    {
        private HAConnectionFactory HAConnectionFactory { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="haConnectionFactory"></param>
        public MessageSender(HAConnectionFactory haConnectionFactory)
        {
            HAConnectionFactory = haConnectionFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName">if string.Empty, will use (AMQP Default) exchagne, routingKey is queue name</param>
        /// <param name="routingKey"></param>
        /// <param name="msg"></param>
        /// <param name="persistent"></param>
        /// <param name="headers"></param>
        public void Publish(string exchangeName, string routingKey, string msg, bool persistent = true, IDictionary<string, object> headers = null)
        {
            Publish(exchangeName, routingKey, Encoding.UTF8.GetBytes(msg), persistent, headers);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName">if string.Empty, will use (AMQP Default) exchagne, routingKey is queue name</param>
        /// <param name="routingKey"></param>
        /// <param name="msg"></param>
        /// <param name="persistent"></param>
        /// <param name="headers"></param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Publish(string exchangeName, string routingKey, byte[] msg, bool persistent = true, IDictionary<string, object> headers = null)
        {
            if (string.IsNullOrEmpty(routingKey))
                throw new ArgumentNullException(nameof(routingKey));

            IBasicProperties propties;

            string rmqUrl = null;
            try
            {
                using (var connection = HAConnectionFactory.CreateConnection())
                {
                    rmqUrl = connection.AMQPUrl;

                    propties = connection.Channel.CreateBasicProperties();
                    propties.Persistent = persistent;
                    propties.Headers = headers;
                    connection.Channel.BasicPublish(exchangeName, routingKey, propties, msg);
                    // no error, end publish
#if NET46
                    // increase performance counter
                    RabbitMQPerformanceCounter.Instance.PublishMessage();
#endif
                    return;
                }
            }
            catch (Exception ex)
            {
                EntLib.Log.Logger.LogWarn($"ThreadId:{Thread.CurrentThread.ManagedThreadId} publish message error, will reconnect", ex);
                if (Debugger.IsAttached)
                    Debug.WriteLine("MessageSender.Send Error:" + ex.Message);
            }

            // with error, reconnect and resend
            try
            {
                EntLib.Log.Logger.LogWarn("RabbitMQ reconnect and resend");

                if (rmqUrl != null)
                    HAConnectionFactory.ReConnect(rmqUrl);

                using (var connection = HAConnectionFactory.CreateConnection())
                {
                    propties = connection.Channel.CreateBasicProperties();
                    propties.Persistent = persistent;
                    propties.Headers = headers;
                    connection.Channel.BasicPublish(exchangeName, routingKey, propties, msg);
#if NET46
                    // increase performance counter
                    RabbitMQPerformanceCounter.Instance.PublishMessage();
#endif
                }
            }
            catch (Exception ex)
            {
                EntLib.Log.Logger.LogError("RabbitMQ reconnect and resend fail", ex);
                if (Debugger.IsAttached)
                    Debug.WriteLine("MessageSender.Send Retry Error:" + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Close()
        {

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
        ~MessageSender()
        {
            Close();
        }
    }
}
