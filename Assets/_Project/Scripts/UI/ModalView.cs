using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tetris.UI
{
    public class ModalView : MonoBehaviour
    {
        private static ModalView _instance;
        public static ModalView Instance => _instance;

        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Button _button;

        private CanvasGroup _canvasGroup;
        private event Action ConfirmPressed;

        private string Title
        {
            get => _titleText.text;
            set => _titleText.text = value;
        }

        private string Description
        {
            get => _descriptionText.text;
            set => _descriptionText.text = value;
        }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _canvasGroup = GetComponent<CanvasGroup>();
            DontDestroyOnLoad(gameObject);
            _button.onClick.AddListener(Hide);
            Hide();
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(Hide);
        }

        public void ShowModal(string title, string description, Action onConfirmPressed = null)
        {
            Title = title;
            Description = description;
            ConfirmPressed = onConfirmPressed;
            Show();
        }

        private void Show()
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;
        }

        private void Hide()
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;
            ConfirmPressed?.Invoke();
            ConfirmPressed = null;
        }
    }
}