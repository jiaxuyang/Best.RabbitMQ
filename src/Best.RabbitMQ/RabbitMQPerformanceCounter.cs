#if NET46
using System;
using System.Diagnostics;
using Best.EntLib.Log;
using Best.PerformanceCounterExt;

namespace Best.RabbitMQ
{
    /// <summary>
    /// 
    /// </summary>
    [PerformanceCounterCategory("Best.RabbitMQ", "Best RabbitMQ Client performances")]
    internal class RabbitMQPerformanceCounter : IDisposable
    {
        private static readonly object LockObject = new object();

        private static RabbitMQPerformanceCounter _instance;

        private static volatile bool _isRegisted;

        /// <summary>
        /// 
        /// </summary>
        public static RabbitMQPerformanceCounter Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new RabbitMQPerformanceCounter();
                            try
                            {
                                if (!_isRegisted)
                                {
                                    PerformanceCounterManagerExt.Register(_instance);
                                    if (_instance.Connections == null)
                                        throw new Exception("RabbitMQ performance counter register fail");
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogWarn("PerformanceCounterManagerExt.Register:", ex);
                            }
                            finally
                            {
                                _isRegisted = true;
                            }
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [PerformanceCounter("Connections", "RabbitMQ connections in this machine", PerformanceCounterType.NumberOfItems64, true)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public PerformanceCounter ConnectionsGlobal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [PerformanceCounter("Connections", "RabbitMQ connections in certain application", PerformanceCounterType.NumberOfItems64)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public PerformanceCounter Connections { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [PerformanceCounter("Channels", "RabbitMQ channels in this machine", PerformanceCounterType.NumberOfItems64, true)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public PerformanceCounter ChannelsGlobal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [PerformanceCounter("Channels", "RabbitMQ channels in certain application", PerformanceCounterType.NumberOfItems64)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public PerformanceCounter Channels { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [PerformanceCounter("PublishMessages", "RabbitMQ publish messages in this machine", PerformanceCounterType.NumberOfItems64, true)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public PerformanceCounter PublishMessagesGlobal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [PerformanceCounter("PublishMessages", "RabbitMQ publish messages in certain application", PerformanceCounterType.NumberOfItems64)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public PerformanceCounter PublishMessages { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [PerformanceCounter("PublishMessages/sec", "RabbitMQ publish messages per second in this machine", PerformanceCounterType.RateOfCountsPerSecond64, true)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public PerformanceCounter PublishMessagesPerSecGlobal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [PerformanceCounter("PublishMessages/sec", "RabbitMQ publish messages per second in certain application", PerformanceCounterType.RateOfCountsPerSecond64)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public PerformanceCounter PublishMessagesPerSec { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [PerformanceCounter("ConsumeMessages", "RabbitMQ consume messages in this machine", PerformanceCounterType.NumberOfItems64, true)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public PerformanceCounter ConsumeMessagesGlobal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [PerformanceCounter("ConsumeMessages", "RabbitMQ consume messages in certain application", PerformanceCounterType.NumberOfItems64)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public PerformanceCounter ConsumeMessages { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [PerformanceCounter("ConsumeMessages/sec", "RabbitMQ consume messages per second in this machine", PerformanceCounterType.RateOfCountsPerSecond64, true)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public PerformanceCounter ConsumeMessagesPerSecGlobal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [PerformanceCounter("ConsumeMessages/sec", "RabbitMQ consume messages per second in certain application", PerformanceCounterType.RateOfCountsPerSecond64)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public PerformanceCounter ConsumeMessagesPerSec { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public void CreateConnection()
        {
            ConnectionsGlobal?.Increment();
            Connections?.Increment();
        }

        /// <summary>
        /// 
        /// </summary>
        public void CloseConnection()
        {
            ConnectionsGlobal?.Decrement();
            Connections?.Decrement();
        }

        /// <summary>
        /// 
        /// </summary>
        public void CreateChannel()
        {
            ChannelsGlobal?.Increment();
            Channels?.Increment();
        }

        /// <summary>
        /// 
        /// </summary>
        public void CloseChannel()
        {
            ChannelsGlobal?.Decrement();
            Channels?.Decrement();
        }

        /// <summary>
        /// 
        /// </summary>
        public void PublishMessage()
        {
            PublishMessagesGlobal?.Increment();
            PublishMessages?.Increment();
            PublishMessagesPerSecGlobal?.Increment();
            PublishMessagesPerSec?.Increment();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ConsumeMessage()
        {
            ConsumeMessagesGlobal?.Increment();
            ConsumeMessages?.Increment();
            ConsumeMessagesPerSecGlobal?.Increment();
            ConsumeMessagesPerSec?.Increment();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            ConnectionsGlobal?.Close();
            Connections?.Close();

            ChannelsGlobal?.Close();
            Channels?.Close();

            PublishMessagesGlobal?.Close();
            PublishMessages?.Close();
            PublishMessagesPerSecGlobal?.Close();
            PublishMessagesPerSec?.Close();

            ConsumeMessagesGlobal?.Close();
            ConsumeMessages?.Close();
            ConsumeMessagesPerSecGlobal?.Close();
            ConsumeMessagesPerSec?.Close();
        }

        ~RabbitMQPerformanceCounter()
        {
            Dispose();
        }
    }
}
#endif