using System.Collections.Generic;
using Tetris.Network;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tetris.Game
{
    [RequireComponent(typeof(NetcodeHooks))]
    public class ServerGameManager : MonoBehaviour
    {
        private NetcodeHooks _netcodeHooks;

        [SerializeField] private NetworkObject _gridPrefab;
        [SerializeField] private Transform[] _spawnPoint;

        private List<NetworkObject> _spawnedPlayerObject = new();
        private static ServerGameManager _instance;
        public static ServerGameManager Instance => _instance;

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
            _netcodeHooks.NetworkDespawnHook += OnNetworkDespawn;
            ServerGameController.ServerGameControllers.Clear();
        }

        private void OnDestroy()
        {
            if (_netcodeHooks)
            {
                _netcodeHooks.NetworkSpawnHook -= OnNetworkSpawn;
                _netcodeHooks.NetworkDespawnHook -= OnNetworkDespawn;
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            GUI.Box(new Rect(100, 100, 300, 600), "Test Menu");
            if (GUI.Button(new Rect(130, 140, 200, 100), "Start Game"))
            {
                StartGame();
            }

            if (GUI.Button(new Rect(130, 340, 200, 100), "Start Host"))
            {
                NetworkManager.Singleton.StartHost();
            }

            if (GUI.Button(new Rect(130, 540, 200, 100), "Start Client"))
            {
                NetworkManager.Singleton.StartClient();
            }
        }
#endif

        private void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
                return;
            }
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }

        private void OnNetworkDespawn()
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            Debug.Log("Scene Load Event Completed!");
            if (loadSceneMode != LoadSceneMode.Single)
            {
                return;
            }

            int index = 0;

            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                _spawnedPlayerObject.Add(SpawnPlayer(client.Key, _spawnPoint[index++].position));
            }

            StartGame();
        }

        private NetworkObject SpawnPlayer(ulong clientId, Vector3 spawnPosition)
        {
            Debug.Log($"Client Connect: {clientId}");
            var grid = Instantiate(_gridPrefab);
            grid.transform.position = spawnPosition;
            grid.SpawnWithOwnership(clientId);
            return grid;
        }

        private void StartGame()
        {
            LobbyManager.Instance.DeleteActiveLobby();
            foreach (var playerObject in _spawnedPlayerObject)
            {
                playerObject.GetComponent<ClientGameController>().StartGameCountdownRpc();
            }
        }

        public void FinishGame(ulong defeatedClientId)
        {
            foreach (var serverGameController in ServerGameController.ServerGameControllers.Values)
            {
                serverGameController.FinishGame(defeatedClientId != serverGameController.OwnerClientId);
            }
        }
    }
}