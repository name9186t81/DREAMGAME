using Networking;
using Networking.Packages;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Debug = UnityEngine.Debug;

namespace Networking
{
    public abstract class ListenerBase
    {
        public enum DebugLevel
        {
            High,
            Medium,
            Low,
            None
        }

        protected struct ReceivedInfo
        {
            public byte[] Memory;
            public int Received;
            public IPEndPoint Sender;
        }

        protected class ACKInfo
        {
            public byte[] Memory;
            public long Time;
            public byte Tries;
            public IPEndPoint Destination;

            public ACKInfo(byte[] memory, long time, IPEndPoint destination)
            {
                Memory = memory;
                Time = time;
                Destination = destination;
                Tries = 0;
            }
        }

        private string _debugName;
        private DebugLevel _level;
        private Socket _listener;
        private short _ackID;
        private bool _isRunning = true;

        private Task _tickLoop;
        private Task[] _workers;
        private Task[] _listeners;

        private CancellationTokenSource _cts;
        private Stopwatch _watch;
        private Stopwatch _tickWatch;
        private int _tickRate = 60;
        private float _inverseTickRate = 1 / 60f;
        private readonly SemaphoreSlim _queueSignal;

        private readonly int _workerThreadsCount;
        private readonly int _listenerThreadsCount;

        private readonly static ConcurrentDictionary<PackageType, PackageFlags> _typeToFlags;
        private readonly static ConcurrentDictionary<PackageType, IPackageProcessor> _typeToProcessor;
        private readonly static ConcurrentDictionary<PackageType, ProcessorAttribute> _typeToProcessorAttribte;

        private readonly ConcurrentQueue<ReceivedInfo> _receivedInfos = new ConcurrentQueue<ReceivedInfo>();
        private readonly ConcurrentDictionary<int, ACKInfo> _pendingACKs = new ConcurrentDictionary<int, ACKInfo>();
        private readonly ConcurrentBag<(IPackage, IPEndPoint)> _pendingTickPackages = new ConcurrentBag<(IPackage, IPEndPoint)>();

        private int _processedPackages;
        private int _receivedPackages;

        private IPEndPoint _ownPoint;
        public int _ownPort;
        private List<IPEndPoint> _connected = new List<IPEndPoint>();

        private const int MTU = 1400;

        public ListenerBase(int workerThreads = -1, int listenerThreads = -1, int listeningPort = -1)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (listeningPort == -1) listeningPort = (int)UnityEngine.Mathf.Lerp(2048, 65535, UnityEngine.Random.value);
            _ownPort = listeningPort;
            _ownPoint = new IPEndPoint(IPAddress.Any, listeningPort);
            _listener.Bind(_ownPoint);
            _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.NoDelay, true);
            _listener.ReceiveBufferSize = 1024 * 1024;
            _listener.SendBufferSize = 1024 * 1024;
            _listener.Blocking = false;
            _listener.DontFragment = true;

            if (listeningPort != -1)
            {
                _listenerThreadsCount = Math.Clamp(listeningPort, 1, Environment.ProcessorCount);
            }
            else
            {
                _listenerThreadsCount = Environment.ProcessorCount / 2;
            }
            if (workerThreads != -1)
            {
                _workerThreadsCount = Math.Clamp(workerThreads, 1, Environment.ProcessorCount);
            }
            else
            {
                _workerThreadsCount = Environment.ProcessorCount;
            }

            _queueSignal = new SemaphoreSlim(0);
            _cts = new CancellationTokenSource();
            _watch = new Stopwatch();
            _watch.Start();
            _tickWatch = new Stopwatch();
            _tickWatch.Start();

            DebugMessage("Starting listening on " + _listener.LocalEndPoint, DebugLevel.Low);
            StartWorkerThreads();
            StartListenWorkers();
            _tickLoop = Task.Factory.StartNew(() => TickLoop(_cts.Token), _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        static ListenerBase()
        {
            var processorTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IPackageProcessor).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            var packagesTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IPackage).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            _typeToProcessorAttribte = new ConcurrentDictionary<PackageType, ProcessorAttribute>();
            _typeToProcessor = new ConcurrentDictionary<PackageType, IPackageProcessor>();
            _typeToFlags = new ConcurrentDictionary<PackageType, PackageFlags>();

            foreach (var processorType in processorTypes)
            {
                var atr = processorType.GetCustomAttribute<ProcessorAttribute>();
                if (atr != null)
                {
                    try
                    {
                        var instance = Activator.CreateInstance(processorType);
                        _typeToProcessor.TryAdd(atr.ProcessedType, (IPackageProcessor)instance);
                        _typeToProcessorAttribte.TryAdd(atr.ProcessedType, atr);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            foreach (var packageType in packagesTypes)
            {
                var atr = packageType.GetCustomAttribute<PackageAttribute>();
                if (atr != null)
                {
                    if (!_typeToFlags.TryAdd(atr.PackageType, atr.Flags))
                    {
                        Debug.LogError("Failed to add package of a type " + atr.PackageType);
                    }
                }
                else
                {
                    Debug.LogError("Package - " + packageType.Name + " does not have package attribute");
                }
            }

            Debug.Log("Listener: Loaded " + _typeToProcessor.Count + " processors out of " + processorTypes.Count());
            Debug.Log("Loaded " + _typeToFlags.Count + " packages out of " + packagesTypes.Count());

            foreach (var proc in _typeToProcessor)
            {
                if (!_typeToFlags.TryGetValue(proc.Key, out _))
                {
                    Debug.LogWarning("No matching package for type " + proc.Key);
                }
            }

            foreach (var proc in _typeToFlags)
            {
                if (!_typeToProcessor.TryGetValue(proc.Key, out _) && proc.Key != PackageType.Ack)
                {
                    Debug.LogWarning("No matching processor for type " + proc.Key);
                }
            }
        }

        private void StartWorkerThreads()
        {
            DebugMessage("Starting " + _workerThreadsCount + " processing workers", DebugLevel.Low);
            _workers = new Task[_workerThreadsCount];
            for (int i = 0; i < _workerThreadsCount; i++)
            {
                _workers[i] = Task.Factory.StartNew(() => WorkLoop(_cts.Token), _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        private void StartListenWorkers()
        {
            DebugMessage("Starting " + _listenerThreadsCount + " listening workers", DebugLevel.Low);
            _listeners = new Task[_listenerThreadsCount];
            for (int i = 0; i < _listenerThreadsCount; i++)
            {
                _listeners[i] = Task.Factory.StartNew(() => ListenLoop(_cts.Token), _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        private async Task TickLoop(CancellationToken cancellationToken)
        {
            const int BATCH = 64;
            (IPackage, IPEndPoint)[] packages = new (IPackage, IPEndPoint)[BATCH];

            try
            {
                while (_isRunning && !cancellationToken.IsCancellationRequested)
                {
                    var elapsed = _tickWatch.ElapsedMilliseconds;
                    float c = elapsed * 0.001f;

                    if (c > _inverseTickRate)
                    {
                        int count = 0;
                        while (_pendingTickPackages.TryTake(out var res) && count < BATCH)
                        {
                            packages[count++] = res;
                        }

                        _tickWatch.Restart();
                        if (count > 2 && Environment.ProcessorCount > 1)
                        {
                            await Task.Run(() =>
                            {
                                Parallel.For(0, count, new ParallelOptions() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = Environment.ProcessorCount }, (ind) =>
                                {
                                    var package = packages[ind];

                                    try
                                    {
                                        var mem = SerializePackage(package.Item1);
                                        SendPackageInternal(mem, package.Item2, package.Item1.NeedACK);
                                    }
                                    catch (Exception ex)
                                    {
                                        DebugMessageError("Error when sending package: " + ex.Message, DebugLevel.Low);
                                    }
                                });
                            });
                        }
                        else
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var package = packages[i];

                                try
                                {
                                    var mem = SerializePackage(package.Item1);
                                    SendPackageInternal(mem, package.Item2, package.Item1.NeedACK);
                                }
                                catch (Exception ex)
                                {
                                    DebugMessageError("Error when sending package: " + ex.Message, DebugLevel.Low);
                                }
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(10);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugMessageError("Error " + ex.Message, DebugLevel.Low);
            }
        }

        private async Task ListenLoop(CancellationToken cancellationToken)
        {
            const int BUFFER_COUNT = 32;
            byte[][] buffers = new byte[BUFFER_COUNT][];
            var tasks = new Task<SocketReceiveFromResult>[BUFFER_COUNT];

            for (int i = 0; i < BUFFER_COUNT; i++)
            {
                buffers[i] = ArrayPool<byte>.Shared.Rent(MTU);
                tasks[i] = _listener.ReceiveFromAsync(buffers[i], SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));
            }

            try
            {
                while (_isRunning && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var completed = Task.WhenAny(tasks);

                        var res = await completed.Result;
                        int ind = Array.IndexOf(tasks, completed.Result);

                        if (res.ReceivedBytes > 0)
                        {
                            DebugMessage("Received " + res.ReceivedBytes + " bytes avaible " + _listener.Available, DebugLevel.High);
                            _receivedInfos.Enqueue(new ReceivedInfo() { Memory = buffers[ind], Sender = (IPEndPoint)res.RemoteEndPoint, Received = res.ReceivedBytes });
                            _queueSignal.Release();

                            Interlocked.Increment(ref _receivedPackages);

                            buffers[ind] = ArrayPool<byte>.Shared.Rent(MTU);
                            tasks[ind] = _listener.ReceiveFromAsync(buffers[ind], SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));
                        }
                    }
                    catch (TaskCanceledException ex)
                    {

                    }
                    catch (Exception e)
                    {
                        DebugMessageError(e.Message, DebugLevel.Low);
                    }
                }
            }
            catch (TaskCanceledException ex)
            {

            }
        }

        private async Task WorkLoop(CancellationToken cancellationToken)
        {
            const int BATCH_SIZE = 64;
            var batch = new ReceivedInfo[BATCH_SIZE];

            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                _queueSignal.Wait(10, cancellationToken);

                int processedCount = await ProcessBatch(batch, BATCH_SIZE, cancellationToken);

                if (processedCount == BATCH_SIZE)
                {
                    while (_receivedInfos.Count > 0 && !cancellationToken.IsCancellationRequested)
                    {
                        processedCount = await ProcessBatch(batch, BATCH_SIZE, cancellationToken);
                        if (processedCount < BATCH_SIZE) break;
                    }
                }
            }
        }

        private async Task<int> ProcessBatch(ReceivedInfo[] batch, int batchSize, CancellationToken cancellationToken)
        {
            int count = 0;

            while (_receivedInfos.TryDequeue(out var package) && count < batchSize)
            {
                batch[count++] = package;
            }

            if (count == 0) return 0;

            DebugMessage(" Processing " + count + " packages in batch", DebugLevel.Medium);
            if (count > 2 && Environment.ProcessorCount > 1)
            {
                await ProcessBatchParallel(batch, count, cancellationToken);
            }
            else
            {
                await ProcessBatchSequential(batch, count, cancellationToken);
            }

            return count;
        }

        private async Task ProcessBatchParallel(ReceivedInfo[] batch, int count, CancellationToken cancellationToken)
        {
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            try
            {
                await Task.Run(() =>
                {
                    Parallel.For(0, count, options, i =>
                    {
                        var package = batch[i];
                        try
                        {
                            ProcessInternal(new ReadOnlySpan<byte>(package.Memory, 0, package.Received), package.Sender);
                        }
                        catch (Exception ex)
                        {
                            DebugMessageError($" Error processing package in worker {Thread.CurrentThread.ManagedThreadId}: {ex.Message}", DebugLevel.Medium);
                        }
                        finally
                        {
                            Interlocked.Increment(ref _processedPackages);
                            ArrayPool<byte>.Shared.Return(package.Memory);
                        }
                    });
                }, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                for (int i = 0; i < count; i++)
                {
                    ArrayPool<byte>.Shared.Return(batch[i].Memory);
                }
                throw;
            }
        }

        private async Task ProcessBatchSequential(ReceivedInfo[] batch, int count, CancellationToken cancellationToken)
        {
            for (int i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var package = batch[i];
                try
                {
                    ProcessInternal(new ReadOnlySpan<byte>(package.Memory, 0, package.Received), package.Sender);
                }
                catch (Exception ex)
                {
                    DebugMessageError($"Error processing package: {ex.Message}", DebugLevel.Medium);
                }
                finally
                {
                    Interlocked.Increment(ref _processedPackages);
                    ArrayPool<byte>.Shared.Return(package.Memory);
                }
            }
        }

        private void ProcessInternal(in ReadOnlySpan<byte> memory, IPEndPoint sender)
        {
            var header = NetworkUtils.GetPackageType(memory);
            DebugMessage("processing package - " + header, DebugLevel.Medium);
            if (header == PackageType.Ack)
            {
                var package = new AckPackage();
                package.Deserialize(memory, package.GetOffset());

                if (_pendingACKs.ContainsKey(package.ID))
                {
                    _pendingACKs.TryRemove(package.ID, out _);
                }
                else
                {
                    DebugMessage("Received unknown ACK with ID: " + package.ID, DebugLevel.Medium);
                }
                return;
            }

            if (_typeToProcessor.TryGetValue(header, out var processor))
            {
                DebugMessage("processor - " + processor, DebugLevel.Medium);
                ProcessorAttribute.ProcessorType type = ProcessorAttribute.ProcessorType.Both;
                if (_typeToProcessorAttribte.TryGetValue(header, out var attribute))
                {
                    type = attribute.Type;
                }

                bool res = processor.Process(memory, _cts, sender, this).Result;
                DebugMessage("result - " + res, DebugLevel.Medium);

                if (_typeToFlags.TryGetValue(header, out var flags))
                {
                    if ((flags & PackageFlags.NeedAck) != 0)
                    {
                        DebugMessage("sending ack", DebugLevel.High);
                        short ackID = BitConverter.ToInt16(memory.Slice(NetworkUtils.PackageHeaderSize));
                        var ackPackage = new AckPackage(ackID);
                        SendAsync(SerializePackage(ackPackage), sender);
                    }
                }
            }
        }

        protected abstract bool Process(PackageType type, in ReadOnlySpan<byte> memory, IPEndPoint sender);

        private void SendPackageInternal(byte[] data, IPEndPoint point, bool needACK)
        {
            if (needACK)
            {
                short id = BitConverter.ToInt16(data, NetworkUtils.PackageHeaderSize);
                if (!_pendingACKs.ContainsKey(id))
                {
                    DebugMessage("adding ACK - " + id, DebugLevel.High);
                    _pendingACKs.TryAdd(id, new ACKInfo(data, _watch.ElapsedMilliseconds, point));
                }
            }

            SendAsync(data, point);
        }

        public byte[] SerializePackage(IPackage package)
        {
            int realSize = package.DataSize + (package.NeedACK ? sizeof(short) : 0) + NetworkUtils.PackageHeaderSize;
            byte[] buffer = new byte[realSize];

            NetworkUtils.PackageTypeToByteArray(package.Type, ref buffer);
            if (package.NeedACK)
            {
                var id = ACKID;
                id.Convert(buffer, NetworkUtils.PackageHeaderSize);
            }

            package.Serialize(buffer, package.GetOffset());
            return buffer;
        }

        public void SendPackageToEveryone(byte[] memory)
        {
            for (int i = 0; i < _connected.Count; ++i)
            {
                SendAsync(memory, _connected[i]);
            }
        }

        public async void SendAsyncWithAck(byte[] memory, IPEndPoint point)
        {
            short id = BitConverter.ToInt16(memory, NetworkUtils.PackageHeaderSize);
            if (!_pendingACKs.ContainsKey(id))
            {
                DebugMessage("adding ACK - " + id, DebugLevel.High);
                _pendingACKs.TryAdd(id, new ACKInfo(memory, _watch.ElapsedMilliseconds, point));
            }

            DebugMessage("sending[WITH ACK] - " + memory.Length + " bytes to " + point, DebugLevel.High);
            await _listener.SendToAsync(memory, SocketFlags.None, point);
        }

        public async void SendAsync(byte[] memory, IPEndPoint point)
        {
            DebugMessage("sending - " + memory.Length + " bytes to " + point, DebugLevel.High);
            await _listener.SendToAsync(memory, SocketFlags.None, point);
        }

        public void DebugMessage(string message, DebugLevel level)
        {
            if (level >= _level)
            {
                Debug.Log(_debugName + ": " + message);
            }
        }

        public void DebugMessageWarning(string message, DebugLevel level)
        {
            if (level >= _level)
            {
                Debug.LogWarning(_debugName + ": " + message);
            }
        }

        public void DebugMessageError(string message, DebugLevel level)
        {
            if (level >= _level)
            {
                Debug.LogError(_debugName + ": " + message);
            }
        }

        public void ChangeDebugLevel(DebugLevel level)
        {
            _level = level;
        }

        public void SetListenerName(string name)
        {
            _debugName = name;
        }

        public bool TryGetProcessorByType(PackageType type, out IPackageProcessor processor) => _typeToProcessor.TryGetValue(type, out processor);

        public void Kill()
        {
            DebugMessage("TERMINATING LISTER", DebugLevel.None);
            _cts.Cancel();

            _isRunning = false;
        }

        public void SendPackageInstantly(IPackage package, IPEndPoint destination)
        {
            SendPackageInternal(SerializePackage(package), destination, package.NeedACK);
        }

        public void SendPackageNextTick(IPackage package, IPEndPoint destination)
        {
            _pendingTickPackages.Add((package, destination));
        }

        public int OwnPort => _ownPort;
        public IPEndPoint OwnPoint => _ownPoint;
        public CancellationTokenSource CTS => _cts;
        private short ACKID => _ackID++;
    }
}