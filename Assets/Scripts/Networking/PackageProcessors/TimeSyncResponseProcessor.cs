//the following code was partly auto-generated
using Networking.Packages;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
	[Processor(PackageType.TimeSyncResponse, ProcessorAttribute.ProcessorType.Client)]
	public sealed class TimeSyncResponseProcessor : IPackageProcessor
    {
        private int _currentIteration;
        private readonly long[] _offsets = new long[SYNC_TIME_ITERATIONS];

        public const int SYNC_TIME_ITERATIONS = 8;

        public Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)
		{
			TimeSyncResponsePackage package = new TimeSyncResponsePackage();
			package.Deserialize(data, package.GetOffset());

            long currentTime = receiver.RunTimeRaw;
            long rtt = currentTime - package.TimeStamp;

            long serverTime = package.ServerTimeStamp + rtt / 2;
            long clientTime = package.TimeStamp;
            _offsets[_currentIteration++] = serverTime - clientTime;

            if (_currentIteration == SYNC_TIME_ITERATIONS)
            {
                _currentIteration = 0;

                long sum = 0;
                for (int i = 0; i < _offsets.Length; i++)
                {
                    sum += _offsets[i];
                }
                long averageOffset = sum / _offsets.Length;
                (receiver as Client).SetTimeOffset(averageOffset);
                (receiver as Client).FinishSyncingTime();
            }
            else
            {
                var request = new TimeSyncPackage(receiver.RunTime);
                receiver.SendPackageInstantly(request, sender);
            }

            return Task.FromResult(true);
		}
	}
}