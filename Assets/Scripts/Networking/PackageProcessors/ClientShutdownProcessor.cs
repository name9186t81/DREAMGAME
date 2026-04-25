//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
	[Processor(PackageType.ClientShutdown)]
	public sealed class ClientShutdownProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			ClientShutdownPackage package = new ClientShutdownPackage();
			package.Deserialize(data, package.GetOffset());
			if(receiver is Server server)
				SolveForServer(package, cts, sender, server);
			if(receiver is Client client)
				SolveForClient(package, cts, sender, client);
			return Task.FromResult(true);
		}

		private void SolveForServer(ClientShutdownPackage package, CancellationTokenSource cts, IPEndPoint sender, Server server)
		{			
			if (!server.IsUserConnected(sender))
			{
				server.DebugMessageWarning("Received package from unknown IP - " + sender.ToString(), ListenerBase.DebugLevel.Low);
				return;
			}
			
		}

		private void SolveForClient(ClientShutdownPackage package, CancellationTokenSource cts, IPEndPoint sender, Client client)
		{			
		}
	}
}