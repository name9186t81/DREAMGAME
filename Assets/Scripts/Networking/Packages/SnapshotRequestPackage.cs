//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.NeedAck, PackageType.SnapshotRequest)]
	public sealed class SnapshotRequestPackage : IPackage
	{
		public PackageType Type => PackageType.SnapshotRequest;
		public PackageFlags Flags => PackageFlags.NeedAck;
		public int DataSize => 4;

		public System.Int32 EntityID;

		public SnapshotRequestPackage(){}
		public SnapshotRequestPackage(System.Int32 entityID)
		{
			EntityID = entityID;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			EntityID.Convert(data, offset + localOffset);
			localOffset += sizeof(Int32);
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			EntityID = BitConverter.ToInt32(data[(offset + localOffset)..]);
			localOffset += sizeof(Int32);
		}
	}
}
