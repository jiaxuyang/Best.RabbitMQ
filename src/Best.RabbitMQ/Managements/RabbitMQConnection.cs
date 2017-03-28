using System;
using System.Diagnostics;
using RabbitMQ.Client;

namespace Best.RabbitMQ.Managements
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitMQConnection : IConnection
    {
        /// <summary>
        /// 
        /// </summary>
        private RabbitMQConnectionPool ConnectionPool { get; }

        private IModel _channel;

        /// <summary>
        /// 
        /// </summary>
        public string AMQPUrl { get; }

        /// <summary>
        /// 
        /// </summary>
        public string HostName { get; }

        /// <summary>
        /// 
        /// </summary>
        public string VHost { get; }

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// 
        /// </summary>
        public IModel Channel => _channel;

        /// <summary>
        /// 
        /// </summary>
        public int ChannelNumber => _channel.ChannelNumber;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="amqpUrl"></param>
        /// <param name="userName"></param>
        /// <param name="connectionPool"></param>
        /// <param name="hostName"></param>
        /// <param name="vHost"></param>
        internal RabbitMQConnection(
            IModel channel,
            string amqpUrl,
            string hostName,
            string vHost,
            string userName,
            RabbitMQConnectionPool connectionPool)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel), $"{nameof(channel)} must set");

            _channel = channel;
            AMQPUrl = amqpUrl;
            HostName = hostName;
            VHost = vHost;
            UserName = userName;
            ConnectionPool = connectionPool;
        }

        private volatile bool _isDisposed;

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(false);
        }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Dispose(bool forceDispose)
        {
            if (_isDisposed
                || _channel == null)
            {
                // do nothing
            }
            else if (forceDispose)
            {
                ForceClose();
                _isDisposed = true;
            }
            else if (!_channel.IsOpen)
            {
                ForceClose();
                _isDisposed = true;
            }
            // without any error, just back connection to pool
            else
            {
                ConnectionPool.Enqueue(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ForceClose()
        {
            // hbase thrift error, real dispose all resource
            if (_channel != null)
            {
                try
                {
                    _channel.Dispose();
                    _channel = null;
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                        Debug.WriteLine(ex.Message + ex.StackTrace);
                }
            }

            try
            {
#if NET46
                // performanceCounter increase
                RabbitMQPerformanceCounter.Instance.CloseChannel();
#endif
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine(ex.Message + ex.StackTrace);
            }

            // when connection disposed really, then decrease pool counter, then new connection can be created
            ConnectionPool.Decrease();
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~RabbitMQConnection()
        {
            Dispose();
        }
    }
}
