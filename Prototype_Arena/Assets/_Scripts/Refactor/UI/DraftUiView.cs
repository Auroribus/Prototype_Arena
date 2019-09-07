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

         public GameManager GameManager { private get; set; }
        
        public Text PlayerOneDraftedText
        {
            get { return _playerOneDraftedText; }
        }

        public Text PlayerTwoDraftedText
        {
            get { return _playerTwoDraftedText; }
        }

        public DraftUiView()
        {
            
        }

        private void Start()
        {
            _readyButton.onClick.AddListener(() => GameManager.OnDraftReady());
            gameObject.SetActive(false);
        }
    }
}