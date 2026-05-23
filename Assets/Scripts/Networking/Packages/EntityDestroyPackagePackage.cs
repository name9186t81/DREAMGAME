//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.NeedAck, PackageType.EntityDestroyPackage)]
	public sealed class EntityDestroyPackagePackage : IPackage
	{
		public PackageType Type => PackageType.EntityDestroyPackage;
		public PackageFlags Flags => PackageFlags.NeedAck;
		public int DataSize => 4;

		public System.Int32 ID;

		public EntityDestroyPackagePackage(){}
		public EntityDestroyPackagePackage(System.Int32 iD)
		{
			ID = iD;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			ID.Convert(data, offset + localOffset);
			localOffset += sizeof(Int32);
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			ID = BitConverter.ToInt32(data[(offset + localOffset)..]);
			localOffset += sizeof(Int32);
		}
	}
}
