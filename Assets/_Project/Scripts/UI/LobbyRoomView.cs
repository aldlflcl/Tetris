using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Tetris.UI
{
    public class LobbyRoomView : MenuViewBase
    {
        [SerializeField] private PlayerIconView _playerIconViewPrefab;
        [SerializeField] private TMP_Text _lobbyNameText;
        [SerializeField] private Transform _playerIconContainer;
        [SerializeField] private Button _readyButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _leaveButton;    

        private MenuController _menuController;
        private List<PlayerIconView> _playerIcons = new();
        private bool _isReady;

        protected override void Awake()
        {
            base.Awake();
            _menuController = GetComponentInParent<MenuController>();
            _readyButton.onClick.AddListener(_menuController.OnReadyButtonPressed);
            _cancelButton.onClick.AddListener(_menuController.OnReadyButtonPressed);
            _startButton.onClick.AddListener(_menuController.OnStartButtonPressed);
            _leaveButton.onClick.AddListener(_menuController.OnLeaveButtonPressed);
        }

        private void OnDestroy()
        {
            _readyButton.onClick.RemoveListener(_menuController.OnReadyButtonPressed);
            _cancelButton.onClick.RemoveListener(_menuController.OnReadyButtonPressed);
            _startButton.onClick.RemoveListener(_menuController.OnStartButtonPressed);
            _leaveButton.onClick.RemoveListener(_menuController.OnLeaveButtonPressed);
        }

        public override void SetInteractable(bool isInteractable)
        {
            base.SetInteractable(isInteractable);
            if (isInteractable && !LobbyManager.Instance.IsHost)
            {
                _readyButton.gameObject.SetActive(!_isReady);
                _cancelButton.gameObject.SetActive(_isReady);
            }
        }

        public void SetPlayers(List<Player> players)
        {
            RemoveAllPlayers();

            foreach (var player in players)
            {
                AddPlayer(player);
            }
        }

        public void TogglePlayerReadyState(string playerId)
        {
            foreach (var playerIcon in _playerIcons)
            {
                if (playerIcon.PlayerId == playerId)
                {
                    _isReady = playerIcon.ToggleReadyState();
                }
            }
        }

        private void AddPlayer(Player player)
        {
            var playerIcon = Instantiate(_playerIconViewPrefab, _playerIconContainer);
            var isHost = LobbyManager.Instance.HostId == player.Id;

            playerIcon.Setup(
                player.Id,
                player.Data[LobbyManager.k_PlayerNameKey].Value,
                isHost ? new Color(0.31f, 0.52f, 0.78f) : new Color(0.7f, 0.27f, 0.43f),
                isHost ? new Color(0.38f, 0.65f, 1f) : new Color(1f, 0.26f, 0.55f),
                isHost,
                bool.Parse(player.Data[LobbyManager.k_IsReadyKey].Value)
            );
            _playerIcons.Add(playerIcon);
        }

        private void RemoveAllPlayers()
        {
            foreach (var playerIcon in _playerIcons)
            {
                Destroy(playerIcon.gameObject);
            }

            _playerIcons.Clear();
        }

        public void Setup(Lobby lobby)
        {
            _isReady = false;
            _lobbyNameText.text = lobby.Name;
            _startButton.gameObject.SetActive(LobbyManager.Instance.IsHost);
            _readyButton.gameObject.SetActive(!LobbyManager.Instance.IsHost);
        }

        public void SetGameReady(bool isGameReady)
        {
            _startButton.interactable = isGameReady;
        }
    }
}