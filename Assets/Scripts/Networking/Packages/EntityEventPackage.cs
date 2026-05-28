//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.NeedAck, PackageType.EntityEvent)]
	public sealed class EntityEventPackage : IPackage
	{
		public PackageType Type => PackageType.EntityEvent;
		public PackageFlags Flags => PackageFlags.NeedAck;
		public int DataSize => VariableSize + CONSTANT_DATA_SIZE;
		private const int CONSTANT_DATA_SIZE = 15;

		public System.Int32 EntityID;
		public System.Byte EventID;
		public System.Int64 TimeStamp;
		private short EventDataSize;
		public System.Byte[] EventData;
		private int VariableSize => (EventData == null ? 0 : EventData.Length * 1);

		public EntityEventPackage(){}
		public EntityEventPackage(System.Int32 entityID, System.Byte eventID, System.Int64 timeStamp, System.Byte[] eventData)
		{
			EntityID = entityID;
			EventID = eventID;
			TimeStamp = timeStamp;
			EventData = eventData;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			EntityID.Convert(data, offset + localOffset);
			localOffset += sizeof(Int32);
			data[offset + localOffset] = EventID;
			localOffset++;
			TimeStamp.Convert(data, offset + localOffset);
			localOffset += sizeof(Int64);
			EventDataSize = EventData == null ? (short)0 : (short)EventData.Length;
			EventDataSize.Convert(data, offset + localOffset);
			localOffset += sizeof(short);
			if(EventDataSize > 0)
			{
				int singleSizeEventData = Marshal.SizeOf(EventData[0]);
				for(int i = 0; i < EventData.Length; ++i)
				{
					EventData[i].Convert(data, offset + localOffset);
					localOffset += singleSizeEventData;
				}
			}
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			EntityID = BitConverter.ToInt32(data[(offset + localOffset)..]);
			localOffset += sizeof(Int32);
			EventID = data[offset + localOffset];
			localOffset++;
			TimeStamp = BitConverter.ToInt64(data[(offset + localOffset)..]);
			localOffset += sizeof(Int64);
			EventDataSize = BitConverter.ToInt16(data[(offset + localOffset)..]);
			localOffset += sizeof(short);
			if(EventDataSize > 0)
			{
				var split = data[(offset + localOffset)..(offset + localOffset + EventDataSize)];
				EventData = split.ToArray();
			}
		}
	}
}
