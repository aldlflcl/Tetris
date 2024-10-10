using System.Collections;
using Tetris.Extensions;
using Tetris.Game;
using UnityEngine;

namespace Tetris.GamePlay
{
    public class CellView : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private CellColorSpritesData _spritesData;
    
        private void Awake()
        {
            _renderer = this.GetOrAddComponent<SpriteRenderer>();
        }
    
        public void Initialize(Transform parent, Vector2 position, CellColorSpritesData spritesData)
        {
            transform.SetParent(parent);
            transform.localPosition = position;
            _spritesData = spritesData;
        }
    
        public void ChangeSprite(CellColor color)
        {
            var sprite = _spritesData.GetSpriteByColor(color);
            _renderer.sprite = sprite;
            // StartCoroutine(CO_ChangeSprite(sprite));
        }
    
        private IEnumerator CO_ChangeSprite(Sprite sprite)
        {
            yield return null;
    
            _renderer.sprite = sprite;
        }
    }
}