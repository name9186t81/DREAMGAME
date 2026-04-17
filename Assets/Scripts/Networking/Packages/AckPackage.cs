//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.None, PackageType.Ack)]
	public sealed class AckPackage : IPackage
	{
		public PackageType Type => PackageType.Ack;
		public PackageFlags Flags => PackageFlags.None;
		public int DataSize => 2;

		public System.Int16 ID;

		public AckPackage(){}
		public AckPackage(System.Int16 iD)
		{
			ID = iD;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			ID.Convert(data, offset + localOffset);
			localOffset += sizeof(Int16);
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			ID = BitConverter.ToInt16(data[(offset + localOffset)..]);
			localOffset += sizeof(Int16);
		}
	}
}
