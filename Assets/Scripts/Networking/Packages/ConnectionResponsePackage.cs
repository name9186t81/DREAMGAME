//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.NeedAck, PackageType.ConnectionResponse)]
	public sealed class ConnectionResponsePackage : IPackage
	{
		public PackageType Type => PackageType.ConnectionResponse;
		public PackageFlags Flags => PackageFlags.NeedAck;
		public int DataSize => 2;

		public System.Byte ResponseType;
		public System.Byte AssignedID;

		public ConnectionResponsePackage(){}
		public ConnectionResponsePackage(System.Byte responseType, System.Byte assignedID)
		{
			ResponseType = responseType;
			AssignedID = assignedID;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			data[offset + localOffset] = ResponseType;
			localOffset++;
			data[offset + localOffset] = AssignedID;
			localOffset++;
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			ResponseType = data[offset + localOffset];
			localOffset++;
			AssignedID = data[offset + localOffset];
			localOffset++;
		}
	}
}
