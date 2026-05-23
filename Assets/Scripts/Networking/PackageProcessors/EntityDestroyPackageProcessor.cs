//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
	[Processor(PackageType.EntityDestroyPackage)]
	public sealed class EntityDestroyPackageProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			EntityDestroyPackagePackage package = new EntityDestroyPackagePackage();
			package.Deserialize(data, package.GetOffset());
			if(receiver is Server server)
				SolveForServer(package, cts, sender, server);
			if(receiver is Client client)
				SolveForClient(package, cts, sender, client);
			return Task.FromResult(true);
		}

		private void SolveForServer(EntityDestroyPackagePackage package, CancellationTokenSource cts, IPEndPoint sender, Server server)
		{
			if (!server.IsUserConnected(sender))
			{
				server.DebugMessageWarning("Received package from unknown IP - " + sender.ToString(), ListenerBase.DebugLevel.Low);
				return;
			}
			
			if(!NetworkManager.Instance.TryGetSpawnedEnityByID(package.ID, out var entity) || 
				(server.TryGetUserID(sender, out var id) && !NetworkManager.Instance.OwnsEntity(package.ID, id)))
            {
                server.DebugMessageWarning("failed to find entity with id - " + package.ID, ListenerBase.DebugLevel.Low);
				return;
            }

			server.SendPackageNextTickToEveryoneExcept(package, sender);
		}

		private void SolveForClient(EntityDestroyPackagePackage package, CancellationTokenSource cts, IPEndPoint sender, Client client)
		{
			
		}
	}
}