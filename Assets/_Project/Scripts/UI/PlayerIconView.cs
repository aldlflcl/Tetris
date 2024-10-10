using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tetris.UI
{
    public class PlayerIconView: MonoBehaviour
    {
        [SerializeField] private TMP_Text _playerNameText;

        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _profileBorderImage;
        [SerializeField] private Image _namePlateImage;
        [SerializeField] private GameObject _readyImage;
        [SerializeField] private GameObject _hostMarkImage;

        public string PlayerId { get; private set; }
        
        public void Setup(string playerId, string playerName, Color backgroundColor, Color borderImageColor, bool isHost, bool isReady)
        {
            PlayerId = playerId;
            _playerNameText.text = playerName;
            _backgroundImage.color = backgroundColor;
            _profileBorderImage.color = borderImageColor;
            _namePlateImage.color = borderImageColor;
            _hostMarkImage.SetActive(isHost);
            _readyImage.SetActive(isReady);
        }

        public void SetReady(bool isReady)
        {
           _readyImage.SetActive(isReady); 
        }

        public bool ToggleReadyState()
        {
            var isReady = !_readyImage.activeSelf;
            SetReady(isReady);
            return isReady;
        }
    }
}