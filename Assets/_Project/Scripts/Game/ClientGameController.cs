using System.Linq;
using Tetris.GamePlay;
using Tetris.Network;
using Tetris.UI;
using Tetris.UnityService;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Tetris.Game
{
    public class ClientGameController : NetworkBehaviour
    {
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private GameSettings _settings;
        [SerializeField] private CellColorSpritesData _cellColorSpritesData;
        [SerializeField] private float _firstInputDelay;
        [SerializeField] private float _inputInterval;
        [SerializeField] private TMP_Text _nameText;

        private ServerGameController _serverGameController;
        private ClientGridView _clientGrid;
        private ClientTetrominoNextView _nextView;
        private AttackIndicator _attackIndicator;

        private Vector2Int _moveInput;
        private bool _isPressed;
        private float _nextInputTime;
        private float _lastDropTime;

        private void Awake()
        {
            enabled = false;
            _serverGameController = GetComponent<ServerGameController>();
            _clientGrid = GetComponent<ClientGridView>();
            _nextView = GetComponentInChildren<ClientTetrominoNextView>();
            _attackIndicator = GetComponentInChildren<AttackIndicator>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsClient)
            {
                return;
            }

            enabled = true;
            _clientGrid.Initialize(_settings, _cellColorSpritesData);

            if (IsOwner)
            {
                _inputReader.Move += OnMove;
                _inputReader.Rotate += OnRotate;
                _inputReader.HardDrop += OnHardDrop;
                _inputReader.EnableInputActions();
                ClientGameManager.Instance.CountdownTimer.TimerEnd += OnTimerEnd;
                _serverGameController.SetPlayerNameRpc(CloudSaveManager.Instance.PlayerName);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                _inputReader.Move -= OnMove;
                _inputReader.Rotate -= OnRotate;
                _inputReader.HardDrop -= OnHardDrop;
                ClientGameManager.Instance.CountdownTimer.TimerEnd -= OnTimerEnd;
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void InitializeTetrominoNextViewRpc(TetrominoType[] nextTetrominoes)
        {
            _nextView.Initialize(nextTetrominoes.ToList());
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void PushBackTetrominoNextViewRpc(TetrominoType type)
        {
            _nextView.PushBack(type);
        }

        private void Update() => ApplyMoveInput();

        private void ApplyMoveInput()
        {
            if (_moveInput == Vector2Int.zero)
            {
                _isPressed = false;
                _nextInputTime = 0f;
                return;
            }

            if (Time.time < _nextInputTime)
            {
                return;
            }

            if (_isPressed)
            {
                _nextInputTime = Time.time + _inputInterval;
            }
            else
            {
                _isPressed = true;
                _nextInputTime = Time.time + _firstInputDelay;
            }

            if (_moveInput.y < 0)
            {
                _lastDropTime = Time.time;
            }

            if (_moveInput.y > 0)
            {
                _moveInput.y = 0;
            }

            _serverGameController.MoveTetrominoRpc(_moveInput);
        }

        private void OnMove(Vector2Int move) => _moveInput = move;

        private void OnRotate(bool isClockwise) => _serverGameController.RotateTetrominoRpc(isClockwise);

        private void OnHardDrop() => _serverGameController.HardDropTetrominoRpc();


        [Rpc(SendTo.ClientsAndHost)]
        public void UpdateGridRpc(CellDto[] changedCellDtos)
        {
            _clientGrid.UpdateGridCellsColor(changedCellDtos);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void StartGameCountdownRpc()
        {
            if (IsOwner)
            {
                ClientGameManager.Instance.CountdownTimer.StartTimer(3);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void UpdateAttackIndicatorRpc(int value)
        {
            _attackIndicator.SetValue(value);
        }

        private void OnTimerEnd()
        {
            _serverGameController.StartGameRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void FinishGameRpc(bool isWin)
        {
            if (IsOwner)
            {
                ModalView.Instance.ShowModal(isWin ? "Victory" : "Defeat", "Return to menu",
                    () => { TetrisMultiPlayer.Instance.ReturnMenuScene(); });
            }
        }


        [Rpc(SendTo.ClientsAndHost)]
        public void UpdatePlayerNameDisplayRpc(string playerName)
        {
            _nameText.text = playerName;
        }
    }
}