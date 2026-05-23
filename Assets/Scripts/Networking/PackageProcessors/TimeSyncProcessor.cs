//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
	[Processor(PackageType.TimeSync, ProcessorAttribute.ProcessorType.Server)]
	public sealed class TimeSyncProcessor : IPackageProcessor
	{
		public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			TimeSyncPackage package = new TimeSyncPackage();
			package.Deserialize(data, package.GetOffset());

			TimeSyncResponsePackage response = new TimeSyncResponsePackage(package.TimeStamp, receiver.RunTime);
			receiver.SendPackageInstantly(response, sender);
			return Task.FromResult(true);
		}
	}
}