using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RabbitMQ.Client;

namespace Best.RabbitMQ.Managements
{
    /// <summary>
    /// RabbitMQ HA ConnectionFactory
    /// </summary>
    public class HAConnectionFactory
    {
        /// <summary>
        /// 
        /// </summary>
        public enum HAMode
        {
            /// <summary>
            /// 
            /// </summary>
            MasterSlave,
            /// <summary>
            /// 
            /// </summary>
            LoadBalance
        }

        private readonly Random _random = new Random();

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string AMQPUrls { get; }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public List<string> HAConnectionUrlList { get; }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public ushort ChannelMax { get; }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int ConnectionTimeout { get; }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public uint FrameMax { get; }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public ushort Heartbeat { get; }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public HAMode HAConnMode { get; }

        /// <summary>
        /// Amount of time client will wait for before re-trying to recover connection.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int NetworkRecoveryInterval { get; }

        private static ConcurrentDictionary<string, RabbitMQConnectionPool> UrlPoolMap { get; }

        static HAConnectionFactory()
        {
            UrlPoolMap = new ConcurrentDictionary<string, RabbitMQConnectionPool>();
#if NET46
            var perfCounter = RabbitMQPerformanceCounter.Instance;
#endif
        }

        /// <summary>
        /// Ctor
        /// Default Value:
        ///     ChannelMax:        10000
        ///     connectinTimeout:  3000
        ///     FrameMax:          1024000
        ///     Heartbeat:         60
        /// </summary>
        /// <param name="amqpAMQPUrls">amqp://username:password@servername1:port/vhost;amqp://username:password@servername2:port/vhost;amqp://username:password@servername3:port/vhost</param>
        /// <param name="haMode">Default use master/Slave mode</param>
        public HAConnectionFactory(string amqpAMQPUrls, HAMode haMode = HAMode.LoadBalance)
            // ReSharper disable once RedundantArgumentDefaultValue
            : this(amqpAMQPUrls, 10240, 3000, 1024000, 60, haMode, 120)
        {

        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="amqpAMQPUrls">amqp://username:password@servername1:port/vhost;amqp://username:password@servername2:port/vhost;amqp://username:password@servername3:port/vhost</param>
        /// <param name="channelMax">Maximum channel number to ask for</param>
        /// <param name="connectinTimeout">Timeout setting for connection attempts (in milliseconds)</param>
        /// <param name="frameMax">Frame-max parameter to ask for (in bytes)</param>
        /// <param name="heartBeat">Heartbeat setting to request (in seconds)</param>
        /// <param name="haMode">Default use master/Slave mode</param>
        /// <param name="networkRecoveryInterval">Amount of time client will wait for before re-trying to recover connection.</param>
        public HAConnectionFactory(
            string amqpAMQPUrls,
            ushort channelMax,
            int connectinTimeout,
            uint frameMax,
            ushort heartBeat,
            HAMode haMode = HAMode.LoadBalance,
            int networkRecoveryInterval = 120)
        {
            if (string.IsNullOrWhiteSpace(amqpAMQPUrls))
                throw new ArgumentNullException(nameof(amqpAMQPUrls), "Must set rabbitMQ server urls");
            if (channelMax < 1)
                throw new ArgumentNullException(nameof(channelMax), "ChannelMax Must > 0");
            if (connectinTimeout < 1)
                throw new ArgumentNullException(nameof(connectinTimeout), "connectinTimeout Must > 0");
            if (frameMax < 1)
                throw new ArgumentNullException(nameof(frameMax), "FrameMax Must > 0");
            if (heartBeat < 1)
                throw new ArgumentNullException(nameof(heartBeat), "Heartbeat Must > 0");

            HAConnectionUrlList = new List<string>();

            var urls = amqpAMQPUrls.Split(';').Where(t => !string.IsNullOrWhiteSpace(t));
            HAConnectionUrlList.AddRange(urls);

            AMQPUrls = amqpAMQPUrls;
            ChannelMax = channelMax;
            ConnectionTimeout = connectinTimeout;
            FrameMax = frameMax;
            Heartbeat = heartBeat;
            HAConnMode = haMode;
            NetworkRecoveryInterval = networkRecoveryInterval;

            foreach (var url in HAConnectionUrlList)
            {
                AddToMapConnectionPool(url);
            }
        }

        private RabbitMQConnectionPool AddToMapConnectionPool(string amqpUrl)
        {
            RabbitMQConnectionPool pool;
            if (!UrlPoolMap.TryGetValue(amqpUrl, out pool))
            {
                var connFactory = new ConnectionFactory
                {
                    Uri = amqpUrl,
                    RequestedChannelMax = ChannelMax,
                    RequestedConnectionTimeout = ConnectionTimeout,
                    RequestedFrameMax = FrameMax,
                    RequestedHeartbeat = Heartbeat,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(NetworkRecoveryInterval),
                    TopologyRecoveryEnabled = true
                };
                pool = new RabbitMQConnectionPool(connFactory, amqpUrl, ChannelMax);
                UrlPoolMap.TryAdd(amqpUrl, pool);
            }
            return pool;
        }

        /// <summary>
        /// Create a connection to the first available server in the list provided.
        /// </summary>
        /// <returns></returns>
        public IConnection CreateConnection()
        {
            IConnection conn = null;
            switch (HAConnMode)
            {
                case HAMode.MasterSlave:
                    {
                        foreach (var connectionUrl in HAConnectionUrlList)
                        {
                            try
                            {
                                // Get or new pool obj
                                RabbitMQConnectionPool pool;
                                if (!UrlPoolMap.TryGetValue(connectionUrl, out pool))
                                {
                                    pool = AddToMapConnectionPool(connectionUrl);
                                }

                                // get channel from pool
                                conn = pool.Dequeue();
                                break;
                            }
                            catch (Exception ex)
                            {
#if DEBUG
                                Debug.WriteLine("HAConnectionFactory.CreateConnection {0} Error:{1}", connectionUrl, ex.Message);
#endif
                            }
                        }
                    }
                    break;
                case HAMode.LoadBalance:
                    {
                        var tryedConnUrlList = new List<string>();
                        do
                        {
                            var rndIndex = _random.Next(0, HAConnectionUrlList.Count);
                            var rndConnUrl = HAConnectionUrlList[rndIndex];
                            tryedConnUrlList.Add(rndConnUrl);
                            try
                            {
                                // Get or new pool obj
                                RabbitMQConnectionPool pool;
                                if (!UrlPoolMap.TryGetValue(rndConnUrl, out pool))
                                {
                                    pool = AddToMapConnectionPool(rndConnUrl);
                                }

                                // get channel from pool
                                conn = pool.Dequeue();
                                break;
                            }
                            catch (Exception ex)
                            {
#if DEBUG
                                Debug.WriteLine("HAConnectionFactory.CreateConnection {0} Error:{1}", rndConnUrl,
                                    ex.Message);
#endif
                            }

                        } while (tryedConnUrlList.Count < HAConnectionUrlList.Count);
                    }
                    break;
                default:
                    // ReSharper disable once NotResolvedInText
                    throw new ArgumentOutOfRangeException("HAConnMode", "HAConnMode not support:" + HAConnMode);
            }

            if (conn != null)
            {
                // perf counter increase
                return conn;
            }

            throw new Exception("HAConnectionFactory.CreateConnection all rabbitmq can not create connection");
        }

        /// <summary>
        /// Create connection list to all available servers in the list provided.
        /// </summary>
        /// <returns></returns>
        public IList<IConnection> CreateAllConnection()
        {
            IList<IConnection> connList = new List<IConnection>();
            foreach (var connectionUrl in HAConnectionUrlList)
            {
                try
                {
                    // Get or new pool obj
                    RabbitMQConnectionPool pool;
                    if (!UrlPoolMap.TryGetValue(connectionUrl, out pool))
                    {
                        pool = AddToMapConnectionPool(connectionUrl);
                    }
                    // get channel from pool
                    var conn = pool.Dequeue();
                    connList.Add(conn);
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine("HAConnectionFactory.CreateConnection {0} Error:{1}", connectionUrl,
                        ex.Message);
#endif
                }
            }
            return connList;
        }

        /// <summary>
        /// Create connection list to all available servers in the list provided.
        /// Use by consumer
        /// </summary>
        /// <returns></returns>
        public IList<RabbitMQConnectionPool> CreateAllConnectionPool()
        {
            IList<RabbitMQConnectionPool> connList = new List<RabbitMQConnectionPool>();
            foreach (var connectionUrl in HAConnectionUrlList)
            {
                try
                {
                    // Get or new pool obj
                    RabbitMQConnectionPool pool;
                    if (!UrlPoolMap.TryGetValue(connectionUrl, out pool))
                    {
                        pool = AddToMapConnectionPool(connectionUrl);
                    }
                    // get channel from pool
                    connList.Add(pool);
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine("HAConnectionFactory.CreateConnection {0} Error:{1}", connectionUrl,
                        ex.Message);
#endif
                }
            }
            return connList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rabbitMQUrl"></param>
        public void ReConnect(string rabbitMQUrl)
        {
            // Get or new pool obj
            RabbitMQConnectionPool pool;
            if (!UrlPoolMap.TryGetValue(rabbitMQUrl, out pool))
            {
                pool = AddToMapConnectionPool(rabbitMQUrl);
            }
            pool.ReConnect();
        }
    }
}
