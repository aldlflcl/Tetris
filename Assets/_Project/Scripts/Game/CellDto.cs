using Tetris.GamePlay;
using Tetris.Game;
using Unity.Netcode;

namespace Tetris.Network
{
    public struct CellDto : INetworkSerializable
    {
        public int X;
        public int Y;
        public CellColor Color;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref X);
            serializer.SerializeValue(ref Y);
            serializer.SerializeValue(ref Color);
        }
    }
}