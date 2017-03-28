using System;
using RabbitMQ.Client;

namespace Best.RabbitMQ.Managements
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConnection : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        string AMQPUrl { get; }

        /// <summary>
        /// 
        /// </summary>
        IModel Channel { get; }

        /// <summary>
        /// 
        /// </summary>
        int ChannelNumber { get; }

        /// <summary>
        /// 
        /// </summary>
        string HostName { get; }

        /// <summary>
        /// 
        /// </summary>
        string VHost { get; }

        /// <summary>
        /// 
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// 
        /// </summary>
        void Close();

        /// <summary>
        /// 
        /// </summary>
        void ForceClose();
    }
}
