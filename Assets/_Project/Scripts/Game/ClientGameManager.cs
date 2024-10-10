using Tetris.Network;
using Unity.Netcode;
using UnityEngine;

namespace Tetris.Game
{
    public class ClientGameManager : MonoBehaviour
    {

        [SerializeField] private CountdownTimer _countdownTimer;

        private static ClientGameManager _instance;
        public static ClientGameManager Instance => _instance;
        private NetcodeHooks _netcodeHooks;

        public CountdownTimer CountdownTimer => _countdownTimer;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }
            
            _instance = this;
            _netcodeHooks = GetComponent<NetcodeHooks>();
            _netcodeHooks.NetworkSpawnHook += OnNetworkSpawn;
        }

        private void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                enabled = false;
                return;
            }
        }

        private void OnDestroy()
        {
            if (_netcodeHooks)
            {
                _netcodeHooks.NetworkSpawnHook -= OnNetworkSpawn;
            }
        }

    }
}