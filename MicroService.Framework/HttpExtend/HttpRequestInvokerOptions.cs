using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Core.HttpExtend
{
    public class HttpRequestInvokerOptions
    {
        public string? Message { get; set; } = "test Message";
        public string Version { get; set; } = "1.0";
        public bool IsUseHttpClient { get; set; } = true;
    }
}
