using Networking.Packages;
using System;
using System.Net;

namespace Networking
{
    public class Client : ListenerBase
    {
        private byte _id = 255;
        private bool _isMaster = false;
        private bool _inited = false;
        private IPEndPoint _server;

        public event Action OnFinishConnect;

        public Client(int workers, int listeners) : base(workers, listeners, -1)
        {
            SetListenerName("Client");
        }

        protected override bool Process(PackageType type, in ReadOnlySpan<byte> buffer, IPEndPoint point)
        {
            if (TryGetProcessorByType(type, out var processor))
            {
                try
                {
                    processor.Process(buffer, CTS, point, this);
                    return true;
                }
                catch (Exception ex)
                {
                    DebugMessageError(ex.Message, DebugLevel.Low);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool SendPackageToServer(IPackage package)
        {
            if (_server == null) return false;

            SendPackageInstantly(package, Server);
            return true;
        }

        public bool SendPackageToServerNextTick(IPackage package)
        {
            if (_server == null) return false;

            SendPackageNextTick(package, _server);
            return true;
        }

        public void Connect(IPEndPoint server)
        {
            SendPackageNextTick(new ConnectionRequestPackage(), server);
        }

        public void SyncTime()
        {
            var package = new TimeSyncPackage(RunTime);
            SendPackageInstantly(package, Server);
        }

        public void FinishSyncingTime()
        {
            OnFinishConnect?.Invoke();
        }

        public void SendTestMessage(string testMessage, IPEndPoint point)
        {
            TestPackage package = new TestPackage(testMessage);
            SendPackageInstantly(package, point);
        }

        public void SetServer(IPEndPoint point)
        {
            _server = point;
            SyncTime();
        }

        public void SetID(byte id)
        {
            _id = id;
        }

        public IPEndPoint Server => _server;
        public byte ID => _id;
        public bool IsMasterClient => _isMaster;
    }
}
