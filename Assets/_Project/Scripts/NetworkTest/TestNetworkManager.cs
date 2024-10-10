using System;
using Unity.Netcode;
using UnityEngine;

namespace Tetris.NetworkTest
{
    public class TestNetworkManager : MonoBehaviour
    {
        private NetworkManager _networkManager;

        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!_networkManager.IsClient && !_networkManager.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
                SubmitNewPosition();
            }

            GUILayout.EndArea();
        }

        private void StartButtons()
        {
            if (GUILayout.Button("Host"))
                _networkManager.StartHost();
            if (GUILayout.Button("Client"))
                _networkManager.StartClient();
            if (GUILayout.Button("Server"))
                _networkManager.StartServer();
        }

        private void StatusLabels()
        {
            var mode = _networkManager.IsHost ? "Host" : _networkManager.IsServer ? "Server" : "Client";

            GUILayout.Label($"Transport: {_networkManager.NetworkConfig.NetworkTransport.GetType()}");
            GUILayout.Label($"Mode: {mode}");
        }

        private void SubmitNewPosition()
        {
            if (GUILayout.Button(_networkManager.IsServer ? "Move" : "Request Position Change"))
            {
                if (_networkManager.IsServer && !_networkManager.IsClient)
                {
                    foreach (ulong uid in _networkManager.ConnectedClientsIds)
                    {
                        _networkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<TestNetworkPlayer>().Move();
                    }
                }
                else
                {
                    var playerObject = _networkManager.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<TestNetworkPlayer>();
                    player.Move();
                }
            }
        }
    }
}