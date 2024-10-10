using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Tetris.UI
{
    public class MenuSceneView : MonoBehaviour
    {
        [SerializeField] private MainMenuView _mainMenuView;
        [SerializeField] private LobbySetupView _lobbySetupView;
        [SerializeField] private LobbyRoomView _lobbyRoomView;
        [SerializeField] private JoinLobbyView _joinLobbyView;
        
        private MenuViewBase _currentView;

        private void ShowView(MenuViewBase view)
        {
            HideCurrentView();
            view.Show();
            _currentView = view;
            SetInteractable(true);
        }

        private void HideCurrentView()
        {
            if (_currentView != null)
            {
                _currentView.Hide();
            }
        }

        public void SetInteractable(bool isInteractable) => _currentView?.SetInteractable(isInteractable);

        public void ShowMainMenuView() => ShowView(_mainMenuView);

        public void ShowLobbySetupView() => ShowView(_lobbySetupView);

        public void SetPlayerName(string playerName) => _mainMenuView.SetPlayerName(playerName);

        public void SetProfileName(string profileName) => _mainMenuView.SetProfileName(profileName);

        public void ShowLobbyView() => ShowView(_lobbyRoomView);

        public void ToggleReadyState(string playerId) => _lobbyRoomView.TogglePlayerReadyState(playerId);

        public void InitializeLobbyView(Lobby lobby)
        {
            _lobbyRoomView.Setup(lobby);
        }

        public void SetLobbyPlayers(List<Player> players)
        {
            _lobbyRoomView.SetPlayers(players);
        }

        public void SetGameReady(bool isGameReady)
        {
            _lobbyRoomView.SetGameReady(isGameReady);
        }

        public void SetLobbySelected(string selectedLobbyId)
        {
            _joinLobbyView.SetLobbySelected(selectedLobbyId);
        }

        public void ClearLobbySelection()
        {
            _joinLobbyView.ClearLobbySelection();
        }

        public bool IsJoinLobbyViewVisible() => _joinLobbyView.IsVisible();

        public void UpdateLobbies(List<Lobby> lobbies) => _joinLobbyView.UpdateLobbies(lobbies);

        public void SetLobbies(List<Lobby> lobbies) => _joinLobbyView.SetLobbies(lobbies);

        public void ShowJoinLobbyView() => ShowView(_joinLobbyView);
    }

}