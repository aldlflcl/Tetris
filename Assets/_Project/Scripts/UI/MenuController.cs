using System;
using Tetris.UnityService;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tetris.UI
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private MenuSceneView _sceneView;

        private string _selectedLobbyId = string.Empty;
        private float _nextUpdateLobbiesTime;
        private const float k_updateLobbiesListInterval = 2f;
        public string PlayerName => CloudSaveManager.Instance.PlayerName;

        #region Unity Lifecycle

        private void Start()
        {
            LobbyManager.Instance.LobbyChanged += OnLobbyChanged;
            LobbyManager.Instance.PlayerNotInLobby += OnPlayerNotInLobby;
            if (AuthenticationManager.IsInitialized)
                ShowMainMenu();
        }

        private async void Update()
        {
            if (Time.time >= _nextUpdateLobbiesTime)
            {
                _nextUpdateLobbiesTime = float.MaxValue;

                if (!_sceneView.IsJoinLobbyViewVisible())
                {
                    return;
                }

                var lobbies = await LobbyManager.Instance.GetUpdatedLobbiesList();

                if (!_sceneView.IsJoinLobbyViewVisible())
                {
                    return;
                }

                _sceneView.UpdateLobbies(lobbies);
                _nextUpdateLobbiesTime = Time.time + k_updateLobbiesListInterval;
            }
        }

        private void OnDestroy()
        {
            LobbyManager.Instance.LobbyChanged -= OnLobbyChanged;
            LobbyManager.Instance.PlayerNotInLobby -= OnPlayerNotInLobby;
        }

        #endregion


        public async void OnPlayerNameUpdateButtonPressed(string newPlayerName)
        {
            try
            {
                _sceneView.SetInteractable(false);

                await CloudSaveManager.Instance.SetPlayerName(newPlayerName);
                Debug.Log($"Update PlayerName: {newPlayerName}");
            }
            catch (Exception ex)
            {
                ModalView.Instance.ShowModal($"Something wrong ({ex.GetType().Name})", ex.Message);
                Debug.LogException(ex);
            }
            finally
            {
                _sceneView.SetInteractable(true);
            }
        }

        public async void OnMainMenuJoinLobbyButtonPressed()
        {
            try
            {
                ClearLobbySelection();

                _sceneView.SetInteractable(false);
                var lobbies = await LobbyManager.Instance.GetUpdatedLobbiesList();
                _sceneView.SetInteractable(true);
                _sceneView.SetLobbies(lobbies);
                _sceneView.ShowJoinLobbyView();

                _nextUpdateLobbiesTime = Time.time + k_updateLobbiesListInterval;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void OnCreateLobbyButtonPressed()
        {
            _sceneView.ShowLobbySetupView();
        }

        public async void OnLobbySetupConfirmButtonPressed(string lobbyName)
        {
            if (lobbyName.Trim().Length < 3)
            {
                return;
            }

            try
            {
                _sceneView.SetInteractable(false);
                var lobby = await LobbyManager.Instance.CreateLobby(lobbyName, PlayerName);
                _sceneView.SetInteractable(true);
                JoinLobby(lobby);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public async void OnReadyButtonPressed()
        {
            try
            {
                _sceneView.SetInteractable(false);
                _sceneView.ToggleReadyState(AuthenticationService.Instance.PlayerId);

                await LobbyManager.Instance.ToggleReadyState();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                _sceneView.SetInteractable(true);
            }
        }

        public void JoinLobby(Lobby joinedLobby)
        {
            Debug.Log($"JoinLobby {joinedLobby.Name}");
            _sceneView.InitializeLobbyView(joinedLobby);
            _sceneView.ShowLobbyView();
            _sceneView.SetLobbyPlayers(joinedLobby.Players);
        }

        public void ShowMainMenu()
        {
            TetrisNetworkManager.Instance.Shutdown();
            _sceneView.SetPlayerName(PlayerName);
            _sceneView.SetProfileName(AuthenticationManager.ProfileName);
            _sceneView.ShowMainMenuView();
        }

        public void UpdateLobbySelection(string selectedLobbyId)
        {
            if (string.IsNullOrEmpty(selectedLobbyId))
            {
                ClearLobbySelection();
            }
            else
            {
                _sceneView.SetLobbySelected(selectedLobbyId);
            }

            _selectedLobbyId = selectedLobbyId;
        }

        public void ClearLobbySelection()
        {
            _selectedLobbyId = string.Empty;
            _sceneView.ClearLobbySelection();
        }

        public void OnShowMainMenuButtonPressed()
        {
            ShowMainMenu();
        }

        public void OnLobbyListItemSelectButtonPressed(LobbyListItemView lobbyListItemView)
        {
            var lobbyId = lobbyListItemView.Lobby.Id;
            if (_selectedLobbyId == lobbyId)
            {
                ClearLobbySelection();
            }
            else
            {
                UpdateLobbySelection(lobbyId);
            }
        }

        public async void OnJoinLobbyJoinButtonPressed(Lobby lobby)
        {
            try
            {
                _sceneView.SetInteractable(false);
                var joinLobby = await LobbyManager.Instance.JoinLobby(lobby.Id, PlayerName);
                if (joinLobby == null)
                {
                    ModalView.Instance.ShowModal("Can't Find Lobby",
                        $"Lobby is not exist LobbyName: {lobby.Name} LobbyId: ({lobby.Id})");
                }
                else
                {
                    JoinLobby(joinLobby);
                }
            }
            catch (LobbyServiceException ex) when (ex.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                ModalView.Instance.ShowModal("Can't Find Lobby",
                    $"Lobby is not exist LobbyName: {lobby.Name} LobbyId: ({lobby.Id})");
            }
            catch (LobbyServiceException ex) when (ex.Reason == LobbyExceptionReason.LobbyFull)
            {
                ModalView.Instance.ShowModal("Lobby is full",
                    $"Lobby is Full LobbyName: {lobby.Name} LobbyId: ({lobby.Id})");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                _sceneView.SetInteractable(true);
            }
        }

        private void OnLobbyChanged(Lobby updatedLobby, bool isGameReady)
        {
            _sceneView.SetLobbyPlayers(updatedLobby.Players);
            if (LobbyManager.Instance.IsHost)
            {
                _sceneView.SetGameReady(isGameReady);
            }
        }

        public async void OnStartButtonPressed()
        {
            await LobbyManager.Instance.OnStartedGame();
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }

        public async void OnLeaveButtonPressed()
        {
            _sceneView.SetInteractable(false);
            if (LobbyManager.Instance.IsHost)
            {
                await LobbyManager.Instance.DeleteActiveLobbyWithNotify();
            }
            else
            {
                await LobbyManager.Instance.LeaveJoinedLobby();
            }
        }

        private void OnPlayerNotInLobby()
        {
            _sceneView.SetInteractable(true);
            _sceneView.ShowMainMenuView();
        }

        public async void OnSignInButtonPressed(string profileName)
        {
            try
            {
                _sceneView.SetInteractable(false);
                await AuthenticationManager.SignInAnonymously(profileName);
                await CloudSaveManager.Instance.LoadData();
                ShowMainMenu();
            }
            catch (Exception ex)
            {
                ModalView.Instance.ShowModal($"Something wrong ({ex.GetType().Name})", ex.Message);
                Debug.LogException(ex);
            }
            finally
            {
                _sceneView.SetInteractable(true);
            }
        }
    }
}