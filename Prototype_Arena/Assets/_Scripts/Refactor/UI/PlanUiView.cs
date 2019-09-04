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
            _readyButton.onClick.AddListener(() => GameManager.Instance.OnPlanReady());
        }
    }
}