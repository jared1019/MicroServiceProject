using AgileFramework.Core;
using MicroService.Core.RabbitMQ;
using MicroService.Model;
using Newtonsoft.Json;

string HostName = "192.168.5.150", UserName = "guest", Password = "guest";

RabbitMQInvoker _rabbitMQInvoker = new RabbitMQInvoker(HostName, UserName, Password);

RabbitMQConsumerModel rabbitMQConsumerModel = new RabbitMQConsumerModel()
{
    ExchangeName = "user.Exchange",
    QueueName = "user.queue.list"
};
HttpClient _HttpClient = new HttpClient();

_rabbitMQInvoker.RegistReciveAction(rabbitMQConsumerModel, message =>
{
    var userList = JsonConvert.DeserializeObject<List<User>>(message);
    return true;
});
Console.WriteLine("消费端已经启动");
Console.ReadKey();
