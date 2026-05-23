//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.None, PackageType.TransformSync)]
	public sealed class TransformSyncPackage : IPackage
	{
		public PackageType Type => PackageType.TransformSync;
		public PackageFlags Flags => PackageFlags.None;
		public int DataSize => 36;

		public UnityEngine.Vector3 Position;
		public UnityEngine.Vector3 Rotation;
		public System.Int32 EntityID;
		public System.Int64 TimeStamp;

		public TransformSyncPackage(){}
		public TransformSyncPackage(UnityEngine.Vector3 position, UnityEngine.Vector3 rotation, System.Int32 entityID, System.Int64 timeStamp)
		{
			Position = position;
			Rotation = rotation;
			EntityID = entityID;
			TimeStamp = timeStamp;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			Position.AddVector3ToBuffer(data, offset + localOffset);
			localOffset += sizeof(float) * 3;
			Rotation.AddVector3ToBuffer(data, offset + localOffset);
			localOffset += sizeof(float) * 3;
			EntityID.Convert(data, offset + localOffset);
			localOffset += sizeof(Int32);
			TimeStamp.Convert(data, offset + localOffset);
			localOffset += sizeof(Int64);
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			Position = NetworkUtils.GetVector3FromBuffer(data, offset + localOffset);
			localOffset += sizeof(float) * 3;
			Rotation = NetworkUtils.GetVector3FromBuffer(data, offset + localOffset);
			localOffset += sizeof(float) * 3;
			EntityID = BitConverter.ToInt32(data[(offset + localOffset)..]);
			localOffset += sizeof(Int32);
			TimeStamp = BitConverter.ToInt64(data[(offset + localOffset)..]);
			localOffset += sizeof(Int64);
		}
	}
}
