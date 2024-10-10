using System;
using Tetris.UI;
using Tetris.UnityService;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tetris.Network
{
    public class TetrisMultiPlayer : MonoBehaviour
    {
        [SerializeField] private MenuController _menuController;

        public static TetrisMultiPlayer Instance => _instance;
        private static TetrisMultiPlayer _instance;
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;

        private async void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
           _menuController.ShowMainMenu(); 
        }

        /*
        private async void Start()
        {
#if UNITY_EDITOR
            var tag = CurrentPlayer.ReadOnlyTags();
            if (tag.Length > 0)
            {
                if (tag.Contains("Host"))
                {
                    await AuthenticationManager.SignInAnonymously("Host");
                }
                else if (tag[0].StartsWith("Client"))
                {
                    await AuthenticationManager.SignInAnonymously($"{tag[0]}");
                }
            }
            else
            {
                await AuthenticationManager.SignInAnonymously("User");
            }
#else
                await AuthenticationManager.SignInAnonymously("User");
#endif
            await CloudSaveManager.Instance.LoadData();
            _menuController.ShowMainMenu();
            _isInitialized = true;
        }
        */

        public void ReturnMenuScene()
        {
            if (SceneManager.GetActiveScene().name != "Menu")
            {
                TetrisNetworkManager.Instance.Shutdown();
                SceneManager.LoadScene("Menu");
            }
        }


    }
}