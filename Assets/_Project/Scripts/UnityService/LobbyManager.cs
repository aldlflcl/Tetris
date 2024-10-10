using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tetris.UnityService;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static Unity.Services.Lobbies.Models.DataObject.IndexOptions;
using static Unity.Services.Lobbies.Models.DataObject.VisibilityOptions;

namespace Tetris
{
    public class LobbyManager : MonoBehaviour
    {
        public event Action PlayerNotInLobby;
        public event Action<Lobby, bool> LobbyChanged;

        private static LobbyManager _instacne;
        public static LobbyManager Instance => _instacne;

        private const string k_HostNameKey = "HOST_NAME";
        private const string k_RelayJoinCodeKey = "RELAY_JOIN_CODE";
        private const string k_GameStartedKey = "GAME_STARTED";

        public const string k_PlayerNameKey = "PLAYER_NAME";
        public const string k_IsReadyKey = "IS_READY";
        private const float k_LobbyUpdateInterval = 1.5f;
        private const float k_HostHeartbeatInterval = 15f;


        private string _playerName = string.Empty;
        public bool IsHost { get; private set; }
        private bool _isPlayerReady = false;
        private bool _gameStarted = false;
        private float _nextLobbyUpdateTime;
        private float _nextHostHeartbeatTime;
        private Lobby _activeLobby;
        public Lobby ActiveLobby => _activeLobby;

        public List<Player> Players => _players;
        private List<Player> _players = new();
        private List<Lobby> _lobbies = new();
        public string HostId => _activeLobby.HostId;
        private string PlayerId => AuthenticationService.Instance.PlayerId;
        private ILobbyEvents _lobbyEvents;

        private void Awake()
        {
            if (_instacne != null)
            {
                Destroy(gameObject);
                return;
            }

            _instacne = this;
        }

        private void OnLobbyChanged(ILobbyChanges changes)
        {
            Debug.Log("LobbyChanged Invoke");

            if (_gameStarted)
            {
                _lobbyEvents.UnsubscribeAsync();
                OnPlayerNotInLobby();
                return;
            }

            if (changes.LobbyDeleted)
            {
                _lobbyEvents.UnsubscribeAsync();
                OnPlayerNotInLobbyWithNotify();
                return;
            }
            
            changes.ApplyToLobby(_activeLobby);
            UpdateLobby(_activeLobby);
        }

        private void OnHostChanged(string hostId)
        {
            if (hostId != PlayerId || IsHost)
            {
                return;
            }

            Debug.Log($"Host Changed!");

            // IsHost = true;
        }

        private async void Update()
        {
            try
            {
                if (_activeLobby != null && !_gameStarted)
                {
                    if (IsHost && Time.time >= _nextHostHeartbeatTime)
                    {
                        await PeriodicHostHeartbeat();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }


        public async Task<Lobby> CreateLobby(string lobbyName, string hostName, int maxPlayer = 2)
        {
            try
            {
                IsHost = true;
                _playerName = hostName;
                _gameStarted = false;
                _isPlayerReady = false;

                await DeleteActiveLobbyWithNotify();

                Debug.Log("Create Relay Allocation");
                var allocation = await TetrisNetworkManager.Instance.AllocateRelay(maxPlayer);
                var relayJoinCode = await TetrisNetworkManager.Instance.GetRelayJoinCode(allocation);
                Debug.Log("Success create Relay Allocation");

                var options = new CreateLobbyOptions();
                options.IsPrivate = false;
                options.Data = new Dictionary<string, DataObject>
                {
                    { k_HostNameKey, new DataObject(Public, hostName) },
                    { k_RelayJoinCodeKey, new DataObject(Public, relayJoinCode) },
                    { k_GameStartedKey, new DataObject(Public, false.ToString(), S1) }
                };

                options.Player = CreatePlayerData();

                Debug.Log("Create Lobby");
                var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 2, options);
                Debug.Log($"Success Create Lobby {lobby.Name}, {lobby.Id}");

                TetrisNetworkManager.Instance.StartHostWithRelayAllocation(allocation);
                Debug.Log("Success Start Host");

                _activeLobby = lobby;
                _players = lobby.Players;

                LobbyEventCallbacks callbacks = new();
                callbacks.LobbyChanged += OnLobbyChanged;

                _lobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(_activeLobby.Id, callbacks);
                Log(_activeLobby);
            }
            catch (LobbyServiceException ex)
            {
                Debug.Log(ex);
            }

            return _activeLobby;
        }

        public async Task<List<Lobby>> GetUpdatedLobbiesList()
        {
            try
            {
                var queryOptions = new QueryLobbiesOptions();
                queryOptions.Filters = new List<QueryFilter>()
                {
                    new QueryFilter(QueryFilter.FieldOptions.S1, false.ToString(), QueryFilter.OpOptions.EQ)
                };
                var lobbiesQuery = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
                // var lobbies = lobbiesQuery.Results
                //     .Where(lobby => lobby.Data[k_GameStartedKey].Value == false.ToString()).ToList();
                _lobbies = lobbiesQuery.Results;
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogException(ex);
            }

            return _lobbies;
        }

        public async Task<Lobby> JoinLobby(string lobbyId, string playerName)
        {
            try
            {
                await PrepareToJoinLobby(playerName);

                var options = new JoinLobbyByIdOptions();
                options.Player = CreatePlayerData();

                var lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);

                var relayJoinCode = lobby.Data[k_RelayJoinCodeKey].Value;

                var joinAllocation = await TetrisNetworkManager.Instance.JoinRelay(relayJoinCode);

                TetrisNetworkManager.Instance.StartClientWithRelayAllocation(joinAllocation);

                Debug.Log("Success Join Relay");

                _activeLobby = lobby;
                _players = lobby.Players;

                LobbyEventCallbacks callbacks = new();
                callbacks.LobbyChanged += OnLobbyChanged;

                _lobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(_activeLobby.Id, callbacks);
            }
            catch (LobbyServiceException ex)
                when (ex.Reason is
                          LobbyExceptionReason.LobbyNotFound
                          or LobbyExceptionReason.LobbyFull)
            {
                _activeLobby = null;
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return _activeLobby;
        }

        private async Task PeriodicHostHeartbeat()
        {
            try
            {
                _nextHostHeartbeatTime = Time.time + k_HostHeartbeatInterval;

                await LobbyService.Instance.SendHeartbeatPingAsync(_activeLobby.Id);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogException(ex);
            }
        }

        private async Task PeriodicUpdateLobby()
        {
            try
            {
                _nextLobbyUpdateTime = Time.time + k_LobbyUpdateInterval;
                var updatedLobby = await LobbyService.Instance.GetLobbyAsync(_activeLobby.Id);
                UpdateLobby(updatedLobby);
            }
            catch (LobbyServiceException ex)
                when (ex.Reason is LobbyExceptionReason.LobbyNotFound or LobbyExceptionReason.Forbidden)
            {
                OnPlayerNotInLobbyWithNotify();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private async Task PrepareToJoinLobby(string playerName)
        {
            IsHost = false;
            _playerName = playerName;
            _gameStarted = false;
            _isPlayerReady = false;

            if (_activeLobby != null)
            {
                await LeaveJoinedLobby();
            }
        }

        public async Task LeaveJoinedLobby()
        {
            try
            {
                await RemovePlayer(PlayerId);
                await _lobbyEvents.UnsubscribeAsync();
                OnPlayerNotInLobbyWithNotify();
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogException(ex);
            }
        }

        public async Task OnStartedGame()
        {
            if (IsHost)
            {
                _gameStarted = true;
                
                var updateLobbyOptions = new UpdateLobbyOptions();
                updateLobbyOptions.Data = new Dictionary<string, DataObject>()
                {
                    { k_GameStartedKey, new DataObject(Public, true.ToString(), S1) }
                };
                await LobbyService.Instance.UpdateLobbyAsync(_activeLobby.Id, updateLobbyOptions);
            }
            else
            {
                _activeLobby = null;
            }
        }

        private async Task RemovePlayer(string playerId)
        {
            try
            {
                if (_activeLobby != null)
                {
                    await LobbyService.Instance.RemovePlayerAsync(_activeLobby.Id, playerId);
                }
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogException(ex);
            }
        }

        private Player CreatePlayerData()
        {
            var player = new Player();
            player.Data = CreatePlayerDictionary();
            return player;
        }

        private Dictionary<string, PlayerDataObject> CreatePlayerDictionary()
        {
            return new Dictionary<string, PlayerDataObject>
            {
                { k_PlayerNameKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, _playerName) },
                {
                    k_IsReadyKey,
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, _isPlayerReady.ToString())
                }
            };
        }

        public async Task DeleteActiveLobbyWithNotify()
        {
            try
            {
                if (_activeLobby != null && _activeLobby.HostId == PlayerId)
                {
                    await _lobbyEvents.UnsubscribeAsync();
                    await LobbyService.Instance.DeleteLobbyAsync(_activeLobby.Id);
                    Debug.Log($"Delete Lobby: {_activeLobby.Id}");
                    OnPlayerNotInLobbyWithNotify();
                }
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogException(ex);
            }
        }

        public async Task DeleteActiveLobby()
        {
            try
            {
                if (_activeLobby != null && _activeLobby.HostId == PlayerId)
                {
                    await LobbyService.Instance.DeleteLobbyAsync(_activeLobby.Id);
                    Debug.Log($"Delete Lobby: {_activeLobby.Id}");
                }
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogException(ex);
            }
        }


        public static void Log(Lobby lobby)
        {
            if (lobby == null)
            {
                return;
            }

            var lobbyData = lobby.Data.Select(data => $"{data.Key} is {data.Value}");
            var lobbyDataStr = string.Join(", ", lobbyData);
            Debug.Log($"Lobby Named:{lobby.Name}, " +
                      $"Players:{lobby.Players.Count}/{lobby.MaxPlayers}, " +
                      $"IsPrivate:{lobby.IsPrivate}, " +
                      $"IsLocked:{lobby.IsLocked}, " +
                      $"LobbyCode:{lobby.LobbyCode}, " +
                      $"Id:{lobby.Id}, " +
                      $"Created:{lobby.Created}, " +
                      $"HostId:{lobby.HostId}, " +
                      $"EnvironmentId:{lobby.EnvironmentId}, " +
                      $"Upid:{lobby.Upid}, " +
                      $"Lobby.Data:{lobbyDataStr}");
        }

        private void OnPlayerNotInLobby()
        {
            if (_activeLobby != null)
            {
                _activeLobby = null;
                IsHost = false;
            }
        }

        private void OnPlayerNotInLobbyWithNotify()
        {
            if (_activeLobby != null)
            {
                _activeLobby = null;
                IsHost = false;
                PlayerNotInLobby?.Invoke();
            }
        }

        public async Task ToggleReadyState()
        {
            try
            {
                if (_activeLobby == null)
                {
                    return;
                }

                _isPlayerReady = !_isPlayerReady;

                var lobbyId = _activeLobby.Id;

                var options = new UpdatePlayerOptions();
                options.Data = CreatePlayerDictionary();

                var updatedLobby = await LobbyService.Instance.UpdatePlayerAsync(lobbyId, PlayerId, options);
                // UpdateLobby(updatedLobby);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogException(ex);
            }
        }

        private void UpdateLobby(Lobby updatedLobby)
        {
            if (updatedLobby.Players.Exists(player => player.Id == PlayerId))
            {
                var isGameReady = IsGameReady(updatedLobby);
                LobbyChanged?.Invoke(updatedLobby, isGameReady);
            }
            else
            {
                OnPlayerNotInLobbyWithNotify();
            }
        }

        private bool IsGameReady(Lobby lobby)
        {
            if (lobby.Players.Count <= 1)
            {
                return false;
            }

            foreach (var player in lobby.Players)
            {
                if (player.Id == HostId)
                    continue;

                var isReady = bool.Parse(player.Data[k_IsReadyKey].Value);
                if (!isReady)
                {
                    return false;
                }
            }

            return true;
        }

        private bool DidPlayerChange(List<Player> oldPlayers, List<Player> newPlayers)
        {
            if (oldPlayers.Count != newPlayers.Count)
            {
                return true;
            }

            for (int i = 0; i < newPlayers.Count; i++)
            {
                if (oldPlayers[i].Id != newPlayers[i].Id ||
                    oldPlayers[i].Data[k_IsReadyKey].Value != newPlayers[i].Data[k_IsReadyKey].Value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}