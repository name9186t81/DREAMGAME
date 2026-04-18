using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
    [Processor(PackageType.Test)]
    internal class TestMessageProcessor : IPackageProcessor
    {
        public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
        {
            TestPackage package = new TestPackage();
            package.Deserialize(data, package.GetOffset());
            receiver.DebugMessage("Received message - " + package.TestMessage, ListenerBase.DebugLevel.None);
            return Task.FromResult(true);
        }
    }
}
