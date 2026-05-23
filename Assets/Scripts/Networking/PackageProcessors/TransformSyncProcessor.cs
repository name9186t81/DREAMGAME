//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
	[Processor(PackageType.TransformSync)]
	public sealed class TransformSyncProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			TransformSyncPackage package = new TransformSyncPackage();
			package.Deserialize(data, package.GetOffset());
			if(receiver is Server server)
				SolveForServer(package, cts, sender, server);
			if(receiver is Client client)
				SolveForClient(package, cts, sender, client);
			return Task.FromResult(true);
		}

		private void SolveForServer(TransformSyncPackage package, CancellationTokenSource cts, IPEndPoint sender, Server server)
		{
			if (!server.IsUserConnected(sender))
			{
				server.DebugMessageWarning("Received package from unknown IP - " + sender.ToString(), ListenerBase.DebugLevel.Low);
				return;
			}

			server.SendPackageNextTickToEveryoneExcept(package, sender);
		}

		private void SolveForClient(TransformSyncPackage package, CancellationTokenSource cts, IPEndPoint sender, Client client)
		{
			if(!NetworkManager.Instance.ApplyPackageToEntity(package.EntityID, package))
			{
				client.DebugMessageError("Failed to change transform for entity with id - " + package.EntityID, ListenerBase.DebugLevel.Medium);
			}
		}
	}
}
