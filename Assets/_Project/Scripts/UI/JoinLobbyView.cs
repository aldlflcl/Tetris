using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Tetris.UI
{
    public class JoinLobbyView : MenuViewBase
    {
        [SerializeField] private LobbyListItemView _lobbyListItemViewPrefab;
        [SerializeField] private Transform _lobbyItemContainer;
        [SerializeField] private Button _joinButton;

        private MenuController _menuController;
        private List<Lobby> _lobbies = new();
        private Dictionary<string, LobbyListItemView> _lobbyListItemViews = new();
        private string _selectedLobbyId = string.Empty;

        protected override void Awake()
        {
            base.Awake();
            _menuController = GetComponentInParent<MenuController>();
            _joinButton.onClick.AddListener(OnJoinButtonPressed);
        }

        public void SetLobbies(List<Lobby> lobbies)
        {
            _lobbies = lobbies;

            RefreshLobbies();
        }

        public bool IsVisible() => _canvasGroup.alpha >= 1f;
        
        public void UpdateLobbies(List<Lobby> newLobbies)
        {
            if (!DidLobbiesChange(newLobbies))
            {
                return;
            }

            Debug.Log("Updating Lobbies list");

            if (!newLobbies.Exists(lobby => _selectedLobbyId == lobby.Id))
            {
                _selectedLobbyId = string.Empty;
            }

            SetLobbies(newLobbies);
            _menuController.UpdateLobbySelection(_selectedLobbyId);
        }

        private void OnJoinButtonPressed()
        {
            if (_lobbyListItemViews.TryGetValue(_selectedLobbyId, out var selectedLobbyItemView))
            {
                _menuController.OnJoinLobbyJoinButtonPressed(selectedLobbyItemView.Lobby);
            }
        } 

        private bool DidLobbiesChange(List<Lobby> newLobbies)
        {
            if (_lobbies.Count != newLobbies.Count)
            {
                return true;
            }

            for (int i = 0; i < newLobbies.Count; i++)
            {
                if (_lobbies[i].Id != newLobbies[i].Id || _lobbies[i].Players.Count != newLobbies[i].Players.Count)
                {
                    return true;
                }
            }

            return false;
        }

        private void RefreshLobbies()
        {
            RemoveAllLobbies();
            _joinButton.interactable = false;

            foreach (var lobby in _lobbies)
            {
                AddLobby(lobby);
            }

            UpdateJoinButtonInteractable();
        }

        private void UpdateJoinButtonInteractable()
        {
            if (_lobbyListItemViews.TryGetValue(_selectedLobbyId, out var selectedLobbyItem))
            {
                _joinButton.interactable = selectedLobbyItem.Lobby.Players.Count != selectedLobbyItem.Lobby.MaxPlayers;
            }
        }

        private void AddLobby(Lobby lobby)
        {
            var lobbyListItem = Instantiate(_lobbyListItemViewPrefab, _lobbyItemContainer);
            lobbyListItem.Setup(lobby, _menuController, false);
            _lobbyListItemViews.Add(lobby.Id, lobbyListItem);
        }


        private void RemoveAllLobbies()
        {
            for (var i = _lobbyItemContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_lobbyItemContainer.GetChild(i).gameObject);
            }

            _lobbyListItemViews.Clear();
        }

        public void SetLobbySelected(string lobbyId)
        {
            ClearLobbySelection();
            _selectedLobbyId = lobbyId;
            
            if (_lobbyListItemViews.TryGetValue(_selectedLobbyId, out var lobbyListItemView))
            {
               lobbyListItemView.SetSelected(true); 
            }
            UpdateJoinButtonInteractable();
        }

        public void ClearLobbySelection()
        {
            if (_lobbyListItemViews.TryGetValue(_selectedLobbyId, out var lobbyListItemView))
            {
               lobbyListItemView.SetSelected(false); 
            }
            
            _selectedLobbyId = string.Empty;
            _joinButton.interactable = false;
        }
    }
}