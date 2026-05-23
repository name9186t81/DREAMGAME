//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.NeedAck, PackageType.Snapshot)]
	public sealed class SnapshotPackage : IPackage
	{
		public PackageType Type => PackageType.Snapshot;
		public PackageFlags Flags => PackageFlags.NeedAck;
		public int DataSize => VariableSize + CONSTANT_DATA_SIZE;
		private const int CONSTANT_DATA_SIZE = 7;

		public System.Int32 EntityID;
		public System.Byte Target;
		private short SnapshotDataSize;
		public System.Byte[] SnapshotData;
		private int VariableSize => (SnapshotData == null ? 0 : SnapshotData.Length * 1);

		public SnapshotPackage(){}
		public SnapshotPackage(System.Int32 entityID, System.Byte target, System.Byte[] snapshotData)
		{
			EntityID = entityID;
			Target = target;
			SnapshotData = snapshotData;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			EntityID.Convert(data, offset + localOffset);
			localOffset += sizeof(Int32);
			data[offset + localOffset] = Target;
			localOffset++;
			SnapshotDataSize = SnapshotData == null ? (short)0 : (short)SnapshotData.Length;
			SnapshotDataSize.Convert(data, offset + localOffset);
			localOffset += sizeof(short);
			if(SnapshotDataSize > 0)
			{
				int singleSizeSnapshotData = Marshal.SizeOf(SnapshotData[0]);
				for(int i = 0; i < SnapshotData.Length; ++i)
				{
					SnapshotData[i].Convert(data, offset + localOffset);
					localOffset += singleSizeSnapshotData;
				}
			}
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			EntityID = BitConverter.ToInt32(data[(offset + localOffset)..]);
			localOffset += sizeof(Int32);
			Target = data[offset + localOffset];
			localOffset++;
			SnapshotDataSize = BitConverter.ToInt16(data[(offset + localOffset)..]);
			localOffset += sizeof(short);
			if(SnapshotDataSize > 0)
			{
				var split = data[(offset + localOffset)..(offset + localOffset + SnapshotDataSize)];
				SnapshotData = split.ToArray();
			}
		}
	}
}
