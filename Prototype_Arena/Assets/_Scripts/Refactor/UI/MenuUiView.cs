using _Scripts.Refactor.Game;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Refactor.UI
{
    public class MenuUiView : MonoBehaviour
    {
        [SerializeField] private Button _startButton;

        public GameManager GameManager { private get; set; }
        
        private void Start()
        {
            _startButton.onClick.AddListener(() => GameManager.OnStart());
        }
    }
}