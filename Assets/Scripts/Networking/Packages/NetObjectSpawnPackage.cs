//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.NeedAck, PackageType.NetObjectSpawn)]
	public sealed class NetObjectSpawnPackage : IPackage
	{
		public PackageType Type => PackageType.NetObjectSpawn;
		public PackageFlags Flags => PackageFlags.NeedAck;
		public int DataSize => VariableSize + CONSTANT_DATA_SIZE;
		private const int CONSTANT_DATA_SIZE = 35;

		public UnityEngine.Vector3 Position;
		public UnityEngine.Vector3 Rotation;
		public System.Int32 SpawnID;
		public System.Int32 EntityID;
		public System.Byte ClientID;
		private short SpawnDataSize;
		public System.Byte[] SpawnData;
		private int VariableSize => (SpawnData == null ? 0 : SpawnData.Length * 1);

		public NetObjectSpawnPackage(){}
		public NetObjectSpawnPackage(UnityEngine.Vector3 position, UnityEngine.Vector3 rotation, System.Int32 spawnID, System.Int32 entityID, System.Byte clientID, System.Byte[] spawnData)
		{
			Position = position;
			Rotation = rotation;
			SpawnID = spawnID;
			EntityID = entityID;
			ClientID = clientID;
			SpawnData = spawnData;
		}
		public void Serialize(byte[] data, int offset)
		{
            int localOffset = 0;
			Position.AddVector3ToBuffer(data, offset + localOffset);
			localOffset += sizeof(float) * 3;
			Rotation.AddVector3ToBuffer(data, offset + localOffset);
			localOffset += sizeof(float) * 3;
			SpawnID.Convert(data, offset + localOffset);
			localOffset += sizeof(Int32);
			EntityID.Convert(data, offset + localOffset);
			localOffset += sizeof(Int32);
			data[offset + localOffset] = ClientID;
			localOffset++;
			SpawnDataSize = SpawnData == null ? (short)0 : (short)SpawnData.Length;
			SpawnDataSize.Convert(data, offset + localOffset);
			localOffset += sizeof(short);
			if(SpawnDataSize > 0)
			{
				int singleSizeSpawnData = Marshal.SizeOf(SpawnData[0]);
				for(int i = 0; i < SpawnData.Length; ++i)
				{
					SpawnData[i].Convert(data, offset + localOffset);
					localOffset += singleSizeSpawnData;
				}
			}
        }

        public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			Position = NetworkUtils.GetVector3FromBuffer(data, offset + localOffset);
			localOffset += sizeof(float) * 3;
			Rotation = NetworkUtils.GetVector3FromBuffer(data, offset + localOffset);
			localOffset += sizeof(float) * 3;
			SpawnID = BitConverter.ToInt32(data[(offset + localOffset)..]);
			localOffset += sizeof(Int32);
			EntityID = BitConverter.ToInt32(data[(offset + localOffset)..]);
			localOffset += sizeof(Int32);
			ClientID = data[offset + localOffset];
			localOffset++;
			SpawnDataSize = BitConverter.ToInt16(data[(offset + localOffset)..]);
			localOffset += sizeof(short);
			if(SpawnDataSize > 0)
			{
				var split = data[(offset + localOffset)..(offset + localOffset + SpawnDataSize)];
				SpawnData = split.ToArray();
			}
		}
	}
}
