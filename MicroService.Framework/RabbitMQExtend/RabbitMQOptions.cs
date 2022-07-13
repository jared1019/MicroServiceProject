namespace MicroService.Core.RabbitMQ
{
    public class RabbitMQOptions
    {
        /// <summary>
        /// 虚拟机名称
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
    }

    public class RabbitMQConsumerModel
    {
        /// <summary>
        /// 生产者指定，交换机
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }
    }
}
