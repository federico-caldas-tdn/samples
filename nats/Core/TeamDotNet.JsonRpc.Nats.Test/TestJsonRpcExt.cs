using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TeamDotNet.JsonRpc.Nats.Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class TestJsonRpcExt
    {
        [Test]
        public void TestRpcResponseHasErrors()
        {
            var r=new JsonRpcResponse();
            Assert.IsFalse(r.HasError());
            r=new JsonRpcResponse
            {
                Error = new JsonRpcError()
            };
            Assert.IsTrue(r.HasError());
        }

        [Test]
        public void TestNullRespShouldNotReturnError()
        {
            JsonRpcResponse r = null;
            Assert.IsFalse(r.HasError());
        }

        [Test]
        public void TestParseRequestParameters()
        {
            var r=new JsonRpcRequest
            {
                Parameters = new Dictionary<string, string>
                {
                    {"p1","1000"}
                }
            };
            var resp=r.Get<int>("p1", out int val, v => int.Parse(v));
            Assert.AreEqual(1000,val);
            Assert.IsFalse(resp.HasError());
        }
        [Test]
        public void TestParseRequestParametersParameterMissing()
        {
            var r=new JsonRpcRequest
            {
                Parameters = new Dictionary<string, string>
                {
                    {"p1","1000"}
                }
            };
            var resp=r.Get<int>("p2", out int val, v => int.Parse(v));
            Assert.AreEqual(0,val);
            Assert.IsTrue(resp.HasError());
            Assert.AreEqual("p2",resp.Error.Data);
            Assert.AreEqual((int)ErrorCode.InvalidParam,resp.Error.Code);

        }
        [Test]
        public void TestParseRequestParametersParseError()
        {
            var r=new JsonRpcRequest
            {
                Parameters = new Dictionary<string, string>
                {
                    {"p1","xxxxxxx"}
                }
            };
            var resp=r.Get<int>("p1", out int val, v =>int.Parse(v));
            Assert.IsTrue(resp.HasError());
            Assert.AreEqual((int)ErrorCode.ParseError,resp.Error.Code);
            Assert.IsNotNull(resp.Error.Data);
        }

        [Test]
        public void TestRequestParameterExist()
        {
            JsonRpcRequest rq=new JsonRpcRequest
            {
                Id = Guid.NewGuid().ToString(),
                Method = "m1",
                Parameters = new Dictionary<string, string>
                {
                    {"p1","v1"}
                }
            };
            (bool hasValue, string val, JsonRpcResponse r) = rq.Get("p1");
            Assert.IsTrue(hasValue);
            Assert.AreEqual("v1",val);
            Assert.IsNotNull(r);
            Assert.AreEqual(r.Id,rq.Id);
            Assert.IsNull(r.Error);
        }
        [Test]
        public void TestRequestParameterDoesNotExist()
        {
            JsonRpcRequest rq=new JsonRpcRequest
            {
                Id = Guid.NewGuid().ToString(),
                Method = "m1",
                Parameters = new Dictionary<string, string>
                {
                    {"p1","v1"}
                }
            };
            (bool hasValue, string val, JsonRpcResponse r) = rq.Get("p2");
            Assert.IsFalse(hasValue);
            Assert.IsTrue(string.IsNullOrEmpty(val));
            Assert.IsNotNull(r);
            Assert.AreEqual(r.Id,rq.Id);
            Assert.IsNotNull(r.Error);
            Assert.AreEqual((int)ErrorCode.InvalidParam,r.Error.Code);
            Assert.AreEqual("Parameter p2 was not provided.",r.Error.Message);
            Assert.AreEqual("p2",r.Error.Data);

        }
        [Test]
        public void TestProxyRegisterMethod()
        {
            JsonRpcServiceProxyWrapper wrapper=new JsonRpcServiceProxyWrapper();
            wrapper.RegisterMethod("m1", req => { return Task.FromResult<JsonRpcResponse>(new JsonRpcResponse()); });
            Assert.IsTrue(wrapper.HasMethod("m1"));
        }
        [Test]
        public void TestProxyUnregisterMethod()
        {
            JsonRpcServiceProxyWrapper wrapper=new JsonRpcServiceProxyWrapper();
            wrapper.RegisterMethod("m1", req => { return Task.FromResult<JsonRpcResponse>(new JsonRpcResponse()); });
            Assert.IsTrue(wrapper.HasMethod("m1"));
            wrapper.UnregisterMethod("m1");
            Assert.IsFalse(wrapper.HasMethod("m1"));

        }
        [Test]
        public void TestProxyExecuteMethodAsync()
        {
            JsonRpcServiceProxyWrapper wrapper=new JsonRpcServiceProxyWrapper();
            wrapper.RegisterMethod("m1", req => { return Task.FromResult<JsonRpcResponse>(new JsonRpcResponse
            {
                Id = Guid.NewGuid().ToString(),
                Result = "Result"
            }); });
            var resp=wrapper.ServeAsync(new JsonRpcRequest
            {
                Method = "m1"
            }).GetAwaiter().GetResult();
            Assert.IsNotNull(resp);
            Assert.AreEqual("Result",resp.Result);
        }
        [Test]
        public void TestProxyExecuteMethodExceptionAsync()
        {
            JsonRpcServiceProxyWrapper wrapper=new JsonRpcServiceProxyWrapper();
            wrapper.RegisterMethod("m1", req =>
            {
                throw new Exception("Error");
            });
            var resp=wrapper.ServeAsync(new JsonRpcRequest
            {
                Method = "m1"
            }).GetAwaiter().GetResult();
            Assert.IsNotNull(resp);
            Assert.IsNotNull(resp.Error);
            Assert.AreEqual((int)ErrorCode.ServerError,resp.Error.Code);
        }

        [Test]
        public void TestProxyExecuteMethodSync()
        {
            JsonRpcServiceProxyWrapper wrapper=new JsonRpcServiceProxyWrapper();
            wrapper.RegisterMethod("m1", req => { return Task.FromResult<JsonRpcResponse>(new JsonRpcResponse
            {
                Id = Guid.NewGuid().ToString(),
                Result = "Result"
            }); });
            var resp=wrapper.Serve(new JsonRpcRequest
            {
                Method = "m1"
            });
            Assert.IsNotNull(resp);
            Assert.AreEqual("Result",resp.Result);
        }

        [Test]
        public void TestProxyExecuteMethodMethodNotFound()
        {
            JsonRpcServiceProxyWrapper wrapper=new JsonRpcServiceProxyWrapper();
            wrapper.RegisterMethod("m1", req => { return Task.FromResult<JsonRpcResponse>(new JsonRpcResponse
            {
                Id = Guid.NewGuid().ToString(),
                Result = "Result"
            }); });
            var resp=wrapper.ServeAsync(new JsonRpcRequest
            {
                Method = "m2"
            }).GetAwaiter().GetResult();
            Assert.IsNotNull(resp);
            Assert.IsNotNull(resp.Error);
            Assert.AreEqual((int)ErrorCode.MethodNotFound,resp.Error.Code);

        }
        [Test]
        public void TestProxyExecuteMethodMethodIsNull()
        {
            JsonRpcServiceProxyWrapper wrapper=new JsonRpcServiceProxyWrapper();
            wrapper.RegisterMethod("m1", req => { return Task.FromResult<JsonRpcResponse>(new JsonRpcResponse
            {
                Id = Guid.NewGuid().ToString(),
                Result = "Result"
            }); });
            var resp=wrapper.ServeAsync(new JsonRpcRequest()).GetAwaiter().GetResult();
            Assert.IsNotNull(resp);
            Assert.IsNotNull(resp.Error);
            Assert.AreEqual((int)ErrorCode.MethodNotFound,resp.Error.Code);
        }
        [Test]
        public void TestProxyExecuteMethodRequestIsNull()
        {
            JsonRpcServiceProxyWrapper wrapper=new JsonRpcServiceProxyWrapper();
            wrapper.RegisterMethod("m1", req => { return Task.FromResult<JsonRpcResponse>(new JsonRpcResponse
            {
                Id = Guid.NewGuid().ToString(),
                Result = "Result"
            }); });
            var resp=wrapper.ServeAsync(null).GetAwaiter().GetResult();
            Assert.IsNotNull(resp);
            Assert.IsNotNull(resp.Error);
            Assert.AreEqual((int)ErrorCode.InvalidRequest,resp.Error.Code);

        }


    }
}