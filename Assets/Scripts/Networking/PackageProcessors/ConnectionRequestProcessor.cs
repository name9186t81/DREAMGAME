//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
	[Processor(PackageType.ConnectionRequest)]
	public sealed class ConnectionRequestProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			ConnectionRequestPackage package = new ConnectionRequestPackage();
			package.Deserialize(data, package.GetOffset());

			(receiver as Server).RegisterUser(sender, out byte id);
            //todo implement some sort of blacklisting
            var response = new ConnectionResponsePackage(0, id);
			receiver.SendPackageNextTick(response, sender);
			return Task.FromResult(true);
		}
	}
}