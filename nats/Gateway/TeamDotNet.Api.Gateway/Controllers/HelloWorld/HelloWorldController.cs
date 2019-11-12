using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloWorld.Shared;
using Microsoft.AspNetCore.Mvc;

namespace TeamDotNet.Api.Gateway.Controllers.HelloWorld
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelloWorldController : ControllerBase
    {
        private readonly IHelloWorldService _service;

        public HelloWorldController(IHelloWorldService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _service.SayHello();
            return Ok(result);
        }
    }
}