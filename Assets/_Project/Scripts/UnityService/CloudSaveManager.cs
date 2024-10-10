using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;

namespace Tetris.UnityService
{
    public class CloudSaveManager : MonoBehaviour
    {
        private const string k_PlayerNameKey = "PLAYER_NAME";
        public string PlayerName => _playerName;
        private string _playerName = string.Empty;

        public static CloudSaveManager Instance => _instance;
        private static CloudSaveManager _instance;
        public bool HasName => _playerName != string.Empty;
        

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }


        public async Task LoadData()
        {
            try
            {
                var savedData =
                    await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { k_PlayerNameKey });

                if (savedData.TryGetValue(k_PlayerNameKey, out var playerName))
                {
                    _playerName = playerName.Value.GetAsString();
                    Debug.Log($"Successfully Load PlayerName: {_playerName}");
                }
                else
                {
                    _playerName = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public async Task SetPlayerName(string playerName)
        {
            try
            {
                var data = new Dictionary<string, object>
                {
                    [k_PlayerNameKey] = playerName
                };

                await SaveUpdatedData(data);
                _playerName = playerName;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public async Task SaveUpdatedData(Dictionary<string, object> data)
        {
            try
            {
                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}