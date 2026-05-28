//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
	[Processor(PackageType.EntityEvent)]
	public sealed class EntityEventProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			EntityEventPackage package = new EntityEventPackage();
			package.Deserialize(data, package.GetOffset());
			if(receiver is Server server)
				SolveForServer(package, cts, sender, server);
			if(receiver is Client client)
				SolveForClient(package, cts, sender, client);
			return Task.FromResult(true);
		}

		private void SolveForServer(EntityEventPackage package, CancellationTokenSource cts, IPEndPoint sender, Server server)
		{
			if (!server.IsUserConnected(sender))
			{
				server.DebugMessageWarning("Received package from unknown IP - " + sender.ToString(), ListenerBase.DebugLevel.Low);
				return;
			}

			server.SendPackageNextTickToEveryoneExcept(package, sender);
		}

		private void SolveForClient(EntityEventPackage package, CancellationTokenSource cts, IPEndPoint sender, Client client)
		{
			if(NetworkManager.Instance.TryGetSpawnedEnityByID(package.EntityID, out var entity))
			{
				entity.ApplyPackage(package);
			}
			else
			{
				client.DebugMessageWarning($"Cannot find entity for event({package.EventID}), entity id - {package.EntityID}, dataSize - {package.DataSize}", ListenerBase.DebugLevel.Low);
			}
		}
	}
}