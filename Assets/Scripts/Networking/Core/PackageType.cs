namespace Networking
{
    public enum PackageType : byte
    {
        Unknown = 0,
        Ack,
        Combined,
        ConnectionRequest,
        ConnectionResponse,
        Test,
        Invalid
    }
}