using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Core
{
    public class PollingDispatcher : AbstractConsulDispatcher
    {
        private static int _totalCount = 0;

        private static int TotalCount
        {
            get { return _totalCount; }
            set { _totalCount = value >= Int32.MaxValue ? 0 : value; }
        }

        public PollingDispatcher(IOptionsMonitor<ConsulClientOptions> options) : base(options)
        {
        }

        protected override int GetIndex()
        {
            return TotalCount++ % base._CurrentAgentServiceDictionary.Length;
        }
    }
}
