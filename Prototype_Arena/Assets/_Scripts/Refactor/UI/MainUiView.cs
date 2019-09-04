using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Refactor.UI
{
    public class MainUiView : MonoBehaviour
    {
        [SerializeField] private Text _phaseText;
        [SerializeField] private Text _playerTurnText;
        [SerializeField] private Text _turnNumberText;

        public Text PhaseText
        {
            get { return _phaseText; }
        }

        public Text PlayerTurnText
        {
            get { return _playerTurnText; }
        }

        public Text TurnNumberText
        {
            get { return _turnNumberText; }
        }
    }
}