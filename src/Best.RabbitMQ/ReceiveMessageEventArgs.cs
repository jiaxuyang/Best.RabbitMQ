using System;
using System.Text;
using RabbitMQ.Client.Events;

namespace Best.RabbitMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageReceiveEventArgs : EventArgs
    {
        private Encoding _messageEncoding;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="consumerTag"></param>
        /// <param name="rawMessage"></param>
        public MessageReceiveEventArgs(byte[] rawData, string consumerTag = null, BasicDeliverEventArgs rawMessage = null)
        {
            _messageEncoding = Encoding.UTF8;

            ConsumerTag = consumerTag;
            RawData = rawData;

            if (RawData == null || RawData.Length == 0)
                Message = null;
            else
                Message = MessageEncoding.GetString(RawData);

            RawMessage = rawMessage;
        }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string ConsumerTag { get; }

        /// <summary>
        /// 
        /// </summary>

        // ReSharper disable once MemberCanBePrivate.Global
        public byte[] RawData { get; }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Message { get; private set; }

        /// <summary>
        /// RabbitMQ raw message
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public BasicDeliverEventArgs RawMessage { get; }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public Encoding MessageEncoding
        {
            get { return _messageEncoding; }
            // ReSharper disable once UnusedMember.Global
            set
            {
                _messageEncoding = value;

                if (RawData == null || RawData.Length == 0)
                    Message = null;
                else
                    Message = MessageEncoding.GetString(RawData);
            }
        }
    }
}
