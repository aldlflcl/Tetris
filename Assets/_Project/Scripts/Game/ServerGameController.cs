using System.Collections.Generic;
using Tetris.Network;
using Unity.Netcode;
using UnityEngine;

namespace Tetris.Game
{
    public class ServerGameController : NetworkBehaviour
    {
        public static Dictionary<ulong, ServerGameController> ServerGameControllers => _serverGameControllers;
        private static Dictionary<ulong, ServerGameController> _serverGameControllers = new();

        [SerializeField] private GameSettings _settings;
        [SerializeField] private TetrominoFactory _tetrominoFactory;

        private List<CellDto> _changedCellBuffer = new();

        private ClientGameController _clientGameController;
        private Tetris _tetris;

        private string _name = string.Empty;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                enabled = false;
                return;
            }

            Initialize();
        }

        public override void OnNetworkDespawn()
        {
            if (_tetris != null)
            {
                _tetris.UnsubscribeToValueChanged(OnCellChanged);
                _tetris.AfterUpdateNextView -= OnAfterUpdateNextView;
                _tetris.PendingDamageChanged -= OnPendingDamageChanged;
                _tetris.OnAttack -= OnAttack;
                _tetris.OnGameOver -= OnGameOver;
            }
        }

        private void Initialize()
        {
            Debug.Log($"Initialize ServerGameController ClientId:{OwnerClientId}");
            _serverGameControllers.Add(OwnerClientId, this);
            _clientGameController = GetComponent<ClientGameController>();
            _tetris = new Tetris(_tetrominoFactory);
            _tetris.Initialize(_settings);
            _tetris.SubscribeToValueChanged(OnCellChanged);
            _tetris.AfterUpdateNextView += OnAfterUpdateNextView;
            _tetris.PendingDamageChanged += OnPendingDamageChanged;
            _tetris.OnAttack += OnAttack;
            _tetris.OnGameOver += OnGameOver;
        }

        private void OnGameOver()
        {
            ServerGameManager.Instance.FinishGame(OwnerClientId);
        }

        [Rpc(SendTo.Server)]
        public void StartGameRpc()
        {
            Debug.Log("Start Game");
            _clientGameController.InitializeTetrominoNextViewRpc(_tetris.GetUpcomingTetrominoes(4).ToArray());
            _tetris.Start();
        }

        private void Update()
        {
            if (_tetris != null)
            {
                _tetris.Update(Time.time);
                ApplyCellChanged();
            }
        }

        private void OnPendingDamageChanged(int pendingDamage) =>
            _clientGameController.UpdateAttackIndicatorRpc(pendingDamage);

        private void OnAttack(int damage)
        {
            foreach (var serverGameController in ServerGameControllers.Values)
            {
                if (serverGameController == this)
                {
                    continue;
                }

                serverGameController.TakeDamage(damage);
            }
        }

        private void TakeDamage(int damage)
        {
            Debug.Log($"{NetworkManager.Singleton.LocalClientId} TakeDamage {damage}");
            _tetris.TakeDamage(damage);
        }

        private void OnAfterUpdateNextView(TetrominoType nextType) =>
            _clientGameController.PushBackTetrominoNextViewRpc(nextType);

        private void OnCellChanged(Cell cell)
        {
            var cellDto = new CellDto
            {
                Color = cell.Color,
                X = cell.X,
                Y = cell.Y
            };
            _changedCellBuffer.Add(cellDto);
        }

        private void ApplyCellChanged()
        {
            if (_changedCellBuffer.Count < 1)
            {
                return;
            }

            _clientGameController.UpdateGridRpc(_changedCellBuffer.ToArray());
            _changedCellBuffer.Clear();
        }

        [Rpc(SendTo.Server)]
        public void MoveTetrominoRpc(Vector2Int moveInput) => _tetris.Move(moveInput);

        [Rpc(SendTo.Server)]
        public void RotateTetrominoRpc(bool isClockwise) => _tetris.Rotate(isClockwise);

        [Rpc(SendTo.Server)]
        public void HardDropTetrominoRpc() => _tetris.HardDrop();

        [Rpc(SendTo.Server)]
        public void SetPlayerNameRpc(string playerName)
        {
            _clientGameController.UpdatePlayerNameDisplayRpc(playerName);
        }

        public void FinishGame(bool isWin)
        {
            _tetris.StopGame();
            _clientGameController.FinishGameRpc(isWin);
        }

    }
}