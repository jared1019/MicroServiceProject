using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Core.HttpExtend
{
    public interface IHttpRequestInvoker
    {
        string InvokeApi(string url);
    }
}
