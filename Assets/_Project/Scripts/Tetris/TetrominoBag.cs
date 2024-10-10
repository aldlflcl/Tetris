using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetris.Game
{
    public class TetrominoBag
    {
        // 테트로미노타입이 담겨있는 리스트
        private readonly List<TetrominoType> _tetrominoBag = new();

        // 가장 앞에 있는 테트로미노타입을 꺼냄
        public TetrominoType Dequeue()
        {
            if (_tetrominoBag.Count < 1)
            {
                FillTetrominoBag();
            }

            var result = _tetrominoBag[0];
            _tetrominoBag.RemoveAt(0);
            return result;
        }

        // 가방의 특정 위치에 테트로미노타입을 반환
        public TetrominoType GetNext(int index)
        {
            while (_tetrominoBag.Count <= index)
            {
                FillTetrominoBag();
            }

            return _tetrominoBag[index];
        }

        
        // 주어진 길이만큼 가방의 테트로미노 종류들을 반환
        public List<TetrominoType> GetUpcomingTetrominoes(int length)
        {
            while (_tetrominoBag.Count < length)
            {
                FillTetrominoBag();
            }

            return _tetrominoBag.GetRange(0, length);
        }

        // 테트로미노 종류를 가방에 무작위 순서로 중복이 되지 않게 채움
        private void FillTetrominoBag()
        {
            var tetrominoTypes = Enum.GetValues(typeof(TetrominoType)).Cast<TetrominoType>();

            var randomSortedTetrominoTypes = tetrominoTypes.OrderBy(_ => Guid.NewGuid());
            foreach (var tetrominoType in randomSortedTetrominoTypes)
            {
                _tetrominoBag.Add(tetrominoType);
            }
        }
    }
}