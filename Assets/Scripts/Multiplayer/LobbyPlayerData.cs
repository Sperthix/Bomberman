using System;
using Unity.Collections;
using Unity.Netcode;

namespace Multiplayer
{
    public struct LobbyPlayerData : INetworkSerializable, IEquatable<LobbyPlayerData>
    {
        public ulong ClientId;
        public bool IsReady;

        public LobbyPlayerData(ulong clientId, bool isReady = false)
        {
            ClientId = clientId;
            IsReady = isReady;
        }

        public bool Equals(LobbyPlayerData other) => ClientId == other.ClientId && IsReady == other.IsReady;
        public override bool Equals(object obj) => obj is LobbyPlayerData other && Equals(other);
        public override int GetHashCode() => System.HashCode.Combine(ClientId, IsReady);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref IsReady);
        }
    }
}