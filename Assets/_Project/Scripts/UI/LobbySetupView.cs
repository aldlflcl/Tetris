using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using WebSocketSharp;

namespace Tetris.UI
{
    public class LobbySetupView : MenuViewBase
    {
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TMP_InputField _lobbyNameInput;

        private string LobbyName => _lobbyNameInput.text;
        private MenuController _menuController;
        
        protected override void Awake()
        {
            base.Awake();
            _menuController = GetComponentInParent<MenuController>();
            _confirmButton.interactable = false;
            _lobbyNameInput.onValueChanged.AddListener(OnLobbyNameInputChanged);
            _confirmButton.onClick.AddListener(OnConfirmButtonPressed);
        }

        private void OnConfirmButtonPressed()
        {
           _menuController.OnLobbySetupConfirmButtonPressed(LobbyName); 
        }
        
        private void OnLobbyNameInputChanged(string lobbyName)
        {
            _confirmButton.interactable = !lobbyName.IsNullOrEmpty();
        }
    }
}