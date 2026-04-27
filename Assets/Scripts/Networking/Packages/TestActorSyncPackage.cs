//the following code was auto-generated.
#pragma warning disable IDE

using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Networking.Packages
{
	[PackageAttribute(PackageFlags.None, PackageType.TestActorSync)]
	public sealed class TestActorSyncPackage : IPackage
	{
		public PackageType Type => PackageType.TestActorSync;
		public PackageFlags Flags => PackageFlags.None;
		public int DataSize => 26;

		public UnityEngine.Vector3 Position;
		public UnityEngine.Vector3 Rotation;
		public System.Byte LocalFlags;
		public System.Byte ClientID;

		public TestActorSyncPackage(){}
		public TestActorSyncPackage(UnityEngine.Vector3 position, UnityEngine.Vector3 rotation, System.Byte localFlags, System.Byte clientID)
		{
			Position = position;
			Rotation = rotation;
			LocalFlags = localFlags;
			ClientID = clientID;
		}
		public void Serialize(byte[] data, int offset)
		{
			int localOffset = 0;
			Position.AddVector3ToBuffer(data, offset + localOffset);
			localOffset += sizeof(float) * 3;
			Rotation.AddVector3ToBuffer(data, offset + localOffset);
			localOffset += sizeof(float) * 3;
			data[offset + localOffset] = LocalFlags;
			localOffset++;
			data[offset + localOffset] = ClientID;
			localOffset++;
		}
		
		public void Deserialize(ReadOnlySpan<byte> data, int offset)
		{
			int localOffset = 0;
			Position = NetworkUtils.GetVector3FromBuffer(data, offset + localOffset);
			localOffset += sizeof(float) * 3;
            Rotation = NetworkUtils.GetVector3FromBuffer(data, offset + localOffset);
			localOffset += sizeof(float) * 3;
            LocalFlags = data[offset + localOffset];
			localOffset++;
            ClientID = data[offset + localOffset];
			localOffset++;
		}
	}
}
