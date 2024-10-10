using UnityEngine;

namespace Tetris.UI
{
    public abstract class MenuViewBase : MonoBehaviour
    {
        protected CanvasGroup _canvasGroup;

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            Hide();
        }

        public void Show()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        public virtual void SetInteractable(bool isInteractable)
        {
            _canvasGroup.interactable = isInteractable;
        }
    }
}