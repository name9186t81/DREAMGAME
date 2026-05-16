//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.NeedAck, PackageType.NetObjectIDAssignment)]
	public sealed class NetObjectIDAssignmentPackage : IPackage
	{
		public PackageType Type => PackageType.NetObjectIDAssignment;
		public PackageFlags Flags => PackageFlags.NeedAck;
		public int DataSize => 8;

		public System.Int32 LocalID;
		public System.Int32 ServerID;

		public NetObjectIDAssignmentPackage(){}
		public NetObjectIDAssignmentPackage(System.Int32 localID, System.Int32 serverID)
		{
			LocalID = localID;
			ServerID = serverID;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			LocalID.Convert(data, offset + localOffset);
			localOffset += sizeof(Int32);
			ServerID.Convert(data, offset + localOffset);
			localOffset += sizeof(Int32);
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			LocalID = BitConverter.ToInt32(data[(offset + localOffset)..]);
			localOffset += sizeof(Int32);
			ServerID = BitConverter.ToInt32(data[(offset + localOffset)..]);
			localOffset += sizeof(Int32);
		}
	}
}
