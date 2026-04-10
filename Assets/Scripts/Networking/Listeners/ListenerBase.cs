//using Networking;
//using System;
//using System.Buffers;
//using System.Collections.Concurrent;
//using System.Diagnostics;
//using System.Net;
//using System.Net.Sockets;
//using System.Threading;
//using System.Threading.Tasks;

//using Debug = UnityEngine.Debug;

//public abstract class ListenerBase
//{
//    public enum DebugLevel
//    {
//        High,
//        Medium,
//        Low,
//        None
//    }

//    protected struct ReceivedInfo
//    {
//        public byte[] Memory;
//        public int Received;
//        public IPEndPoint Sender;
//    }

//    protected class ACKInfo
//    {
//        public byte[] Memory;
//        public long Time;
//        public byte Tries;
//        public IPEndPoint Destination;

//        public ACKInfo(byte[] memory, long time, IPEndPoint destination)
//        {
//            Memory = memory;
//            Time = time;
//            Destination = destination;
//            Tries = 0;
//        }
//    }

//    private string _debugName;
//    private DebugLevel _level;
//    private Socket _listener;
//    private short _ackID;
//    private bool _isRunning;

//    private CancellationTokenSource _cts;
//    private Stopwatch _watch;
//    private Stopwatch _tickWatch;
//    private int _tickRate = 60;
//    private float _inverseTickRate = 1 / 60f;
//    private readonly SemaphoreSlim _queueSignal;

//    private readonly int _workerThreadsCount;
//    private readonly int _listenerThreadsCount;

//    private readonly ConcurrentQueue<ReceivedInfo> _receivedInfos = new ConcurrentQueue<ReceivedInfo>();
//    private readonly ConcurrentDictionary<int, ACKInfo> _pendingACKs = new ConcurrentDictionary<int, ACKInfo>();
//    private readonly ConcurrentBag<(IPackage, IPEndPoint)> _pendingTickPackages = new ConcurrentBag<(IPackage, IPEndPoint)>();

//    private int _processedPackages;
//    private int _receivedPackages;
//    public ListenerBase(int workerThreads = -1, int listenerThreads = -1, int listeningPort = -1)
//    {
//        _listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//        _listener.Bind(new IPEndPoint(IPAddress.Any, listeningPort));
//        _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
//        _listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.NoDelay, true);
//        _listener.ReceiveBufferSize = 1024 * 1024;
//        _listener.SendBufferSize = 1024 * 1024;
//        _listener.Blocking = false;
//        _listener.DontFragment = true;

//        if (listeningPort != -1)
//        {
//            _listenerThreadsCount = Math.Clamp(listeningPort, 1, Environment.ProcessorCount);
//        }
//        else
//        {
//            _listenerThreadsCount = Environment.ProcessorCount / 2;
//        }
//        if (workerThreads != -1)
//        {
//            _workerThreadsCount = Math.Clamp(workerThreads, 1, Environment.ProcessorCount);
//        }
//        else
//        {
//            _workerThreadsCount = Environment.ProcessorCount;
//        }

//        _queueSignal = new SemaphoreSlim(0);
//        _cts = new CancellationTokenSource();
//        _watch = new Stopwatch();
//        _watch.Start();
//        _tickWatch = new Stopwatch();
//        _tickWatch.Start();

//    }

//    private void StartWorkerThreads()
//    {
//        TaskFactory factory = new TaskFactory();
//        factory.
//    }

//    private async Task ListenLoop(CancellationTokenSource cts)
//    {

//    }

//    private async Task WorkLoop(CancellationToken cancellationToken)
//    {
//        const int BATCH_SIZE = 64;
//        var batch = new ReceivedInfo[BATCH_SIZE];

//        while (_isRunning && !cancellationToken.IsCancellationRequested)
//        {
//            _queueSignal.Wait(10, cancellationToken);

//            int processedCount = await ProcessBatch(batch, BATCH_SIZE, cancellationToken);

//            if (processedCount == BATCH_SIZE)
//            {
//                while (_receivedInfos.Count > 0 && !cancellationToken.IsCancellationRequested)
//                {
//                    processedCount = await ProcessBatch(batch, BATCH_SIZE, cancellationToken);
//                    if (processedCount < BATCH_SIZE) break;
//                }
//            }
//        }
//    }

//    private async Task<int> ProcessBatch(ReceivedInfo[] batch, int batchSize, CancellationToken cancellationToken)
//    {
//        int count = 0;

//        while (_receivedInfos.TryDequeue(out var package) && count < batchSize)
//        {
//            batch[count++] = package;
//        }

//        if (count == 0) return 0;

//        DebugMessage(" Processing " + count + " packages in batch", DebugLevel.Medium);
//        if (count > 2 && Environment.ProcessorCount > 1)
//        {
//            await ProcessBatchParallel(batch, count, cancellationToken);
//        }
//        else
//        {
//            await ProcessBatchSequential(batch, count, cancellationToken);
//        }

//        return count;
//    }
//    private async Task ProcessBatchParallel(ReceivedInfo[] batch, int count, CancellationToken cancellationToken)
//    {
//        var options = new ParallelOptions
//        {
//            CancellationToken = cancellationToken,
//            MaxDegreeOfParallelism = Environment.ProcessorCount
//        };

//        try
//        {
//            await Task.Run(() =>
//            {
//                Parallel.For(0, count, options, i =>
//                {
//                    var package = batch[i];
//                    try
//                    {
//                        ProcessInternal(new ReadOnlySpan<byte>(package.Memory, 0, package.Received), package.Sender);
//                    }
//                    catch (Exception ex)
//                    {
//                        DebugMessageError($" Error processing package in worker {Thread.CurrentThread.ManagedThreadId}: {ex.Message}", DebugLevel.Medium);
//                    }
//                    finally
//                    {
//                        Interlocked.Increment(ref _processedPackages);
//                        ArrayPool<byte>.Shared.Return(package.Memory);
//                    }
//                });
//            }, cancellationToken);
//        }
//        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
//        {
//            for (int i = 0; i < count; i++)
//            {
//                ArrayPool<byte>.Shared.Return(batch[i].Memory);
//            }
//            throw;
//        }
//    }

//    private async Task ProcessBatchSequential(ReceivedInfo[] batch, int count, CancellationToken cancellationToken)
//    {
//        for (int i = 0; i < count; i++)
//        {
//            cancellationToken.ThrowIfCancellationRequested();

//            var package = batch[i];
//            try
//            {
//                ProcessInternal(new ReadOnlySpan<byte>(package.Memory, 0, package.Received), package.Sender);
//            }
//            catch (Exception ex)
//            {
//                DebugMessageError($"Error processing package: {ex.Message}", DebugLevel.Medium);
//            }
//            finally
//            {
//                Interlocked.Increment(ref _processedPackages);
//                ArrayPool<byte>.Shared.Return(package.Memory);
//            }
//        }
//    }

//    private void ProcessInternal(in ReadOnlySpan<byte> memory, IPEndPoint sender)
//    {
//        var header = NetworkUtils.GetPackageType(memory);
//        DebugMessage("processing package - " + header, DebugLevel.Medium);
//        if (header == PackageType.Ack)
//        {
//            var package = new ACKPackage();
//            package.Deserialize(memory, package.GetOffset());

//            if (_pendingACKs.ContainsKey(package.ID))
//            {
//                _pendingACKs.TryRemove(package.ID, out _);
//            }
//            else
//            {
//                DebugMessage("Received unknown ACK with ID: " + package.ID, DebugLevel.Medium);
//            }
//            return;
//        }

//        if (_typeToProcessor.TryGetValue(header, out var processor))
//        {
//            DebugMessage("processor - " + processor, DebugLevel.Medium);
//            ProcessorAttribute.ProcessorType type = ProcessorAttribute.ProcessorType.Both;
//            if (_typeToProcessorAttribte.TryGetValue(header, out var attribute))
//            {
//                type = attribute.Type;
//            }

//            bool res = processor.Process(memory, _cts, sender, this).Result;
//            DebugMessage("result - " + res, DebugLevel.Medium);

//            if (_typeToFlags.TryGetValue(header, out var flags))
//            {
//                if ((flags & PackageFlags.NeedAck) != 0)
//                {
//                    DebugMessage("sending ack", DebugLevel.High);
//                    short ackID = BitConverter.ToInt16(memory.Slice(NetworkUtils.PackageHeaderSize));
//                    var ackPackage = new ACKPackage(ackID);
//                    SendAsync(SerializePackage(ackPackage), sender);
//                }
//            }
//        }
//    }

//    protected abstract bool Process(PackageType type, in ReadOnlySpan<byte> memory, IPEndPoint sender);

//    private void SendPackageInternal(byte[] data, IPEndPoint point, bool needACK)
//    {
//        if (needACK)
//        {
//            short id = BitConverter.ToInt16(data, NetworkUtils.PackageHeaderSize);
//            if (!_pendingACKs.ContainsKey(id))
//            {
//                _pendingACKs.TryAdd(id, new ACKInfo(data, _watch.ElapsedMilliseconds, point));
//            }
//        }

//        SendAsync(data, point);
//    }

//    private byte[] SerializePackage(IPackage package)
//    {
//        int realSize = package.DataSize + (package.NeedACK ? sizeof(short) : 0) + NetworkUtils.PackageHeaderSize;
//        byte[] buffer = new byte[realSize];

//        NetworkUtils.PackageTypeToByteArray(package.Type, ref buffer);
//        if (package.NeedACK)
//        {
//            var id = ACKID;
//            id.Convert(ref buffer, NetworkUtils.PackageHeaderSize);
//        }

//        package.Serialize(ref buffer, package.GetOffset());
//        return buffer;
//    }

//    public void SendPackageToEveryone(byte[] memory)
//    {
//        for (int i = 0; i < _connected.Count; ++i)
//        {
//            SendAsync(memory, _connected[i]);
//        }
//    }

//    public async void SendAsync(byte[] memory, IPEndPoint point)
//    {
//        DebugMessage("sending - " + memory.Length + " bytes to " + point, DebugLevel.High);
//        await _listener.SendToAsync(memory, SocketFlags.None, point);
//    }

//    public void DebugMessage(string message, DebugLevel level)
//    {
//        if (level >= _level)
//        {
//            Debug.Log(_debugName + ": " + message);
//        }
//    }

//    public void DebugMessageWarning(string message, DebugLevel level)
//    {
//        if (level >= _level)
//        {
//            Debug.LogWarning(_debugName + ": " + message);
//        }
//    }

//    public void DebugMessageError(string message, DebugLevel level)
//    {
//        if (level >= _level)
//        {
//            Debug.LogError(_debugName + ": " + message);
//        }
//    }

//    public void ChangeDebugLevel(DebugLevel level)
//    {
//        _level = level;
//    }

//    public void SetListenerName(string name) 
//    {
//        _debugName = name;
//    }
//}
