using HelloWorld.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld.ApplicationService
{
    public class HelloWorldApplicationService : IHelloWorldService
    {
        public async Task<string> SayHello()
        {
            return "Hello World!";
        }
    }
}
