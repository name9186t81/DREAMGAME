using System;

namespace Networking
{
    public sealed class PackageAttribute : Attribute
    {
        public PackageFlags Flags;
        public PackageType Type;

        public PackageAttribute(PackageFlags flags, PackageType type)
        {
            Flags = flags;
            Type = type;
        }
    }
}