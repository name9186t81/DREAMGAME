//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
	[Processor(PackageType.TestActorSync)]
	public sealed class TestActorSyncProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			TestActorSyncPackage package = new TestActorSyncPackage();
			package.Deserialize(data, package.GetOffset());
			if(receiver is Server server)
				SolveForServer(package, cts, sender, server);
			if(receiver is Client client)
				SolveForClient(package, cts, sender, client);
			return Task.FromResult(true);
		}

		private void SolveForServer(TestActorSyncPackage package, CancellationTokenSource cts, IPEndPoint sender, Server server)
		{			
			if (!server.IsUserConnected(sender))
			{
				server.DebugMessageWarning("Received package from unknown IP - " + sender.ToString(), ListenerBase.DebugLevel.Low);
				return;
			}

			server.TryGetUserID(sender, out byte id);
			package.ClientID = id;
			server.SendPackageNextTickToEveryoneExcept(package, sender);
		}

		private void SolveForClient(TestActorSyncPackage package, CancellationTokenSource cts, IPEndPoint sender, Client client)
		{
			NetworkManager.Instance.UpdateDisplayer(package);
		}
	}
}