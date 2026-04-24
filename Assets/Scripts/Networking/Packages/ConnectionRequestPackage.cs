//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.NeedAck, PackageType.ConnectionRequest)]
	public sealed class ConnectionRequestPackage : IPackage
	{
		public PackageType Type => PackageType.ConnectionRequest;
		public PackageFlags Flags => PackageFlags.NeedAck;
		public int DataSize => 0;


		public ConnectionRequestPackage(){}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
		}
	}
}
