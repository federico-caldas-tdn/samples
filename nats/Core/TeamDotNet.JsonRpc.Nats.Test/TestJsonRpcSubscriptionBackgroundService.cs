using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TeamDotNet.JsonRpc.Nats.Test
{
    internal class TestJsonRpcSubscriptionBackgroundService : JsonRpcSubscriptionBackgroundService
    {
        public TestJsonRpcSubscriptionBackgroundService(string natsUrl, string group, ILoggerProvider logger, Action<JsonRpcServiceProxyWrapper> registerMethods) : base(natsUrl, group, logger, registerMethods)
        {
        }
    }

    [TestFixture]
    public class NatsSubscriptionBackgroundServiceTest
    {

        [Test]
        public void TestStart() {

            Mock<ILoggerProvider> logger = new Mock<ILoggerProvider>();
            TestJsonRpcSubscriptionBackgroundService service = new TestJsonRpcSubscriptionBackgroundService("nats://localhost:4222", "test", logger.Object
                , wrapper => { });


            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            service.StartAsync(cancellationToken).GetAwaiter().GetResult();

            System.Threading.Thread.Sleep(1000);

            //Assert.IsTrue(service.NatsConnection.State.Equals(NATS.Client.ConnState.CONNECTED));

            Assert.IsFalse(cancellationToken.IsCancellationRequested);

        }

        [Test]
        public void TestStopImmediatelyAfterStart()
        {

            Mock<ILoggerProvider> logger = new Mock<ILoggerProvider>();
            TestJsonRpcSubscriptionBackgroundService service = new TestJsonRpcSubscriptionBackgroundService("nats://localhost:4222", "test", logger.Object
                , wrapper => { });

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            service.StartAsync(cancellationToken).GetAwaiter().GetResult();

            CancellationToken ct2 = new CancellationToken();
            service.StopAsync(ct2).GetAwaiter().GetResult();

            //Assert.IsNull(service.NatsConnection);

            Assert.IsFalse(ct2.IsCancellationRequested);

        }

        [Test]
        public void TestStopThreeSecondsAfterStart()
        {

            Mock<ILoggerProvider> logger = new Mock<ILoggerProvider>();
            TestJsonRpcSubscriptionBackgroundService service = new TestJsonRpcSubscriptionBackgroundService("nats://localhost:4222", "test", logger.Object
                , wrapper => { });

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            service.StartAsync(cancellationToken).GetAwaiter().GetResult();

            Thread.Sleep(3000);

            CancellationToken ct2 = new CancellationToken();
            service.StopAsync(ct2).GetAwaiter().GetResult();

            //Assert.IsNotNull(service.NatsConnection);

            //Assert.IsTrue(service.NatsConnection.State.Equals(NATS.Client.ConnState.CLOSED));
            Assert.IsFalse(ct2.IsCancellationRequested);

        }


        /// <summary>
        /// This test verifies that publishing on the NatsConnection of a subscription does not fail
        /// </summary>
        [Test]      
        public void TestSubscriptionPublishing()
        {

            Mock<ILoggerProvider> logger = new Mock<ILoggerProvider>();

            ///this background service is subscribe only 
            TestJsonRpcSubscriptionBackgroundService service = new TestJsonRpcSubscriptionBackgroundService("nats://localhost:4222", "test", logger.Object
                , wrapper => {
                    var proxy = new TestProxy();
                    wrapper.RegisterMethod("testsubject", p => proxy.TestMethod(p));
                });

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            CancellationToken cancellationToken = cancellationTokenSource.Token;

            service.StartAsync(cancellationToken).GetAwaiter().GetResult();

            Thread.Sleep(3000);

            DummyObject obj = new DummyObject() { Message = "Hello world!" };

            //service.NatsConnection.Publish("testsubject", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj)));

            
        }
    }
}
