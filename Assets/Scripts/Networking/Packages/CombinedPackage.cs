//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.None, PackageType.Combined)]
	public sealed class CombinedPackage : IPackage
	{
		public PackageType Type => PackageType.Combined;
		public PackageFlags Flags => PackageFlags.None;
		public int DataSize => VariableSize + CONSTANT_DATA_SIZE;
		private const int CONSTANT_DATA_SIZE = 2;

		private short TestSize;
		public System.Double[] Test;
		private int VariableSize => Test.Length * 8;

		public CombinedPackage(){}
		public CombinedPackage(System.Double[] test)
		{
			Test = test;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			TestSize = (short)Test.Length;
			TestSize.Convert(data, offset + localOffset);
			localOffset += sizeof(short);
			if(TestSize > 0)
			{
				int singleSizeTest = Marshal.SizeOf(Test[0]);
				for(int i = 0; i < Test.Length; ++i)
				{
					BitConverter.DoubleToInt64Bits(Test[i]).Convert(data, offset + localOffset);
					localOffset += singleSizeTest;
				}
			}
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			TestSize = BitConverter.ToInt16(data[(offset + localOffset)..]);
			localOffset += sizeof(short);
			if(TestSize > 0)
			{
				var TestCast = MemoryMarshal.Cast<byte, System.Double>(data[(offset + localOffset)..(offset + localOffset + TestSize * sizeof(System.Double))]);
				Test = TestCast.ToArray();
				localOffset += TestSize * Marshal.SizeOf(typeof(System.Double));
			}
		}
	}
}
