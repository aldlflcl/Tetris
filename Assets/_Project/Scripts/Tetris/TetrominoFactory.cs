using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Game
{
    /// <summary>
    /// 테트로미노 객체를 생성
    /// </summary>
    [CreateAssetMenu(menuName = "Tetris/Create Tetromino Factory", fileName = "Tetromino Factory")]
    public class TetrominoFactory : ScriptableObject
    {
        [SerializeField] private List<TetrominoData> _tetrominoList;
        private readonly Dictionary<TetrominoType, TetrominoData> _tetrominoDataMap = new();

        private void OnEnable()
        {
            _tetrominoDataMap.Clear();
            foreach (var tetrominoData in _tetrominoList)
            {
                _tetrominoDataMap.Add(tetrominoData.Type, tetrominoData);
            }
        }

        // 테트로미노 객체 생성 후 반환
        public Tetromino CreateTetromino(TetrominoType type, Grid grid, int xOffset, int yOffset)
        {
            if (!_tetrominoDataMap.TryGetValue(type, out var tetrominoData))
            {
                throw new KeyNotFoundException($"Key: {type} not found in TetrominoData");
            }

            return new Tetromino(tetrominoData, xOffset, yOffset, grid);
        }

        public TetrominoData GetTetrominoData(TetrominoType type)
        {
            if (!_tetrominoDataMap.TryGetValue(type, out var tetrominoData))
            {
                throw new KeyNotFoundException($"Key: {type} not found in TetrominoData");
            }

            return tetrominoData;
        }
    }
}