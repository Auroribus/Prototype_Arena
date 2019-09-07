using _Scripts.Refactor.Game;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Refactor.UI
{
    public class PlanUiView : MonoBehaviour
    {
        [SerializeField] private Button _readyButton;
        [SerializeField] private Text _playerOneActionsText;
        [SerializeField] private Text _playerTwoActionsText;

        public GameManager GameManager { private get; set; }
        
        public Text PlayerOneActionsText
        {
            get { return _playerOneActionsText; }
        }

        public Text PlayerTwoActionsText
        {
            get { return _playerTwoActionsText; }
        }

        private void Start()
        {
            _readyButton.onClick.AddListener(() => GameManager.OnPlanReady());
            
            gameObject.SetActive(false);
        }
    }
}