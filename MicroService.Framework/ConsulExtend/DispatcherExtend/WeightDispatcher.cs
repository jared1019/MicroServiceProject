using Consul;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Core
{
    public class WeightDispatcher : AbstractConsulDispatcher
    {
        public WeightDispatcher(IOptionsMonitor<ConsulClientOptions> options) : base(options)
        {
        }

        protected override int GetIndex()
        {
            throw new NotImplementedException();
        }

        protected override string ChooseAddress(string serviceName)
        {
            ConsulClient client = new ConsulClient(c =>
            {
                c.Address = new Uri($"http://{base._ConsulClientOption.IP}:{base._ConsulClientOption.Port}/");
                c.Datacenter = base._ConsulClientOption.Datacenter;
            });
            AgentService agentService = null;
            var response = client.Agent.Services().Result.Response;

            this._CurrentAgentServiceDictionary = response.Where(s => s.Value.Service.Equals(serviceName, StringComparison.OrdinalIgnoreCase)).ToArray();


            var serviceDictionaryNew = new List<AgentService>();
            foreach (var service in base._CurrentAgentServiceDictionary)
            {
                serviceDictionaryNew.AddRange(Enumerable.Repeat(service.Value, int.TryParse(service.Value.Tags?[0], out int weight) ? 1 : weight));
            }
            int index = new Random(DateTime.Now.Millisecond).Next(0, int.MaxValue) % serviceDictionaryNew.Count;
            agentService = serviceDictionaryNew[index];

            return $"{agentService.Address}:{agentService.Port}";
        }
    }
}
