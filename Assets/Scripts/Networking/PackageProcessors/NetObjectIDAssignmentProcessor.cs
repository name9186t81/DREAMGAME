//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
	[Processor(PackageType.NetObjectIDAssignment, ProcessorAttribute.ProcessorType.Client)]
	public sealed class NetObjectIDAssignmentProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			NetObjectIDAssignmentPackage package = new NetObjectIDAssignmentPackage();
			package.Deserialize(data, package.GetOffset());

			NetworkManager.Instance.AssignID(package.LocalID, package.ServerID);
			return Task.FromResult(true);
		}
	}
}