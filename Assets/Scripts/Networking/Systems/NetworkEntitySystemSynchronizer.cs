using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Networking
{
    public static class NetworkEntitySystemSynchronizer
    {
        private class Comparer : IComparer<INetworkEntitySystem>
        {
            public int Compare(INetworkEntitySystem x, INetworkEntitySystem y)
            {
                return Math.Sign(x.GetType().Name[0] - y.GetType().Name[0]);
            }
        }

        private static readonly ConcurrentDictionary<Type, INetworkEntitySystem> _typeToSystem;
        private readonly static ConcurrentDictionary<INetworkEntitySystem, int> _systemToID;
        private static readonly ConcurrentDictionary<int, INetworkEntitySystem> _idToSystem;

        private static ConcurrentDictionary<int, (byte[], int, int)> _awaitingSnapshots;

        public static byte SystemHeaderSizeInBytes { get; private set; }

        static NetworkEntitySystemSynchronizer()
        {
            _typeToSystem = new ConcurrentDictionary<Type, INetworkEntitySystem>();
            _systemToID = new ConcurrentDictionary<INetworkEntitySystem, int>();
            _idToSystem = new ConcurrentDictionary<int, INetworkEntitySystem>();
            _awaitingSnapshots = new ConcurrentDictionary<int, (byte[], int, int)>();

            List<INetworkEntitySystem> unsorted = new List<INetworkEntitySystem>();
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(INetworkEntitySystem).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            foreach (var type in types)
            {
                object instance = null;
                try
                {
                    instance = Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Failed to create " + type.Name + " system");
                }

                if (instance != null)
                {
                    unsorted.Add((INetworkEntitySystem)instance);
                    _typeToSystem.TryAdd(type, (INetworkEntitySystem)instance);
                }
            }

            unsorted.Sort(new Comparer());
            SystemHeaderSizeInBytes = (unsorted.Count < 256) ? (byte)1 : (byte)2;

            for (int i = 0; i < unsorted.Count; i++)
            {
                _systemToID.TryAdd(unsorted[i], i);
                _idToSystem.TryAdd(i, unsorted[i]);
            }
        }

        public static bool TryGetSnapshot(int networkObjectID, out (byte[] data, int offset, int size) snapshot)
        {
            return _awaitingSnapshots.TryRemove(networkObjectID, out snapshot);
        }

        public static bool TryToApplySnapshot(int networkObjectID, byte[] data, int startOffset, int size)
        {
            if (!NetworkManager.Instance.TryGetSpawnedEnityByID(networkObjectID, out var networkObject))
            {
                Debug.Log("Added snapshot for " + networkObjectID);
                _awaitingSnapshots.TryAdd(networkObjectID, (data, startOffset, size)); //cache snapshot in case the network object wasnt created yet
                return true;
            }

            networkObject.ApplySnapshot(data, startOffset, size);
            return true;
        }

        public static bool TryGetSnapshot(int networkObjectID, byte source)
        {
            if (!NetworkManager.Instance.TryGetSpawnedEnityByID(networkObjectID, out var networkObject))
            {
                return false;
            }

            networkObject.AddSnapshotRequest(source);
            return true;
        }

        public static bool TryGetSystem(int id, out INetworkEntitySystem system)
        {
            return _idToSystem.TryGetValue(id, out system);
        }

        public static bool TryGetSystemID<T>(out int val) where T : INetworkEntitySystem
        {
            if (_typeToSystem.TryGetValue(typeof(T), out var system)) return _systemToID.TryGetValue(system, out val);
            val = -1;
            return false;
        }

        public static bool TryGetSystemID(INetworkEntitySystem system, out int val)
        {
            return _systemToID.TryGetValue(system, out val);
        }
    }
}
