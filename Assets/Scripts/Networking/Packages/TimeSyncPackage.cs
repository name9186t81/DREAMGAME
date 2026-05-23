//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.NeedAck, PackageType.TimeSync)]
	public sealed class TimeSyncPackage : IPackage
	{
		public PackageType Type => PackageType.TimeSync;
		public PackageFlags Flags => PackageFlags.NeedAck;
		public int DataSize => 8;

		public System.Int64 TimeStamp;

		public TimeSyncPackage(){}
		public TimeSyncPackage(System.Int64 timeStamp)
		{
			TimeStamp = timeStamp;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			TimeStamp.Convert(data, offset + localOffset);
			localOffset += sizeof(Int64);
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			TimeStamp = BitConverter.ToInt64(data[(offset + localOffset)..]);
			localOffset += sizeof(Int64);
		}
	}
}
