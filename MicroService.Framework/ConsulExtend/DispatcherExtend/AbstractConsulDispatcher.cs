using Consul;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Core
{
    public abstract class AbstractConsulDispatcher
    {
        protected ConsulClientOptions _ConsulClientOption = null;
        protected KeyValuePair<string, AgentService>[] _CurrentAgentServiceDictionary;

        protected AbstractConsulDispatcher(IOptionsMonitor<ConsulClientOptions> options)
        {
            _ConsulClientOption = options.CurrentValue;
        }

        /// <summary>
        /// 负载均衡获取地址
        /// </summary>
        /// <param name="mappingUrl">Consul映射后的地址</param>
        /// <returns></returns>
        public string MapAddress(string mappingUrl)
        {
            Uri uri = new Uri(mappingUrl);
            string serviceName = uri.Host;
            string addressPort = this.ChooseAddress(serviceName);
            return $"{uri.Scheme}://{addressPort}{uri.PathAndQuery}";
        }

        /// <summary>
        /// 根据服务名字来获取地址
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        protected virtual string ChooseAddress(string serviceName)
        {
            this.InitAgentServiceDictionary(serviceName);

            int index = this.GetIndex();
            AgentService agentService = this._CurrentAgentServiceDictionary[index].Value;
            return $"{agentService.Address}:{agentService.Port}";
        }

        /// <summary>
        /// 跟Consul交互，获取清单
        /// </summary>
        /// <param name="serviceName"></param>
        private void InitAgentServiceDictionary(string serviceName)
        {
            ConsulClient client = new ConsulClient(c =>
            {
                c.Address = new Uri($"http://{this._ConsulClientOption.IP}:{this._ConsulClientOption.Port}/");
                c.Datacenter = this._ConsulClientOption.Datacenter;
            });

            //升级consul实例获取
            var entrys = client.Health.Service(serviceName).Result.Response;
            List<KeyValuePair<string, AgentService>> serviceList = new List<KeyValuePair<string, AgentService>>();
            for (int i = 0; i < entrys.Length; i++)
            {
                serviceList.Add(new KeyValuePair<string, AgentService>(i.ToString(), entrys[i].Service));
            }

            this._CurrentAgentServiceDictionary = serviceList.ToArray();
        }

        // <summary>
        /// 根据不同策略  获得不同的索引
        /// </summary>
        /// <returns></returns>
        protected abstract int GetIndex();
    }
}
