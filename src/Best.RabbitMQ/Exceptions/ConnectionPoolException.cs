namespace Best.RabbitMQ.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class ConnectionPoolException : System.Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        // ReSharper disable once UnusedMember.Global
        public ConnectionPoolException(string message)
            : base(message)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ConnectionPoolException(string message, System.Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
