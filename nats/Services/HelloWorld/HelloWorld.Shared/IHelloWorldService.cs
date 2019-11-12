using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld.Shared
{
    public interface IHelloWorldService
    {
        Task<string> SayHello();
    }
}
