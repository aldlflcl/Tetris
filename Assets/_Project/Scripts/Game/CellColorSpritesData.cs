using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Game
{
    [CreateAssetMenu(menuName = "Tetris/Create Cell Sprites Data", fileName = "CellColor Sprites")]
    public class CellColorSpritesData : ScriptableObject
    {
        [SerializeField] private List<CellColorSprite> _cellColorSprites;
        private Dictionary<CellColor, Sprite> _cellColorSpritesMap = new();

        private void OnEnable()
        {
            foreach (var cellColorSprite in _cellColorSprites)
            {
                _cellColorSpritesMap[cellColorSprite.Color] = cellColorSprite.Sprite;
            }
        }

        public Sprite GetSpriteByColor(CellColor color) => _cellColorSpritesMap[color];

    }

    [Serializable]
    public class CellColorSprite
    {
        public CellColor Color;
        public Sprite Sprite;
    }
}