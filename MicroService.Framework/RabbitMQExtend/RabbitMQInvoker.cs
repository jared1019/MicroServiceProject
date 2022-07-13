using MicroService.Core.RabbitMQ;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace AgileFramework.Core
{
    public class RabbitMQInvoker
    {
        #region Identity
        private readonly RabbitMQOptions _rabbitMQOptions;
        private readonly string _HostName;
        private readonly string _UserName;
        private readonly string _Password;
        public RabbitMQInvoker(IOptionsMonitor<RabbitMQOptions> optionsMonitor) : this(optionsMonitor.CurrentValue.HostName, optionsMonitor.CurrentValue.UserName, optionsMonitor.CurrentValue.Password)
        {
            this._rabbitMQOptions = optionsMonitor.CurrentValue;
        }

        public RabbitMQInvoker(string hostName, string userName, string password)
        {
            this._HostName = hostName;
            this._UserName = userName;
            this._Password = password;
        }
        #endregion

        #region Init
        private static object RabbitMQInvoker_BindQueueLock = new object();
        private static Dictionary<string, bool> RabbitMQInvoker_ExchangeQueue = new Dictionary<string, bool>();
        private void InitBindQueue(RabbitMQConsumerModel rabbitMQConsumerModel)
        {
            if (!RabbitMQInvoker_ExchangeQueue.ContainsKey($"InitBindQueue_{rabbitMQConsumerModel.ExchangeName}_{rabbitMQConsumerModel.QueueName}"))
            {
                lock (RabbitMQInvoker_BindQueueLock)
                {
                    if (!RabbitMQInvoker_ExchangeQueue.ContainsKey($"InitBindQueue_{rabbitMQConsumerModel.ExchangeName}_{rabbitMQConsumerModel.QueueName}"))
                    {
                        this.InitConnection();
                        using (IModel channel = _CurrentConnection.CreateModel())
                        {
                            channel.ExchangeDeclare(exchange: rabbitMQConsumerModel.ExchangeName, type: ExchangeType.Fanout, durable: true, autoDelete: false, arguments: null);
                            channel.QueueDeclare(queue: rabbitMQConsumerModel.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                            channel.QueueBind(queue: rabbitMQConsumerModel.QueueName, exchange: rabbitMQConsumerModel.ExchangeName, routingKey: string.Empty, arguments: null);
                        }
                        RabbitMQInvoker_ExchangeQueue[$"InitBindQueue_{rabbitMQConsumerModel.ExchangeName}_{rabbitMQConsumerModel.QueueName}"] = true;
                    }
                }
            }
        }
        /// <summary>
        /// 必须先声明exchange--检查+初始化
        /// </summary>
        /// <param name="rabbitMQConsumerModel"></param>
        private void InitExchange(string exchangeName)
        {
            if (!RabbitMQInvoker_ExchangeQueue.ContainsKey($"InitExchange_{exchangeName}"))//没用api确认
            {
                lock (RabbitMQInvoker_BindQueueLock)
                {
                    if (!RabbitMQInvoker_ExchangeQueue.ContainsKey($"InitExchange_{exchangeName}"))
                    {
                        this.InitConnection();
                        using (IModel channel = _CurrentConnection.CreateModel())
                        {
                            channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout, durable: true, autoDelete: false, arguments: null);
                        }
                        RabbitMQInvoker_ExchangeQueue[$"InitExchange_{exchangeName}"] = true;
                    }
                }
            }
        }
        //public void UnBindQueue(string exchangeName, string queueName)
        //{
        //}

        private static object RabbitMQInvoker_InitLock = new object();
        private static IConnection _CurrentConnection = null;//链接做成单例重用--channel是新的
        private void InitConnection()
        {
            if (_CurrentConnection == null || !_CurrentConnection.IsOpen)
            {
                lock (RabbitMQInvoker_InitLock)
                {
                    if (_CurrentConnection == null || !_CurrentConnection.IsOpen)
                    {
                        var factory = new ConnectionFactory()
                        {
                            HostName = this._HostName,
                            Password = this._Password,
                            UserName = this._UserName
                        };
                        _CurrentConnection = factory.CreateConnection();
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 只管exchange---
        /// 4种路由类型？
        /// 
        /// Send前完成交换机初始化
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="message">建议Json格式</param>
        public void Send(string exchangeName, string message)
        {
            this.InitExchange(exchangeName);

            if (_CurrentConnection == null || !_CurrentConnection.IsOpen)
            {
                this.InitConnection();
            }
            using (var channel = _CurrentConnection.CreateModel())//开辟新的信道通信
            {
                try
                {
                    channel.TxSelect();//开启Tx事务---RabbitMQ协议级的事务-----强事务

                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: exchangeName,
                                         routingKey: string.Empty,
                                         basicProperties: null,
                                         body: body);
                    channel.TxCommit();//提交
                    Console.WriteLine($" [x] Sent {body.Length}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine($"【{message}】发送到Broker失败！{ex.Message}");
                    channel.TxRollback(); //事务回滚--前面的所有操作就全部作废了。。。。
                }
            }
        }

        #region Receive
        /// <summary>
        /// 注册处理动作
        /// </summary>
        /// <param name="rabbitMQConsumerMode"></param>
        /// <param name="func"></param>
        public void RegistReciveAction(RabbitMQConsumerModel rabbitMQConsumerMode, Func<string, bool> func)
        {
            this.InitBindQueue(rabbitMQConsumerMode);

            Task.Run(() =>
            {
                using (var channel = _CurrentConnection.CreateModel())
                {
                    var consumer = new EventingBasicConsumer(channel);
                    channel.BasicQos(0, 0, true);
                    consumer.Received += (sender, ea) =>
                    {
                        string str = Encoding.UTF8.GetString(ea.Body.ToArray());
                        if (func(str))
                        {
                            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);//确认已消费
                        }
                        else
                        {
                            //channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: true);//放回队列--重新包装信息，放入其他队列
                        }
                    };
                    channel.BasicConsume(queue: rabbitMQConsumerMode.QueueName,
                                         autoAck: false,//不ACK
                                         consumer: consumer);
                    Console.WriteLine($" Register Consumer To {rabbitMQConsumerMode.ExchangeName}-{rabbitMQConsumerMode.QueueName}");
                    Console.ReadLine();
                }
            });
        }
        #endregion
    }
}
