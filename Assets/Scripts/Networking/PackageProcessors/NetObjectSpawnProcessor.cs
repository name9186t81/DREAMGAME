//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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

			Debug.LogWarning("1");
			server.AddObjectOwner(serverID, id);
            Debug.LogWarning("2");
            server.SendPackageNextTickToEveryoneExcept(new NetObjectSpawnPackage(package.Position, package.Rotation, package.SpawnID, package.EntityID, package.ClientID, package.SpawnData), sender);
            Debug.LogWarning("3");
            server.SendPackageNextTick(new NetObjectIDAssignmentPackage(clientID, serverID), sender);
            Debug.LogWarning("4");
        }

		private void SolveForClient(NetObjectSpawnPackage package, CancellationTokenSource cts, IPEndPoint sender, Client client)
		{
			NetworkManager.Instance.SpawnEntity(package.SpawnID, package.EntityID, package.Position, Quaternion.Euler(package.Rotation), package.ClientID, package.DataSize > 0 ? package.SpawnData : null);
		}
	}
}