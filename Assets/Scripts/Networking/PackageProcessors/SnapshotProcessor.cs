//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
	[Processor(PackageType.Snapshot)]
	public sealed class SnapshotProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			SnapshotPackage package = new SnapshotPackage();
			package.Deserialize(data, package.GetOffset());
			if(receiver is Server server)
				SolveForServer(package, cts, sender, server);
			if(receiver is Client client)
				SolveForClient(package, cts, sender, client);
			return Task.FromResult(true);
		}

		private void SolveForServer(SnapshotPackage package, CancellationTokenSource cts, IPEndPoint sender, Server server)
		{
			if (!server.IsUserConnected(sender))
			{
				server.DebugMessageWarning("Received package from unknown IP - " + sender.ToString(), ListenerBase.DebugLevel.Low);
				return;
			}

			var target = package.Target;
			if(server.TryGetUserByID(target, out var receiver))
			{
				server.SendPackageNextTick(package, receiver);
			}
		}

		private void SolveForClient(SnapshotPackage package, CancellationTokenSource cts, IPEndPoint sender, Client client)
		{
			if(NetworkManager.Instance.TryGetSpawnedEnityByID(package.EntityID, out var entity))
			{
				entity.ApplySnapshot(package.SnapshotData, 0, package.DataSize);
			}
		}
	}
}