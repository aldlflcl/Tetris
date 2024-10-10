using System;
using Tetris.GamePlay;
using UnityEngine;

namespace Tetris
{
    public class InputSender : MonoBehaviour
    {
        [SerializeField] private InputReader _inputReader;

        public event Action<Vector2Int> Move;
        public event Action<bool> Rotate;
        public event Action HardDrop;

        private void Start() => _inputReader.EnableInputActions();

        private void OnEnable()
        {
            _inputReader.Move += OnMove;
            _inputReader.Rotate += OnRotate;
            _inputReader.HardDrop += OnHardDrop;
        }

        private void OnDisable()
        {
            _inputReader.Move -= OnMove;
            _inputReader.Rotate -= OnRotate;
            _inputReader.HardDrop -= OnHardDrop;
        }

        private void OnMove(Vector2Int inputValue)
        {
            Move?.Invoke(inputValue);
        }
        
        private void OnRotate(bool isClockWise)
        {
            Rotate?.Invoke(isClockWise);
        }

        private void OnHardDrop() => HardDrop?.Invoke();

    }
}