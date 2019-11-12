using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using MyNatsClient;
using MyNatsClient.Extensions;
using Moq;

namespace TeamDotNet.JsonRpc.Nats.Test
{
    /// <summary>
    /// Testing of Json-Rpc over NATS transport
    /// WARNING: NATS server must be running on Localhost 
    /// </summary>
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class NatsJsonRpcTests
    {
        private JsonRpcSubscription _subscription = new JsonRpcSubscription("test:channel", "test::group", new EchoService(), null);
        private JsonRpcSubscription _exceptionSubscription = new JsonRpcSubscription("test:exception", "test::group", new EchoService(), null);
        [OneTimeSetUp]
        public void FixtureSetup()
        {
            _subscription.Subscribe("nats://127.0.0.1");
            _subscription.Start().GetAwaiter().GetResult();
            _exceptionSubscription.Subscribe("nats://127.0.0.1");
            _exceptionSubscription.Start().GetAwaiter().GetResult();
        }
        [OneTimeTearDown]
        public void FixtureTeardown()
        {
            _subscription.Dispose();
            _exceptionSubscription.Dispose();

        }
        [Test]
        public void TestCallMethodOverJsonRpc()
        {
            JsonRpcServer server = new JsonRpcServer("nats://127.0.0.1", 5000);
            using (server)
            {
                JsonRpcRequest request = new JsonRpcRequest
                {
                    Method = "test:channel",
                    Id = Guid.NewGuid().ToString(),
                    Parameters = new Dictionary<string, string>
                            {
                                {"data", "ping"}
                            }
                };
                var resp = server.ServeAsync(request).GetAwaiter().GetResult();
                Assert.IsNotNull(resp);
                Assert.IsNull(resp.Error);
                Assert.AreEqual(request.Id.ToString(), resp.Id);
                Assert.AreEqual("ping", resp.Result);
            }
        }
        [Test]
        public void TestTimingThousandCalls()
        {
            JsonRpcServer server = new JsonRpcServer("nats://127.0.0.1", 5000);
            using (server)
            {
                JsonRpcRequest request = new JsonRpcRequest
                {
                    Method = "test:channel",
                    Id = Guid.NewGuid().ToString(),
                    Parameters = new Dictionary<string, string>
                    {
                        {"data", "ping"}
                    }
                };
                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < 1000; i++)
                {
                    var resp = server.ServeAsync(request).GetAwaiter().GetResult();
                    Assert.IsNotNull(resp);
                    Assert.IsNull(resp.Error);
                    Assert.AreEqual(request.Id.ToString(), resp.Id);
                    Assert.AreEqual("ping", resp.Result);
                }
                sw.Stop();
                Console.WriteLine($"Elapsed Time:{sw.Elapsed}");
            }
        }

        [Test]
        public void TestExceptionInJsonRpcShouldReturnInternalError()
        {
            JsonRpcServer server = new JsonRpcServer("nats://127.0.0.1", 10000);
            using (server)
            {
                JsonRpcRequest request = new JsonRpcRequest
                {
                    Method = "test:exception",
                    Id = Guid.NewGuid().ToString(),
                    Parameters = new Dictionary<string, string>
                            {
                                {"data", "ping"}
                            }
                };
                var resp = server.ServeAsync(request).GetAwaiter().GetResult();

                Assert.IsNotNull(resp);
                Assert.AreEqual(request.Id.ToString(), resp.Id);
                Assert.IsNull(resp.Result);
                Assert.IsNotNull(resp.Error);
                Assert.AreEqual((int)ErrorCode.InternalError, resp.Error.Code);
                Assert.AreEqual("Error", resp.Error.Message);
            }
        }


        [Test]
        public void TestCallUnknownMethodCallTimeout()
        {
            JsonRpcServer server = new JsonRpcServer("nats://127.0.0.1", 5000);
            using (server)
            {
                JsonRpcRequest request = new JsonRpcRequest
                {
                    Method = "test:channel-unknown",
                    Id = Guid.NewGuid().ToString(),
                    Parameters = new Dictionary<string, string>
                    {
                        {"data", "ping"}
                    }
                };
                Stopwatch sw = Stopwatch.StartNew();
                Assert.Throws<JsonRpcException>(() => server.ServeAsync(request).GetAwaiter().GetResult());
                sw.Stop();
                Assert.IsTrue(sw.ElapsedMilliseconds >= 5000);
            }
        }

        [Test]
        public void TestDisposedSubscritionThrowsException()
        {
            JsonRpcSubscription subscription =
                new JsonRpcSubscription("test:channel", "test::group", new EchoService(), null);
            subscription.Subscribe("nats://127.0.0.1");
            subscription.Dispose();
            Assert.Throws<ObjectDisposedException>(() => subscription.Start().GetAwaiter().GetResult());

        }

        [Test]
        public void TestDisposedServiceThrowsException()
        {
            JsonRpcServer server = new JsonRpcServer("nats://127.0.0.1", 5000);
            server.Dispose();
            JsonRpcRequest request = new JsonRpcRequest
            {
                Method = "test:channel",
                Id = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, string>
                {
                    {"data", "ping"}
                }
            };
            Assert.Throws<ObjectDisposedException>(() => server.Serve(request));

        }

        [Test]
        public void TestMultipleConcurrentCallsShouldNotThrowException()
        {
            JsonRpcServer server = new JsonRpcServer("nats://127.0.0.1", 5000);
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                var task = Task.Run(() =>
                {
                    JsonRpcRequest request = new JsonRpcRequest
                    {
                        Method = "test:channel",
                        Id = Guid.NewGuid().ToString(),
                        Parameters = new Dictionary<string, string>
                        {
                            {"data", "ping"}
                        }
                    };

                    //server.ServeAsync(request).GetAwaiter().GetResult();
                });
                tasks.Add(task);
            }

            Assert.DoesNotThrow(() => { Task.WaitAll(tasks.ToArray()); });
            server.Dispose();
        }

        /// <summary>
        /// Test that errors publishing replies to NATS are logged
        /// Important - this test assumed NATS server is configured with the default max payload of 1048576 bytes
        /// </summary>
        [Test]
        public void TestNatSubscriptionShouldLogPublishingError()
        {
            var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger>();         
            JsonRpcSubscription subscription = new JsonRpcSubscription("test:largepayload", "test::group", new LargePayloadService(), mockLogger.Object);
            subscription.Subscribe("nats://127.0.0.1");
            subscription.Start().GetAwaiter().GetResult();

            JsonRpcServer server = new JsonRpcServer("nats://127.0.0.1", 5000);
            using (server)
            {
                Assert.Throws<JsonRpcException>(() =>
                {
                    var resp = server.ServeAsync(new JsonRpcRequest
                    {
                        Method = "test:largepayload",
                        Id = Guid.NewGuid().ToString(),
                        Parameters = new Dictionary<string, string>
                            {
                                {"size", "10000000"}
                            }
                    }).GetAwaiter().GetResult();
                });
            }
            
            mockLogger.Verify(m => m.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<Object>(), It.IsAny<Exception>(), It.IsAny<Func<Object, Exception, string>>()));
        }

        /// <summary>
        /// Test that error publishing to NATS does not prevent subsequent calls from succeeding
        /// Important - this test assumed NATS server is configured with the default max payload of 1048576 bytes
        /// </summary>
        [Test]        
        public void TestNatSubscriptionShouldRecoverFromPublishingError()
        {
            JsonRpcSubscription subscription = new JsonRpcSubscription("test:largepayload", "test::group", new LargePayloadService(), null);
            subscription.Subscribe("nats://127.0.0.1");
            subscription.Start().GetAwaiter().GetResult();

            JsonRpcServer server = new JsonRpcServer("nats://127.0.0.1", 5000);
            using (server)
            {
                Assert.Throws<JsonRpcException>(() =>
                {
                    var resp = server.ServeAsync(new JsonRpcRequest
                    {
                        Method = "test:largepayload",
                        Id = Guid.NewGuid().ToString(),
                        Parameters = new Dictionary<string, string>
                             {
                                {"size", "10000000"}
                            }
                    }).GetAwaiter().GetResult();
                });

                var resp2 = server.ServeAsync(new JsonRpcRequest
                {
                    Method = "test:largepayload",
                    Id = Guid.NewGuid().ToString(),
                    Parameters = new Dictionary<string, string>
                            {
                                {"size", "100"}
                            }
                }).GetAwaiter().GetResult();

                Assert.IsNotNull(resp2);
                Assert.IsNull(resp2.Error);
                Assert.AreEqual(resp2.Id.ToString(), resp2.Id);
                Assert.AreEqual(new string('A', 100), resp2.Result);
            }
        }

        [ExcludeFromCodeCoverage]
        public class EchoService : IJsonRpcServer
        {
            public JsonRpcResponse Serve(JsonRpcRequest request)
            {
                if (request.Method == "test:exception")
                    throw new Exception("Error");
                return new JsonRpcResponse
                {
                    Id = request.Id,
                    Result = request.Parameters["data"]
                };
            }

            public async Task<JsonRpcResponse> ServeAsync(JsonRpcRequest request)
            {
                return await Task.FromResult(Serve(request));
            }
        }

        [ExcludeFromCodeCoverage]
        public class LargePayloadService : IJsonRpcServer
        {
            public JsonRpcResponse Serve(JsonRpcRequest request)
            {
                int size = int.Parse(request.Parameters["size"]);
                return new JsonRpcResponse
                {
                    Id = request.Id,
                    Result = new string('A', size)
                };
            }

            public async Task<JsonRpcResponse> ServeAsync(JsonRpcRequest request)
            {
                return await Task.FromResult(Serve(request));
            }
        }
    }
}
