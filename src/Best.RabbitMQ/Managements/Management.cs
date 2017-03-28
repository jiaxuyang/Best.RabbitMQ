using System;
using System.Collections.Generic;

namespace Best.RabbitMQ.Managements
{
    /// <summary>
    /// 
    /// </summary>
    public class Management : IDisposable
    {
        private readonly IList<IConnection> _connections;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionFactory"></param>
        public Management(HAConnectionFactory connectionFactory)
        {
            _connections = connectionFactory.CreateAllConnection();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="exchangeType">ExchangeType</param>
        public void ExchangeDeclare(string exchangeName, string exchangeType)
        {
            foreach (var conn in _connections)
            {
                conn.Channel.ExchangeDeclare(exchangeName, exchangeType, true, false, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueName"></param>
        // ReSharper disable once RedundantOverload.Global
        public void QueueDeclare(string queueName)
        {
            QueueDeclare(queueName, true, false, false, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="durable"></param>
        /// <param name="exclusive"></param>
        /// <param name="autoDelete"></param>
        /// <param name="arguments"></param>
        public void QueueDeclare(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            foreach (var conn in _connections)
            {
                conn.Channel.QueueDeclare(queueName, durable, exclusive, autoDelete, arguments);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        public void QueueBind(string queueName, string exchangeName, string routingKey)
        {
            foreach (var conn in _connections)
            {
                conn.Channel.QueueBind(queueName, exchangeName, routingKey, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Close()
        {
            foreach (var connection in _connections)
            {
                connection.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}
