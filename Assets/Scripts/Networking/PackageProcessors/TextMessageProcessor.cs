//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
	[Processor(PackageType.TextMessage)]
	public sealed class TextMessageProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			TextMessagePackage package = new TextMessagePackage();
			package.Deserialize(data, package.GetOffset());
			if (receiver is Server server && !server.IsUserConnected(sender))
			{
				server.DebugMessageWarning("Received package from unknown IP - " + sender.ToString(), ListenerBase.DebugLevel.Low);
				return Task.FromResult(false);
			}
			return Task.FromResult(true);
		}
	}
}