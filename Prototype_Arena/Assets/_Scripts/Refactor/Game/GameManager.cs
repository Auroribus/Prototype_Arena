using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Refactor.Grid;
using _Scripts.Refactor.Hero;
using _Scripts.Refactor.PlayerScripts;
using _Scripts.Refactor.UI;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Scripts.Refactor.Game
{
    public class GameManager : MonoBehaviour
    {
        [Header("Scriptable Objects")] 
        [SerializeField] private HeroBaseConfig _heroBaseConfig;
        [SerializeField] private UiConfig _uiConfig;
        
        [Header("Prefabs")]
        [SerializeField] private HeroInfoPanel _heroInfoPanelPrefab;
        private HeroInfoPanel _heroInfoPanel;

        [SerializeField] private FadingBannerView _fadingBannerPrefab;
        private FadingBannerView _fadingBannerView;

        private MainUiView _mainUi;
        private ResolveUiView _resolveUi;
        private EndScreenUiView _endScreenUi;
        private GridUiView _gridUi;
        private MenuUiView _menuUi;
        private DraftUiView _draftUi;
        private PlanUiView _planUi;
        
        [Header("Grid")]
        [SerializeField] private GridCreator _gridPrefab;
        private GridCreator _gridPlayerOne;
        private GridCreator _gridPlayerTwo;
        [SerializeField] private Transform _gridPlayerOneAnchor;
        [SerializeField] private Transform _gridPlayerTwoAnchor;
        public GridCreator GridPlayerOne
        {
            get { return _gridPlayerOne; }
        }

        public GridCreator GridPlayerTwo
        {
            get { return _gridPlayerTwo; }
        }
        
        [Header("Draft")]
        [SerializeField] private Transform _draftPlayerOneAnchor;
        [SerializeField] private Transform _draftPlayerTwoAnchor;
        
        private Text _phaseText;
        private Text _playerTurnText;
        private Text _turnNumberText;
        private Text _winnerNameText;

        [SerializeField] private Text playerOneDraftedTextText;
        [SerializeField] private Text playerTwoDraftedTextText;
        [SerializeField] private Text playerOneActionsTextText;
        [SerializeField] private Text playerTwoActionsTextText;

        public Text PlayerOneDraftedText
        {
            get { return playerOneDraftedTextText; }
        }

        public Text PlayerTwoDraftedText
        {
            get { return playerTwoDraftedTextText; }
        }

        public Text PlayerOneActionsText
        {
            get { return playerOneActionsTextText; }
        }

        public Text PlayerTwoActionsText
        {
            get { return playerTwoActionsTextText; }
        }
        
        #region Variables
    
        //static reference which can be accessed in all other scripts by calling GameManager.instance
        public static GameManager Instance;

        [Header("Further clean up:")]
        //References to enums
        public Phase CurrentPhase = Phase.DraftPhase;
        public PlayerTurn CurrentPlayerTurn = PlayerTurn.Player1;
        
        //max units that each player can have
        public int MaxAmountOfUnits = 5;

        //temp reference to hero prefab
        public GameObject HeroPrefab;

        //Lists of all the hero units per player that are on the board
        [NonSerialized] public List<GameObject> HeroListP1 = new List<GameObject>();
        [NonSerialized] public List<GameObject> HeroListP2 = new List<GameObject>();

        private List<GameObject> HeroPool_P1 = new List<GameObject>();
        private List<GameObject> HeroPool_P2 = new List<GameObject>();

        //temp spawn point for randomly spawning heros
        private Vector2 hero_spawnpoint;
    
        private int _currentTurnCount;

        //bool to keep track if an action has ended so that resolving can continue
        public bool HasActionEnded;

        //player colors
        public Color ColorPlayer1;
        public Color ColorPlayer2;

        #endregion

        private void Awake()
        {
            //check if no instance already exists of game manager
            if (Instance == null)
            {
                Instance = this;
            }
        }

        // Use this for initialization
        void Start () {
            
            _heroInfoPanel = Instantiate(_heroInfoPanelPrefab);
            _fadingBannerView = Instantiate(_fadingBannerPrefab);

            _gridPlayerOne = Instantiate(_gridPrefab, _gridPlayerOneAnchor);
            _gridPlayerTwo = Instantiate(_gridPrefab, _gridPlayerTwoAnchor);
            
            //flip grid parent 2, so that the column orientation is the same as grid p1
            _gridPlayerTwo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            _mainUi = Instantiate(_uiConfig.MainUiPrefab);
            _menuUi = Instantiate(_uiConfig.MenuUiPrefab);
            _resolveUi = Instantiate(_uiConfig.ResolveUiPrefab);
            _gridUi = Instantiate(_uiConfig.GridUiPrefab);
            _endScreenUi = Instantiate(_uiConfig.EndScreenUiPrefab);
            _planUi = Instantiate(_uiConfig.PlanUiPrefab);
            _draftUi = Instantiate(_uiConfig.DraftUiPrefab);
            _resolveUi = Instantiate(_uiConfig.ResolveUiPrefab);

            _phaseText = _mainUi.PhaseText;
            _playerTurnText = _mainUi.PlayerTurnText;
            _turnNumberText = _mainUi.TurnNumberText;
            _winnerNameText = _endScreenUi.WinnersNameText;

            _gridUi.gameObject.SetActive(false);
            _endScreenUi.gameObject.SetActive(false);
            _planUi.gameObject.SetActive(false);
            _draftUi.gameObject.SetActive(false);
            _resolveUi.gameObject.SetActive(false);

            //set first state
            SetCurrentGameState(GameState.Menu);
        }
	
        // Update is called once per frame
        void Update () {     
        
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            DebugKeyboard();		
        }

        //For testing/debugging only
        private void DebugKeyboard()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetCurrentPhase(Phase.DraftPhase);
            }
            else if(Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetCurrentPhase(Phase.PlanPhase);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetCurrentPhase(Phase.ResolvePhase);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                ResetGame();
            }
        }

    
        //Function for handling the players drafting heroes
        private void DraftHeroUnits()
        {
            var heroBases = _heroBaseConfig.HeroBases;
            
            //spawn 10 random heroes for player 1
            for (var j = 0; j < 5; j++)
            {
                for (var i = 0; i < 2; i++)
                {
                    hero_spawnpoint.x = i * 1.2f + _draftPlayerOneAnchor.transform.position.x;
                    hero_spawnpoint.y = j * 1.2f + _draftPlayerOneAnchor.transform.position.y;

                    var random = Random.Range(0, heroBases.Count);
                    HeroPool_P1.Add(Instantiate(HeroPrefab, hero_spawnpoint, Quaternion.identity, _draftPlayerOneAnchor));
                    var heroView = HeroPool_P1[HeroPool_P1.Count - 1].GetComponent<HeroView>();

                    //assisgn hero specifics based on the hero base presets
                    heroView.HeroStatsModel.SetHealthPoints(heroBases[random].HealthPoints);
                    heroView.HeroStatsModel.SetAttackDamage(heroBases[random].Damage);
                    heroView.HeroStatsModel.SetInitiative(heroBases[random].Initiative);
                    heroView.GetComponentInChildren<SpriteRenderer>().sprite = heroBases[random].DraftSprite;
                    heroView.MainClass = heroBases[random].MainClass;

                    HeroPool_P1[HeroPool_P1.Count - 1].GetComponentInChildren<SpriteRenderer>().color = ColorPlayer1;
                    HeroPool_P1[HeroPool_P1.Count - 1].tag = "HeroP1";
                }
            }

            //spawn 10 random heroes for player 2
            for (var j = 0; j < 5; j++)
            {
                for (var i = 0; i < 2; i++)
                {
                    hero_spawnpoint.x = i * 1.2f + _draftPlayerTwoAnchor.transform.position.x;
                    hero_spawnpoint.y = j * 1.2f + _draftPlayerTwoAnchor.transform.position.y;

                    var random = Random.Range(0, heroBases.Count);
                    HeroPool_P2.Add(Instantiate(HeroPrefab, hero_spawnpoint, Quaternion.identity, _draftPlayerTwoAnchor));
                    var heroView = HeroPool_P2[HeroPool_P2.Count - 1].GetComponent<HeroView>();

                    //assisgn hero specifics based on the hero base presets
                    heroView.HeroStatsModel.SetHealthPoints(heroBases[random].HealthPoints);
                    heroView.HeroStatsModel.SetAttackDamage(heroBases[random].Damage);
                    heroView.HeroStatsModel.SetInitiative(heroBases[random].Initiative);
                    heroView.GetComponentInChildren<SpriteRenderer>().sprite = heroBases[random].DraftSprite;
                    heroView.MainClass = heroBases[random].MainClass;

                    HeroPool_P2[HeroPool_P2.Count - 1].GetComponentInChildren<SpriteRenderer>().color = ColorPlayer2;
                    HeroPool_P2[HeroPool_P2.Count - 1].GetComponentInChildren<SpriteRenderer>().flipX = true;
                    HeroPool_P2[HeroPool_P2.Count - 1].tag = "HeroP2";
                }
            }
        }

        public void SetHeroInfo(MainClass mainClass, int healthPoints, int damage, int initiative, string abilityName)
        {
            var heroName = "";

            switch(mainClass)
            {
                case MainClass.Scout:
                    heroName = "Scout";
                    break;
                case MainClass.Warrior:
                    heroName = "Warrior";
                    break;
                case MainClass.Mage:
                    heroName = "Mage";
                    break;
            }

            _heroInfoPanel.SetHeroName(heroName);
            _heroInfoPanel.SetHealthPoints(healthPoints);
            _heroInfoPanel.SetAttackDamage(damage);
            _heroInfoPanel.SetInitiative(initiative);
            _heroInfoPanel.SetAbilityName(abilityName);
        }

        private void PlaceDraftedUnits()
        {
            var column = 0;

            //instantiate heroes in the hero list in the grid system
            foreach (var hero in HeroListP1)
            {
                var heroView = hero.GetComponent<HeroView>();

                //check the class and select the column
                switch (heroView.MainClass)
                {
                    case MainClass.Warrior:
                        column = 2;
                        break;
                    case MainClass.Scout:
                        column = 1;
                        break;
                    case MainClass.Mage:
                        column = 0;
                        break;
                }

                //put character on random tile in set column
                //loop till no longer drafted
                while (heroView.IsHeroDrafted)
                {
                    //select random y
                    var random_y = Random.Range(0, 3);

                    var _tile = _gridPlayerOne.Grid[column, random_y].GetComponent<GridTile>();

                    if (!_tile.isOccupied)
                    {
                        var tile_x = _tile.pos_grid_x;
                        var tile_y = _tile.pos_grid_y;

                        //remove unit from pool list                                                                      
                        HeroPool_P1.Remove(hero);

                        //place unit and set spawnpoint
                        hero_spawnpoint = _gridPlayerOne.Grid[tile_x, tile_y].transform.position;
                        hero.transform.position = hero_spawnpoint;

                        //disable drafted visual
                        heroView.SetDrafted(false);
                    
                        //bool for the grid tile gets set to true so that no other unit can be spawned on top at the same time
                        _gridPlayerOne.Grid[tile_x, tile_y].GetComponent<GridTile>().isOccupied = true;

                        heroView.XPositionGrid = tile_x;
                        heroView.YPositionGrid = tile_y;

                        //enable ui text and images
                        heroView.SetUI(true);
                    }                
                }
            }

            foreach (var hero in HeroListP2)
            {
                var heroView = hero.GetComponent<HeroView>();

                //check the class and select the column
                switch (heroView.MainClass)
                {
                    case MainClass.Warrior:
                        column = 2;
                        break;
                    case MainClass.Scout:
                        column = 1;
                        break;
                    case MainClass.Mage:
                        column = 0;
                        break;
                }

                //put character on random tile in set column
                //loop till no longer drafted
                while (heroView.IsHeroDrafted)
                {
                    //select random y
                    var random_y = Random.Range(0, 3);

                    var _tile = _gridPlayerTwo.Grid[column, random_y].GetComponent<GridTile>();

                    if (!_tile.isOccupied)
                    {
                        var tile_x = _tile.pos_grid_x;
                        var tile_y = _tile.pos_grid_y;

                        //remove unit from pool list                                                                      
                        HeroPool_P2.Remove(hero);

                        //place unit and set spawnpoint
                        hero_spawnpoint = _gridPlayerTwo.Grid[tile_x, tile_y].transform.position;
                        hero.transform.position = hero_spawnpoint;

                        //disable drafted visual
                        heroView.SetDrafted(false);
                    
                        //bool for the grid tile gets set to true so that no other unit can be spawned on top at the same time
                        _gridPlayerTwo.Grid[tile_x, tile_y].GetComponent<GridTile>().isOccupied = true;

                        heroView.XPositionGrid = tile_x;
                        heroView.YPositionGrid = tile_y;
                    
                        //enable ui text and images
                        heroView.SetUI(true);
                    }
                }
            }

            //destroy pool lists
            foreach(var g in HeroPool_P1)
            {
                Destroy(g);
            }
            foreach (var g in HeroPool_P2)
            {
                Destroy(g);
            }

            HeroPool_P1.Clear();
            HeroPool_P2.Clear();
        }
    
        private void ResetGame()
        {
            //destroy all heros
            foreach (var g in HeroListP1)
            {
                Destroy(g);
            }
            foreach (var g in HeroListP2)
            {
                Destroy(g);
            }

            //destroy all heroes in pools
            foreach(var g in HeroPool_P1)
            {
                Destroy(g);
            }
            foreach(var g in HeroPool_P2)
            {
                Destroy(g);
            }

            //Reset bools in grid tiles
            foreach (var g in _gridPlayerOne.Grid)
            {
                g.GetComponent<GridTile>().isOccupied = false;
                g.GetComponent<GridTile>().SetMovementRing(false);
            }
            foreach (var g in _gridPlayerTwo.Grid)
            {
                g.GetComponent<GridTile>().isOccupied = false;
                g.GetComponent<GridTile>().SetMovementRing(false);
            }

            //clear lists
            HeroListP1.Clear();
            HeroListP2.Clear();
            
            Player.Instance.ClearActionIcons();
            Player.Instance.ClearActionsList();
                
            //reset enums
            SetCurrentGameState(GameState.Game);
            SetPlayerTurn(PlayerTurn.Player1);
        }

        public void SetPlayerTurn(PlayerTurn playerTurn)
        {
            CurrentPlayerTurn = playerTurn;
            switch(playerTurn)
            {
                case PlayerTurn.Player1:
                    _playerTurnText.text = "Player 1";
                    _playerTurnText.color = ColorPlayer1;
                    break;
                case PlayerTurn.Player2:
                    _playerTurnText.text = "Player 2";
                    _playerTurnText.color = ColorPlayer2;
                                
                    switch (CurrentPhase)
                    {
                        case Phase.PlanPhase:

                            foreach (GameObject g in HeroListP1)
                            {
                                var hero = g.GetComponent<HeroView>();

                                hero.SetUI(false);
                                hero.SetAction(false);
                            }

                            PlayerOneActionsText.text = string.Empty;
                            PlayerTwoActionsText.text = "Actions: 0/" + Player.MaxNumberOfPlayerActions;
                            break;
                    }
                
                    //fade animation ui
                    _fadingBannerView.SetAnimationUI(true, CurrentPhase, CurrentPlayerTurn);

                    break;
            }
        }

        public void SetCurrentGameState(GameState active_state)
        {        
            //do stuff with new state
            switch (active_state)
            {
                case GameState.Menu:
                    break;
                case GameState.Game:
                    _menuUi.gameObject.SetActive(false);
                    _endScreenUi.gameObject.SetActive(false);
                    SetCurrentPhase(Phase.DraftPhase);
                    break;
                case GameState.Paused:
                    break;
                case GameState.Ended:
                    SetEndScreen();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("No game state found for: " + active_state);
            }
        }

        public void SetCurrentPhase(Phase active_phase)
        {
            switch(active_phase)
            {
                case Phase.IdlePhase:
                    break;

                case Phase.DraftPhase:

                    //fade in/out animation ui
                    _fadingBannerView.SetAnimationUI(true, Phase.DraftPhase, CurrentPlayerTurn);

                    //set turn to 1
                    _currentTurnCount = 1;
                    _turnNumberText.text = "turn: " + _currentTurnCount;

                    _draftUi.gameObject.SetActive(true);
                    _phaseText.text = "Draft Phase";

                    //reset draft text
                    PlayerOneDraftedText.text = "Drafted: 0/5";
                    PlayerTwoDraftedText.text = "Drafted: 0/5";
                
                    //disable plan ui
                    _planUi.gameObject.SetActive(false);

                    DraftHeroUnits();

                    break;

                case Phase.PlanPhase:

                    Player.Instance.ClearActionIcons();

                    //check if turn is player 1s turn
                    SetPlayerTurn(CurrentPlayerTurn);

                    //fade in/out animation ui
                    _fadingBannerView.SetAnimationUI(true, Phase.PlanPhase, CurrentPlayerTurn);
                
                    CleanLists();

                    //enable grid ui
                    _gridUi.gameObject.SetActive(true);
                
                    _draftUi.gameObject.SetActive(false);
                    _resolveUi.gameObject.SetActive(false);
                    _phaseText.text = "Planning Phase";

                    _planUi.gameObject.SetActive(true);

                    //reset player actions on new turn
                    PlayerOneActionsText.text = "Actions: 0/3";
                    PlayerTwoActionsText.text = "";

                    Player.Instance.ResetPlayerActionCounts();

                    foreach(var hero in HeroListP1)
                    {
                        var heroView = hero.GetComponent<HeroView>();
                        heroView.SetUI(true);
                    }
                    foreach (var hero in HeroListP2)
                    {
                        var heroView = hero.GetComponent<HeroView>();
                        heroView.SetUI(true);
                    }

                    //if phase was drafted phase, place units
                    if (CurrentPhase == Phase.DraftPhase)
                    {
                        PlaceDraftedUnits();
                    }

                    break;

                case Phase.ResolvePhase:

                    //hide player turn text/set empty
                    _playerTurnText.text = "";

                    //fade in/out animation ui
                    _fadingBannerView.SetAnimationUI(true, Phase.ResolvePhase, CurrentPlayerTurn);

                    //disable plan ui
                    _planUi.gameObject.SetActive(false);

                    //enable resolve ui
                    _resolveUi.gameObject.SetActive(true);

                    //set action ended to true so that animations can play
                    HasActionEnded = true;

                    //hide stats on heroes and green check                
                    foreach(GameObject g in HeroListP2)
                    {
                        HeroView heroView = g.GetComponent<HeroView>();

                        heroView.SetUI(false);
                        heroView.SetAction(false);
                    }

                    _phaseText.text = "Resolve Phase";
                    StartCoroutine(Player.Instance.ResolveActions());
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException("no phase found for: " + active_phase);
            }

            //set current phase to active phase
            CurrentPhase = active_phase;
        }

        public IEnumerator ClearKilledHeroes()
        {
            foreach(var hero in HeroListP1)
            {
                if(hero.GetComponent<HeroView>().HeroStatsModel.HealthPoints <= 0)
                {
                    Destroy(hero);
                }
            }

            foreach(var hero in HeroListP2)
            {
                if (hero.GetComponent<HeroView>().HeroStatsModel.HealthPoints <= 0)
                {
                    Destroy(hero);
                }
            }

            yield return new WaitForSeconds(1f);
            //clean up lists, need small waiting buffer to see if heroes have died or not, could also check if still heroes in list, their hp is 0 or less
            CleanLists();

            //check if either player has no more heroes
            if(HeroListP1.Count == 0 || HeroListP2.Count == 0)
            {
                SetCurrentGameState(GameState.Ended);
            }
            else
            {
                SetCurrentPhase(Phase.PlanPhase);
                //increment turn
                _currentTurnCount++;
                _turnNumberText.text = "turn: " + _currentTurnCount;
            }
        }

        public void CleanLists()
        {
            HeroListP1.RemoveAll(item => item == null);
            HeroListP2.RemoveAll(item => item == null);
        }

        private void SetEndScreen()
        {
            //enable end ui
            _endScreenUi.gameObject.SetActive(true);

            //set winners name
            if(HeroListP2.Count == 0)
            {
                _winnerNameText.text = "Player 1";
                _winnerNameText.color = ColorPlayer1;
            }
            else if(HeroListP1.Count == 0)
            {
                _winnerNameText.text = "Player 2";
                _winnerNameText.color = ColorPlayer2;
            }
        }

        public void OnDraftReady()
        {
            //check if both players selected the max amount of units to continue to planning phase
            if (HeroListP1.Count == MaxAmountOfUnits && HeroListP2.Count == MaxAmountOfUnits)
            {
                //temp SetCurrentPhase(Phase.PlanPhase);
            }

            SetCurrentPhase(Phase.PlanPhase);
        }

        public void OnPlanReady()
        {
            switch(CurrentPlayerTurn)
            {
                case PlayerTurn.Player1:
                    //set players turn to player 2
                    SetPlayerTurn(PlayerTurn.Player2);

                    break;

                case PlayerTurn.Player2:
                    //set to resolve phase
                    SetCurrentPhase(Phase.ResolvePhase);

                    break;
            }
        }

        public void OnResolveReady()
        {
            //increment turn
            _currentTurnCount++;
            _turnNumberText.text = "turn: " + _currentTurnCount;

            //set phase back to planning phase, or end screen if game over
            SetCurrentPhase(Phase.PlanPhase);

            //reset visual plan list
            Player.Instance.ClearActionIcons();
        }

        public void OnRestart()
        {
            ResetGame();
        }

        public void OnStart()
        {
            SetCurrentGameState(GameState.Game);
        }
    }
}