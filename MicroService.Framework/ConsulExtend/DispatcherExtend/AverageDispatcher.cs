using Microsoft.Extensions.Options;

namespace MicroService.Core
{
    internal class AverageDispatcher : AbstractConsulDispatcher
    {
        private static int _totalCount = 0;
        private static int TotalCount
        {
            get { return _totalCount; }
            set { _totalCount = value >= Int32.MaxValue ? 0 : value; }
        }
        public AverageDispatcher(IOptionsMonitor<ConsulClientOptions> options) : base(options)
        {
        }

        protected override int GetIndex()
        {
            return new Random(TotalCount++).Next(0, base._CurrentAgentServiceDictionary.Length);
        }
    }
}
