//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.NeedAck, PackageType.Test)]
	public sealed class TestPackage : IPackage
	{
		public PackageType Type => PackageType.Test;
		public PackageFlags Flags => PackageFlags.NeedAck;
		public int DataSize => VariableSize + CONSTANT_DATA_SIZE;
		private const int CONSTANT_DATA_SIZE = 2;

		public short TestMessageSize;
		public string TestMessage;
		private int VariableSize => Encoding.Unicode.GetBytes(TestMessage).Length;

		public TestPackage(){}
		public TestPackage(System.String testMessage)
		{
			TestMessage = testMessage;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			TestMessageSize = (short)TestMessage.Length;
			TestMessageSize.Convert(data, offset + localOffset);
			localOffset += sizeof(short);
			var bytesTestMessage = Encoding.Unicode.GetBytes(TestMessage);
			Array.Copy(bytesTestMessage, 0, data, offset + localOffset, bytesTestMessage.Length);
			localOffset += bytesTestMessage.Length;
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			TestMessageSize = BitConverter.ToInt16(data[(offset + localOffset)..]);
			localOffset += sizeof(short);
			if(TestMessageSize > 0)
			{
				TestMessage = Encoding.Unicode.GetString(data[(offset + localOffset)..]);
				localOffset += Encoding.Unicode.GetBytes(TestMessage).Length;
			}
		}
	}
}
