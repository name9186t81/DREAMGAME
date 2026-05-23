//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
	[Processor(PackageType.SnapshotRequest, ProcessorAttribute.ProcessorType.Server)]
	public sealed class SnapshotRequestProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			SnapshotRequestPackage package = new SnapshotRequestPackage();
			package.Deserialize(data, package.GetOffset());
			byte id = 0;

            if (receiver is Server server && !server.TryGetUserID(sender, out id))
			{
				server.DebugMessageWarning("Received package from unknown IP - " + sender.ToString(), ListenerBase.DebugLevel.Low);
				return Task.FromResult(false);
			}

			if(package.EntityID == -1)
			{
				foreach(var pair in NetworkManager.Instance.Entities)
				{
					var entity = pair.Key;
					entity.AddSnapshotRequest(id);
				}
			}
			else
			{
				if (NetworkManager.Instance.TryGetSpawnedEnityByID(package.EntityID, out var entity))
				{
					entity.AddSnapshotRequest(id);
				}
			}
			return Task.FromResult(true);
		}
	}
}