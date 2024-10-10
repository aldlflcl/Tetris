using Tetris.UnityService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tetris.UI
{
    public class MainMenuView : MenuViewBase
    {
        [SerializeField] private TMP_InputField _playerNameInput;
        [SerializeField] private Button _playerNameUpdateButton;
        [SerializeField] private Button _createLobbyButton;
        [SerializeField] private Button _joinLobbyButton;

        [SerializeField] private TMP_InputField _profileNameInput;
        [SerializeField] private Button _signInButton;

        private MenuController _menuController;

        private string PlayerNameInput => _playerNameInput.text;

        protected override void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _menuController = GetComponentInParent<MenuController>();
            _playerNameInput.interactable = false;
            _playerNameUpdateButton.interactable = false;
            _signInButton.interactable = false;

            _playerNameInput.onValueChanged.AddListener(OnPlayerNameInputChanged);
            _playerNameUpdateButton.onClick.AddListener(OnPlayerNameUpdateButtonPressed);
            _createLobbyButton.onClick.AddListener(_menuController.OnCreateLobbyButtonPressed);
            _joinLobbyButton.onClick.AddListener(_menuController.OnMainMenuJoinLobbyButtonPressed);

            _signInButton.onClick.AddListener(OnSignInButtonPressed);
            _profileNameInput.onValueChanged.AddListener(OnProfileNameInputChanged);
        }

        public override void SetInteractable(bool isInteractable)
        {
            base.SetInteractable(isInteractable);
            _createLobbyButton.interactable = CloudSaveManager.Instance.HasName;
            _joinLobbyButton.interactable = CloudSaveManager.Instance.HasName;
            _playerNameInput.interactable = AuthenticationManager.IsInitialized;
            _playerNameUpdateButton.interactable = IsValidPlayerName(_playerNameInput.text);
            _signInButton.interactable = IsValidProfileName(_profileNameInput.text);
        }

        public void SetPlayerName(string playerName) => _playerNameInput.text = playerName;

        private void OnSignInButtonPressed()
        {
            _menuController.OnSignInButtonPressed(_profileNameInput.text);
        }

        private void OnPlayerNameUpdateButtonPressed()
        {
            _menuController.OnPlayerNameUpdateButtonPressed(PlayerNameInput);
        }

        private void OnPlayerNameInputChanged(string playerName)
        {
            _playerNameUpdateButton.interactable = IsValidPlayerName(playerName);
        }

        private void OnProfileNameInputChanged(string profileName)
        {
            _signInButton.interactable = IsValidProfileName(profileName);
        }

        private bool IsValidPlayerName(string playerName)
        {
            return !(playerName.Trim().Length < 3) &&
                   CloudSaveManager.Instance.PlayerName != playerName &&
                   AuthenticationManager.IsInitialized;
        }

        private bool IsValidProfileName(string profileName)
        {
            return !(profileName.Trim().Length < 3) &&
                   AuthenticationManager.ProfileName != profileName;
        }

        public void SetProfileName(string profileName)
        {
            _profileNameInput.text = profileName;
        }
    }
}