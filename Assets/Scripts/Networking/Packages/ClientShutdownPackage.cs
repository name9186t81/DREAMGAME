//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.None, PackageType.ClientShutdown)]
	public sealed class ClientShutdownPackage : IPackage
	{
		public PackageType Type => PackageType.ClientShutdown;
		public PackageFlags Flags => PackageFlags.None;
		public int DataSize => 0;


		public ClientShutdownPackage(){}
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
