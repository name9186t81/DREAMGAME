//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.NeedAck, PackageType.TextMessage)]
	public sealed class TextMessagePackage : IPackage
	{
		public PackageType Type => PackageType.TextMessage;
		public PackageFlags Flags => PackageFlags.NeedAck;
		public int DataSize => VariableSize + CONSTANT_DATA_SIZE;
		private const int CONSTANT_DATA_SIZE = 3;

		public short MessageSize;
		public string Message;
		public System.Byte SpecialID;
		private int VariableSize => Encoding.Unicode.GetBytes(Message).Length;

		public TextMessagePackage(){}
		public TextMessagePackage(System.String message, System.Byte specialID)
		{
			Message = message;
			SpecialID = specialID;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			MessageSize = (short)Message.Length;
			MessageSize.Convert(data, offset + localOffset);
			localOffset += sizeof(short);
			var bytesMessage = Encoding.Unicode.GetBytes(Message);
			Array.Copy(bytesMessage, 0, data, offset + localOffset, bytesMessage.Length);
			localOffset += bytesMessage.Length;
			data[offset + localOffset] = SpecialID;
			localOffset++;
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			MessageSize = BitConverter.ToInt16(data[(offset + localOffset)..]);
			localOffset += sizeof(short);
			if(MessageSize > 0)
			{
				Message = Encoding.Unicode.GetString(data[(offset + localOffset)..]);
				localOffset += Encoding.Unicode.GetBytes(Message).Length;
			}
			SpecialID = data[offset + localOffset];
			localOffset++;
		}
	}
}
