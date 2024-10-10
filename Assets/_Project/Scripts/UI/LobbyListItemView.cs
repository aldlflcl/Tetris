using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Tetris.UI
{
    public class LobbyListItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _playerCountText;
        [SerializeField] private GameObject _selectedIndicator;
        [SerializeField] private Button _selectButton;

        private MenuController _menuController;

        public Lobby Lobby { get; private set; }
        public int LobbyIndex { get; private set; }

        private void Awake()
        {
            _selectButton.onClick.AddListener(OnSelectButtonPressed);
        }

        public void Setup(Lobby lobby, MenuController menuController, bool isSelected)
        {
            Lobby = lobby;
            _menuController = menuController;
            _titleText.text = lobby.Name;
            _playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
            SetSelected(isSelected);
        }

        private void OnSelectButtonPressed()
        {
            _menuController.OnLobbyListItemSelectButtonPressed(this);
        }

        public void SetSelected(bool selected)
        {
            _selectedIndicator.SetActive(selected);
        }
    }
}