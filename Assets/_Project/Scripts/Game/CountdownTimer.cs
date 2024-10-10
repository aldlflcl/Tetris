using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Tetris.Game
{
    public class CountdownTimer : MonoBehaviour
    {
        public event Action TimerEnd = delegate { };

        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private float _targetFontSize;
        private CanvasGroup _canvasGroup;

        private float _originalFontSize;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _originalFontSize = _timerText.fontSize;
        }

        [ContextMenu("Test")]
        public void Test()
        {
            StartTimer(3);
        }

        public void StartTimer(int time)
        {
            StartCoroutine(RunTimer(time));
        }

        private IEnumerator RunTimer(int time)
        {
            _canvasGroup.alpha = 1f;
            
            for (int currentTime = time; currentTime > 0; currentTime--)
            {
                yield return StartCoroutine(TimerRoutine(currentTime));
            }
            
            Debug.Log("Timer End");
            _canvasGroup.alpha = 0f;
            TimerEnd.Invoke();
        }

        private IEnumerator TimerRoutine(int time)
        {
            _timerText.text = $"{time}";

            float elapsedTime = 0f;
            
            while (elapsedTime < 1)
            {
                _timerText.fontSize = Mathf.Lerp(_originalFontSize, _targetFontSize, elapsedTime / 1f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _timerText.fontSize = _originalFontSize;
        }

    }
}