//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.NeedAck, PackageType.TimeSyncResponse)]
	public sealed class TimeSyncResponsePackage : IPackage
	{
		public PackageType Type => PackageType.TimeSyncResponse;
		public PackageFlags Flags => PackageFlags.NeedAck;
		public int DataSize => 16;

		public System.Int64 TimeStamp;
		public System.Int64 ServerTimeStamp;

		public TimeSyncResponsePackage(){}
		public TimeSyncResponsePackage(System.Int64 timeStamp, System.Int64 serverTimeStamp)
		{
			TimeStamp = timeStamp;
			ServerTimeStamp = serverTimeStamp;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			TimeStamp.Convert(data, offset + localOffset);
			localOffset += sizeof(Int64);
			ServerTimeStamp.Convert(data, offset + localOffset);
			localOffset += sizeof(Int64);
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			TimeStamp = BitConverter.ToInt64(data[(offset + localOffset)..]);
			localOffset += sizeof(Int64);
			ServerTimeStamp = BitConverter.ToInt64(data[(offset + localOffset)..]);
			localOffset += sizeof(Int64);
		}
	}
}
