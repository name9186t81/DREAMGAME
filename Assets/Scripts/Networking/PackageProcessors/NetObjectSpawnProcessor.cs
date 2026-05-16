//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
	[Processor(PackageType.NetObjectSpawn)]
	public sealed class NetObjectSpawnProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			NetObjectSpawnPackage package = new NetObjectSpawnPackage();
			package.Deserialize(data, package.GetOffset());
			if(receiver is Server server)
				SolveForServer(package, cts, sender, server);
			if(receiver is Client client)
				SolveForClient(package, cts, sender, client);
			return Task.FromResult(true);
		}

		private void SolveForServer(NetObjectSpawnPackage package, CancellationTokenSource cts, IPEndPoint sender, Server server)
		{
			if (!server.IsUserConnected(sender))
			{
				server.DebugMessageWarning("Received package from unknown IP - " + sender.ToString(), ListenerBase.DebugLevel.Low);
				return;
			}

			int serverID = server.NetworkObjectID;
			int clientID = package.EntityID;

			package.EntityID = serverID;
			server.TryGetUserID(sender, out byte id);
			package.ClientID = id;

			server.AddObjectOwner(serverID, id);
			server.SendPackageNextTickToEveryoneExcept(package, sender);
			server.SendPackageNextTick(new NetObjectIDAssignmentPackage(clientID, serverID), sender);
		}

		private void SolveForClient(NetObjectSpawnPackage package, CancellationTokenSource cts, IPEndPoint sender, Client client)
		{
			
		}
	}
}