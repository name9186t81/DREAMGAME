using System;

namespace Networking
{
    [Flags]
    public enum PackageFlags
    {
        None = 0,
        NeedAck = 1 << 0,
        Compress = 1 << 1,
        CompressHigh = 1 << 2,
    }
}