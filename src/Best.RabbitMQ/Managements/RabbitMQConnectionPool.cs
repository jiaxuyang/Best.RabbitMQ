using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Best.RabbitMQ.Exceptions;
using RabbitMQ.Client;

namespace Best.RabbitMQ.Managements
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitMQConnectionPool : IDisposable
    {
        private const int ErrorTryIntervalInSecond = 120;

        private int _poolAllocatedLength;

        /// <summary>
        /// 
        /// </summary>
        public ConnectionFactory RMQConnectionFactory { get; }

        private readonly object _lockObject = new object();

        private volatile global::RabbitMQ.Client.IConnection _rabbitMQConnection;

        private DateTime _lastExceptionOccurTime = DateTime.MinValue;

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int PoolLength => _poolAllocatedLength;

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int MaxPoolSize { get; }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public string AMQPUrl { get; }

        private readonly ConcurrentQueue<IConnection> _connectionPoolQueue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rmqConnectionFactory"></param>
        /// <param name="amqpUrl"></param>
        /// <param name="maxPoolSize"></param>
        public RabbitMQConnectionPool(ConnectionFactory rmqConnectionFactory, string amqpUrl, int maxPoolSize = 1000)
        {
            RMQConnectionFactory = rmqConnectionFactory;
            AMQPUrl = amqpUrl;
            MaxPoolSize = maxPoolSize;

            _connectionPoolQueue = new ConcurrentQueue<IConnection>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IConnection Dequeue()
        {
            while (!_connectionPoolQueue.IsEmpty)
            {
                IConnection conn;
                if (_connectionPoolQueue.TryDequeue(out conn)
                    && conn?.Channel != null
                    && conn.Channel.IsOpen)
                {
                    return conn;
                }
            }

            return GetConnection();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        public void Enqueue(IConnection connection)
        {
            _connectionPoolQueue.Enqueue(connection);
        }


        /// <summary>
        /// Decrease counter for error hbase connection, may be remote gateway rebooted
        /// </summary>
        internal void Decrease()
        {
            Interlocked.Decrement(ref _poolAllocatedLength);
        }

        /// <summary>
        /// Increase counter for new hbase connection, new connection will direct return to caller, when new created or exist connection back to pool, will only enqueue
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        internal void Increase()
        {
            Interlocked.Increment(ref _poolAllocatedLength);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IConnection GetConnection()
        {
            // block create for performance reason
            if ((DateTime.Now - _lastExceptionOccurTime).TotalSeconds < ErrorTryIntervalInSecond)
            {
                throw new ConnectionPoolException($"Block create connection/channel after last exception:{_lastExceptionOccurTime},wait {ErrorTryIntervalInSecond}s, Url:{RMQConnectionFactory.Endpoint}/{RMQConnectionFactory.VirtualHost}", null);
            }

            // limix channel number
            if (PoolLength >= MaxPoolSize)
            {
                throw new ConnectionPoolException($"Max pool size was reached. MaxPoolSize={MaxPoolSize}", null);
            }

            CreateConnection();

            try
            {
                var model = _rabbitMQConnection.CreateModel();
#if NET46
                RabbitMQPerformanceCounter.Instance.CreateChannel();
#endif

                Increase();
                return new RabbitMQConnection(
                    model,
                    AMQPUrl,
                    RMQConnectionFactory.HostName,
                    RMQConnectionFactory.VirtualHost,
                    RMQConnectionFactory.UserName,
                    this);
            }
            catch (Exception ex)
            {
                _lastExceptionOccurTime = DateTime.Now;

                if (Debugger.IsAttached)
                    Debug.WriteLine($"GetConnection-CreateModel {ex.Message} {ex.StackTrace}");

                throw new ConnectionPoolException("CreateModel error", ex);
            }
        }

        private volatile bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (_rabbitMQConnection != null && _rabbitMQConnection.IsOpen)
                {
                    _rabbitMQConnection.Dispose();
                    _rabbitMQConnection = null;
                }
                _isDisposed = true;
            }
        }

        private void CreateConnection(bool reconnect = false)
        {
            if (_rabbitMQConnection == null 
                || !_rabbitMQConnection.IsOpen 
                || reconnect)
            {
                lock (_lockObject)
                {
                    try
                    {
                        if (_rabbitMQConnection == null)
                        {
                            _rabbitMQConnection = RMQConnectionFactory.CreateConnection();
#if NET46
                            RabbitMQPerformanceCounter.Instance.CreateConnection();
#endif
                        }
                        else if (!_rabbitMQConnection.IsOpen
                                || reconnect)
                        {
                            _rabbitMQConnection.Dispose();
                            _rabbitMQConnection = null;
#if NET46
                            RabbitMQPerformanceCounter.Instance.CloseConnection();
#endif
                            _rabbitMQConnection = RMQConnectionFactory.CreateConnection();
#if NET46
                            RabbitMQPerformanceCounter.Instance.CreateConnection();
#endif
                        }
                    }
                    catch (Exception ex)
                    {
                        _lastExceptionOccurTime = DateTime.Now;

                        if (Debugger.IsAttached)
                            Debug.WriteLine($"GetConnection-CreateConnection {ex.Message} {ex.StackTrace}");

                        throw new ConnectionPoolException($"CreateConnection error, Url:{RMQConnectionFactory.Endpoint}/{RMQConnectionFactory.VirtualHost}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReConnect()
        {
            var count = _connectionPoolQueue.Count;
            for (int i = 0; i < count; i++)
            {
                IConnection conn;
                _connectionPoolQueue.TryDequeue(out conn);
                conn?.ForceClose();
                Decrease();
            }

            CreateConnection(true);
        }
    }
}
