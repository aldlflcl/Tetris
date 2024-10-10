using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tetris.NetworkTest
{
    public class TestNetworkPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector2> Position = new NetworkVariable<Vector2>();

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                Move();
            }
        }

        public void Move()
        {
            SubmitPositionRequestRpc();
        }

        [Rpc(SendTo.Server)]
        private void SubmitPositionRequestRpc(RpcParams rpcParams = default)
        {
            Vector2 randomPosition = GetRandomPositionOnSquare();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }

        private static Vector2 GetRandomPositionOnSquare()
        {
            return new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
        }

        private void Update()
        {
            transform.position = Position.Value;
        }
    }
}