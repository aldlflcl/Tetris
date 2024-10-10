using UnityEngine;
using UnityEngine.UI;

namespace Tetris.Game
{
    public class AttackIndicator : MonoBehaviour
    {
        [SerializeField] private Image _attackIndicatorBar;
        [SerializeField] private float _speed;

        [SerializeField] private int _currentValue;
        private const float k_MaxAmount = 20f;

        public void SetValue(int value)
        {
            _currentValue = value;
        }

        private void Update()
        {
            _attackIndicatorBar.fillAmount = Mathf.MoveTowards(_attackIndicatorBar.fillAmount, (_currentValue / k_MaxAmount), _speed * Time.deltaTime);
        }
    }
}