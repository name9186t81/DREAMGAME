using System;

namespace Networking
{
    public interface IPackage
    {
        PackageType Type { get; }
        PackageFlags Flags { get; }
        int DataSize { get; }

        void Serialize(byte[] data, int offset);
        void Deserialize(ReadOnlySpan<byte> data, int offset);
    }
}