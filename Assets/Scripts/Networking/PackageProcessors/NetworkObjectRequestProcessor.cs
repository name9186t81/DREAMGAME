//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking
{
	[Processor(PackageType.NetworkObjectRequest, ProcessorAttribute.ProcessorType.Server)]
	public sealed class NetworkObjectRequestProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			NetworkObjectRequestPackage package = new NetworkObjectRequestPackage();
			package.Deserialize(data, package.GetOffset());

			if(package.EntityID == -1)
			{
				foreach(var pair in NetworkManager.Instance.Entities)
                {
					var locEntity = pair.Key;
                    var spawnPackage = new NetObjectSpawnPackage(locEntity.Position, locEntity.Rotation.eulerAngles, locEntity.SpawnID, locEntity.NetID, locEntity.OwnerID, locEntity.SpawnData);
                    receiver.SendPackageNextTick(spawnPackage, sender);
                }
			}

			if(NetworkManager.Instance.TryGetSpawnedEnityByID(package.EntityID, out var entity))
			{
				var spawnPackage = new NetObjectSpawnPackage(entity.Position, entity.Rotation.eulerAngles, entity.SpawnID, entity.NetID, entity.OwnerID, entity.SpawnData);
				receiver.SendPackageNextTick(spawnPackage, sender);
			}
			return Task.FromResult(true);
		}
	}
}