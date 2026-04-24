//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking
{
	[Processor(PackageType.ConnectionResponse)]
	public sealed class ConnectionResponseProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			ConnectionResponsePackage package = new ConnectionResponsePackage();
			package.Deserialize(data, package.GetOffset());

			if(package.ResponseType != 0)
			{
				Debug.LogError("Connection failed with code - " +  package.ResponseType);
			}
			else
			{
				(receiver as Client).SetServer(sender);
				(receiver as Client).SetID(package.AssignedID);

				receiver.DebugMessage("Assigned ID - " + package.AssignedID, ListenerBase.DebugLevel.Low);
			}
			return Task.FromResult(true);
		}
	}
}