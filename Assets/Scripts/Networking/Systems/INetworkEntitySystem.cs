namespace Networking
{
    public interface INetworkEntitySystem
    {
        public enum HeaderSize
        {
            Byte = 1,
            Short = 2
        }

        /// <summary>
        /// The size of the header for storing length of data.
        /// </summary>
        HeaderSize MaxSize { get; }
        /// <summary>
        /// Used to get the size of data in bytes for concrete network object.
        /// </summary>
        /// <param name="networkBehaviour"></param>
        /// <returns></returns>
        int GetSizeFor(NetworkEntity entity);
        bool TryTakeShapshot(NetworkEntity entity, int offset, byte[] buffer);
        bool TryProcessShapshot(NetworkEntity entity, int offset, int length, byte[] buffer);
    }
}
