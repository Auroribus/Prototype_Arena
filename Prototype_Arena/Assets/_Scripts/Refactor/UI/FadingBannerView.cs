using _Scripts.Refactor.Game;
using _Scripts.Refactor.PlayerScripts;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Refactor.UI
{
    public class FadingBannerView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Text _phaseText;
        [SerializeField] private Text _turnText;

        public Animator Animator
        {
            get { return _animator; }
        }

        private void Start()
        {
            _animator.gameObject.SetActive(false);
        }
        
        public void SetAnimationUI(
            bool fadeIn, 
            Phase currentPhase, 
            PlayerTurn player)
        {
            _animator.gameObject.SetActive(true);
            
            switch(currentPhase)
            {
                case Phase.DraftPhase:
                    _phaseText.text = "Draft Phase";
                    break;
                case Phase.PlanPhase:
                    _phaseText.text = "Planning Phase";
                    break;
                case Phase.ResolvePhase:
                    _phaseText.text = "Resolve Phase";
                    break;
            }

            switch (player)
            {
                case PlayerTurn.Player1:
                    _turnText.text = "Player 1";
                    _turnText.color = GameManager.Instance.ColorPlayer1;
                    break;
                case PlayerTurn.Player2:
                    _turnText.text = "Player 2";
                    _turnText.color = GameManager.Instance.ColorPlayer2;
                    break;
            }
            
            _animator.SetTrigger("FadeIn");
        }
    }
}