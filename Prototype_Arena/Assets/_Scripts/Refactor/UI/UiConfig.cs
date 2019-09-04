using UnityEngine;

namespace _Scripts.Refactor.UI
{
    [CreateAssetMenu(menuName = "ScriptableObject/UI/UiConfig", fileName = "UiConfig")]
    public class UiConfig : ScriptableObject
    {
        [SerializeField] private MainUiView _mainUiPrefab;
        [SerializeField] private ResolveUiView _resolveUiPrefab;
        [SerializeField] private EndScreenUiView _endScreenUiPrefab;
        [SerializeField] private GridUiView _gridUiPrefab;
        [SerializeField] private MenuUiView _menuUiPrefab;
        [SerializeField] private DraftUiView _draftUiPrefab;
        [SerializeField] private PlanUiView _planUiPrefab;

        public MainUiView MainUiPrefab
        {
            get { return _mainUiPrefab; }
        }
        
        public ResolveUiView ResolveUiPrefab
        {
            get { return _resolveUiPrefab; }
        }

        public EndScreenUiView EndScreenUiPrefab
        {
            get { return _endScreenUiPrefab; }
        }

        public GridUiView GridUiPrefab
        {
            get { return _gridUiPrefab; }
        }

        public MenuUiView MenuUiPrefab
        {
            get { return _menuUiPrefab; }
        }
        
        public DraftUiView DraftUiPrefab
        {
            get { return _draftUiPrefab; }
        }
        
        public PlanUiView PlanUiPrefab
        {
            get { return _planUiPrefab; }
        }
    }
}