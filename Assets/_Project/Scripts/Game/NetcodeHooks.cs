using System;
using Unity.Netcode;

namespace Tetris.Network
{
    public class NetcodeHooks : NetworkBehaviour
    {
        public event Action NetworkSpawnHook = delegate { };

        public event Action NetworkDespawnHook = delegate { };

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            NetworkSpawnHook.Invoke();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            NetworkDespawnHook.Invoke();
        }
    }
}