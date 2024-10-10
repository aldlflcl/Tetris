using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Game
{
    public class Tetris
    {
        private Grid _grid;
        private readonly TetrominoFactory _tetrominoFactory;
        private readonly TetrominoBag _tetrominoBag = new TetrominoBag();
        private Tetromino _currentTetromino;
        private bool _hasTetromino;
        private bool _isCanMove;
        private bool _isGameOver;
        private bool _isGameStarted;
        private float _nextTickTime;
        private float _gameTick;
        private int _attackAmount;

        public int PendingDamage
        {
            get => _pendingDamage;
            private set
            {
                if (_pendingDamage != value)
                {
                    _pendingDamage = value;
                    PendingDamageChanged.Invoke(value);
                }
            }
        }

        private int _pendingDamage;

        public event Action AfterGameTick = delegate { };
        public event Action OnGameOver = delegate { };
        public event Action<TetrominoType> AfterUpdateNextView = delegate { };

        public event Action<int> OnAttack = delegate { };
        public event Action<int> PendingDamageChanged = delegate { };

        public Tetris(TetrominoFactory tetrominoFactory)
        {
            _tetrominoFactory = tetrominoFactory;
        }

        public void Initialize(GameSettings settings)
        {
            _grid = new Grid();
            _grid.Initialize(settings);
            _gameTick = settings.GameTick;
        }

        public Tetromino CreateTetromino(int x, int y)
        {
            _isCanMove = true;
            _hasTetromino = true;

            var nextType = _tetrominoBag.Dequeue();
            _currentTetromino = _tetrominoFactory.CreateTetromino(nextType, _grid, x, y);

            AfterUpdateNextView.Invoke(_tetrominoBag.GetNext(3));
            return _currentTetromino;
        }

        public List<TetrominoType> GetUpcomingTetrominoes(int length) => _tetrominoBag.GetUpcomingTetrominoes(length);

        public void SubscribeToValueChanged(Action<Cell> action)
        {
            _grid.ValueChanged += action;
        }

        public void UnsubscribeToValueChanged(Action<Cell> action)
        {
            _grid.ValueChanged -= action;
        }

        public void Move(Vector2Int moveInput)
        {
            if (_isCanMove && _isGameStarted && !_isGameOver)
            {
                _currentTetromino.Move(moveInput.x, moveInput.y);
            }
        }

        public void Rotate(bool isClockwise)
        {
            if (_isCanMove)
            {
                _currentTetromino.SuperRotate(isClockwise);
            }
        }

        public bool Dropdown() => _currentTetromino.Move(0, -1);


        public bool HasTetromino() => _hasTetromino && _currentTetromino != null;

        // 현재 테트로미노를 그리드에 저장

        public void PlaceTetromino()
        {
            _grid.PlaceTetromino(_currentTetromino.Coordinates);
            _hasTetromino = false;
        }

        public void ClearLines()
        {
            var clearedLine = _grid.ClearFullLines(_currentTetromino.Coordinates);

            if (clearedLine < 1)
            {
                return;
            }

            if (clearedLine >= PendingDamage)
            {
                _attackAmount = clearedLine - PendingDamage;
                PendingDamage = 0;
            }
            else
            {
                PendingDamage -= clearedLine;
            }
        }

        public bool CheckCollision() => _grid.CheckCollision(_currentTetromino.Coordinates);

        public void HardDrop()
        {
            _currentTetromino.HardDrop();

            _isCanMove = false;
            _nextTickTime = 0f;
        }

        public void InsertGarbageBlock(int attackAmount)
        {
            _grid.InsertGarbageBlock(attackAmount);
        }


        public void Update(float currentTime)
        {
            if (!IsGamePlaying())
            {
                return;
            }

            if (!ShouldGameTick(currentTime))
            {
                return;
            }

            Tick();
        }

        public void StopGame() => _isGameOver = true;

        private bool IsGamePlaying()
        {
            if (!_isGameOver && _isGameStarted)
            {
                return true;
            }

            return false;
        }

        private void Tick()
        {
            var canDrop = Dropdown();
            if (canDrop)
            {
                return;
            }

            // 테트로미노의 현재위치를 그리드의 저장
            PlaceTetromino();

            // 완성된 줄 제거
            ClearLines();

            // 받은 공격이 있다면 쓰레기 줄 생성
            ApplyPendingDamage();

            // 공격 
            AttackEnemy();

            // 새로운 테트로미노 생성
            CreateTetromino(4, 21);

            // 게임 오버 체크
            CheckGameOver();

            AfterGameTick.Invoke();
        }

        private void ApplyPendingDamage()
        {
            if (PendingDamage < 1)
            {
                return;
            }

            InsertGarbageBlock(PendingDamage);
            PendingDamage = 0;
        }

        private void AttackEnemy()
        {
            if (_attackAmount < 1) return;

            OnAttack.Invoke(_attackAmount);
            _attackAmount = 0;
        }

        private bool ShouldGameTick(float currentTime)
        {
            if (currentTime >= _nextTickTime)
            {
                _nextTickTime = currentTime + _gameTick;
                return true;
            }

            return false;
        }

        private void CheckGameOver()
        {
            if (CheckCollision())
            {
                _isGameOver = true;
                Debug.Log("GameOver");
                OnGameOver.Invoke();
            }
        }

        public void Start()
        {
            _isGameStarted = true;
            CreateTetromino(4, 21);
        }

        public void TakeDamage(int damage) => PendingDamage += damage;
    }
}