using System;
using _Scripts.Refactor.Game;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Refactor.UI
{
    public class DraftUiView : MonoBehaviour
    {
        [SerializeField] private Button _readyButton;
        [SerializeField] private Text _playerOneDraftedText;
        [SerializeField] private Text _playerTwoDraftedText;

        public Text PlayerOneDraftedText
        {
            get { return _playerOneDraftedText; }
        }

        public Text PlayerTwoDraftedText
        {
            get { return _playerTwoDraftedText; }
        }

        private void Start()
        {
            _readyButton.onClick.AddListener(() => GameManager.Instance.OnDraftReady());
        }
    }
}