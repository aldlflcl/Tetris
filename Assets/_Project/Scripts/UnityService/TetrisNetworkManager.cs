using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Tetris.UnityService
{
    public class TetrisNetworkManager : MonoBehaviour
    {
        public static TetrisNetworkManager Instance => _instance;
        private static TetrisNetworkManager _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        public void StartClientWithRelayAllocation(JoinAllocation allocation, string connectionType = "dtls")
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(new RelayServerData(allocation, connectionType));
            NetworkManager.Singleton.StartClient();
        }

        public void StartHostWithRelayAllocation(Allocation allocation, string connectionType = "dtls")
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(new RelayServerData(allocation, connectionType));
            NetworkManager.Singleton.StartHost();
        }

        public void Shutdown()
        {
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        public async Task<Allocation> AllocateRelay(int maxPlayers)
        {
            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
                return allocation;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"Failed to allocate relay: {e.Message}");
                return default;
            }
        }

        public async Task<string> GetRelayJoinCode(Allocation allocation)
        {
            try
            {
                string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                return relayJoinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"Failed to get relay join code: {e.Message}");
                return default;
            }
        }

        public async Task<JoinAllocation> JoinRelay(string relayJoinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
                return joinAllocation;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"Failed to join relay: {e.Message}");
                return default;
            }
        }
    }
}