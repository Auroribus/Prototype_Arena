using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Refactor.Grid;
using _Scripts.Refactor.Hero;
using _Scripts.Refactor.PlayerScripts;
using _Scripts.Refactor.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Scripts.Refactor.Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private HeroInfoPanel _heroInfoPanelPrefab;
        private HeroInfoPanel _heroInfoPanel;

        [SerializeField] private FadingBannerView _fadingBannerPrefab;
        private FadingBannerView _fadingBannerView;

        #region Variables
    
        //static reference which can be accessed in all other scripts by calling GameManager.instance
        public static GameManager Instance;

        //References to enums
        public GameState CurrentState = GameState.Game;
        public Phase CurrentPhase = Phase.DraftPhase;
        public PlayerTurn CurrentPlayerTurn = PlayerTurn.Player1;

        //Reference to the grid parents for each player
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

        //max units that each player can have
        public int max_amount_units = 5;

        //temp reference to hero prefab
        public GameObject HeroPrefab;

        //Lists of all the hero units per player that are on the board
        [NonSerialized] public List<GameObject> HeroListP1 = new List<GameObject>();
        [NonSerialized] public List<GameObject> HeroListP2 = new List<GameObject>();

        private List<GameObject> HeroPool_P1 = new List<GameObject>();
        private List<GameObject> HeroPool_P2 = new List<GameObject>();

        //temp spawn point for randomly spawning heros
        private Vector2 hero_spawnpoint;

        private Text phase_text;
        private Text player_turn_text;
        private Text turn_number_text;
        private Text Winner_playername_text;

        public List<HeroBase> Heroes = new List<HeroBase>();
    
        public int Current_turn_number;

        private Transform DraftP1;
        private Transform DraftP2;

        private GameObject DraftUI;
        [NonSerialized] public Text P1_drafted, P2_drafted;
        
        private GameObject PlanUI;
        [NonSerialized] public Text P1_actions, P2_actions;

        private GameObject ResolveUI;

        private GameObject EndUI;

        private GameObject GridUI;

        private GameObject MenuUI;

        //bool to keep track if an action has ended so that resolving can continue
        public bool action_ended = true;

        //player colors
        [FormerlySerializedAs("Player1_color")] 
        public Color ColorPlayer1;
        [FormerlySerializedAs("Player2_color")] 
        public Color ColorPlayer2;

        #endregion

        private void Awake()
        {
            //check if no instance already exists of game manager
            if (Instance == null)
                Instance = this;
        }

        // Use this for initialization
        void Start () {
            
            _heroInfoPanel = Instantiate(_heroInfoPanelPrefab);
            _fadingBannerView = Instantiate(_fadingBannerPrefab);

            _gridPlayerOne = Instantiate(_gridPrefab);
            _gridPlayerOne.transform.SetParent(_gridPlayerOneAnchor);
            _gridPlayerTwo = Instantiate(_gridPrefab);
            _gridPlayerTwo.transform.SetParent(_gridPlayerTwoAnchor);
            
            MenuUI = GameObject.Find("Menu UI");

            //flip grid parent 2, so that the column orientation is the same as grid p1
            _gridPlayerTwo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            phase_text = GameObject.Find("Phase").GetComponent<Text>();
            player_turn_text = GameObject.Find("Turn").GetComponent<Text>();
            turn_number_text = GameObject.Find("Turn number").GetComponent<Text>();

            DraftP1 = GameObject.Find("DraftP1").transform;
            DraftP2 = GameObject.Find("DraftP2").transform;

            DraftUI = GameObject.Find("Draft UI");
            P1_drafted = DraftUI.transform.Find("P1 drafted").GetComponent<Text>();
            P2_drafted = DraftUI.transform.Find("P2 drafted").GetComponent<Text>();
        
            PlanUI = GameObject.Find("Plan UI");
            P1_actions = PlanUI.transform.Find("P1_actions").GetComponent<Text>();
            P2_actions = PlanUI.transform.Find("P2_actions").GetComponent<Text>();

            ResolveUI = GameObject.Find("Resolve UI");
            ResolveUI.SetActive(true);

            EndUI = GameObject.Find("End UI");
            Winner_playername_text = EndUI.transform.Find("Winner Playername").GetComponent<Text>();

            GridUI = GameObject.Find("Grid UI");

            GridUI.SetActive(false);
            EndUI.SetActive(false);
            PlanUI.SetActive(false);
            DraftUI.SetActive(false);
            ResolveUI.SetActive(false);

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
            //spawn 10 random heroes for player 1
            for (var j = 0; j < 5; j++)
            {
                for (var i = 0; i < 2; i++)
                {
                    hero_spawnpoint.x = i * 1.2f + DraftP1.transform.position.x;
                    hero_spawnpoint.y = j * 1.2f + DraftP1.transform.position.y;

                    var random = Random.Range(0, Heroes.Count);
                    HeroPool_P1.Add(Instantiate(HeroPrefab, hero_spawnpoint, Quaternion.identity, DraftP1));
                    var heroView = HeroPool_P1[HeroPool_P1.Count - 1].GetComponent<HeroView>();

                    //assisgn hero specifics based on the hero base presets
                    heroView.HeroStatsModel.SetHealthPoints(Heroes[random].HealthPoints);
                    heroView.HeroStatsModel.SetAttackDamage(Heroes[random].Damage);
                    heroView.HeroStatsModel.SetInitiative(Heroes[random].Initiative);
                    heroView.GetComponentInChildren<SpriteRenderer>().sprite = Heroes[random].Draft_sprite;
                    heroView.main_class = Heroes[random].Main_class;

                    HeroPool_P1[HeroPool_P1.Count - 1].GetComponentInChildren<SpriteRenderer>().color = ColorPlayer1;
                    HeroPool_P1[HeroPool_P1.Count - 1].tag = "HeroP1";
                }
            }

            //spawn 10 random heroes for player 2
            for (var j = 0; j < 5; j++)
            {
                for (var i = 0; i < 2; i++)
                {
                    hero_spawnpoint.x = i * 1.2f + DraftP2.transform.position.x;
                    hero_spawnpoint.y = j * 1.2f + DraftP2.transform.position.y;

                    var random = Random.Range(0, Heroes.Count);
                    HeroPool_P2.Add(Instantiate(HeroPrefab, hero_spawnpoint, Quaternion.identity, DraftP2));
                    var heroView = HeroPool_P2[HeroPool_P2.Count - 1].GetComponent<HeroView>();

                    //assisgn hero specifics based on the hero base presets
                    heroView.HeroStatsModel.SetHealthPoints(Heroes[random].HealthPoints);
                    heroView.HeroStatsModel.SetAttackDamage(Heroes[random].Damage);
                    heroView.HeroStatsModel.SetInitiative(Heroes[random].Initiative);
                    heroView.GetComponentInChildren<SpriteRenderer>().sprite = Heroes[random].Draft_sprite;
                    heroView.main_class = Heroes[random].Main_class;

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
            for (var i = 0; i < HeroListP1.Count; i++)
            {
                HeroView heroView = HeroListP1[i].GetComponent<HeroView>();

                //check the class and select the column
                switch (heroView.main_class)
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
                while (heroView.isDrafted)
                {
                    //select random y
                    var random_y = Random.Range(0, 3);

                    GridTile _tile = _gridPlayerOne.Grid[column, random_y].GetComponent<GridTile>();

                    if (!_tile.isOccupied)
                    {
                        int tile_x = _tile.pos_grid_x;
                        int tile_y = _tile.pos_grid_y;

                        //remove unit from pool list                                                                      
                        HeroPool_P1.Remove(HeroListP1[i]);

                        //place unit and set spawnpoint
                        hero_spawnpoint = _gridPlayerOne.Grid[tile_x, tile_y].transform.position;
                        HeroListP1[i].transform.position = hero_spawnpoint;

                        //disable drafted visual
                        heroView.SetDrafted(false);
                    
                        //bool for the grid tile gets set to true so that no other unit can be spawned on top at the same time
                        _gridPlayerOne.Grid[tile_x, tile_y].GetComponent<GridTile>().isOccupied = true;

                        heroView.x_position_grid = tile_x;
                        heroView.y_position_grid = tile_y;

                        //enable ui text and images
                        heroView.SetUI(true);
                    }                
                }            
            }

            for (var i = 0; i < HeroListP2.Count; i++)
            {
                HeroView heroView = HeroListP2[i].GetComponent<HeroView>();

                //check the class and select the column
                switch (heroView.main_class)
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
                while (heroView.isDrafted)
                {
                    //select random y
                    var random_y = Random.Range(0, 3);

                    var _tile = _gridPlayerTwo.Grid[column, random_y].GetComponent<GridTile>();

                    if (!_tile.isOccupied)
                    {
                        var tile_x = _tile.pos_grid_x;
                        var tile_y = _tile.pos_grid_y;

                        //remove unit from pool list                                                                      
                        HeroPool_P2.Remove(HeroListP2[i]);

                        //place unit and set spawnpoint
                        hero_spawnpoint = _gridPlayerTwo.Grid[tile_x, tile_y].transform.position;
                        HeroListP2[i].transform.position = hero_spawnpoint;

                        //disable drafted visual
                        heroView.SetDrafted(false);
                    
                        //bool for the grid tile gets set to true so that no other unit can be spawned on top at the same time
                        _gridPlayerTwo.Grid[tile_x, tile_y].GetComponent<GridTile>().isOccupied = true;

                        heroView.x_position_grid = tile_x;
                        heroView.y_position_grid = tile_y;
                    
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
                    player_turn_text.text = "Player 1";
                    player_turn_text.color = ColorPlayer1;
                    break;
                case PlayerTurn.Player2:
                    player_turn_text.text = "Player 2";
                    player_turn_text.color = ColorPlayer2;
                                
                    switch (CurrentPhase)
                    {
                        case Phase.PlanPhase:

                            foreach (GameObject g in HeroListP1)
                            {
                                var hero = g.GetComponent<HeroView>();

                                hero.SetUI(false);
                                hero.SetAction(false);
                            }

                            P1_actions.text = string.Empty;
                            P2_actions.text = "Actions: 0/" + Player.Instance.MaxPlayerActions;
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
                    MenuUI.SetActive(false);
                    EndUI.SetActive(false);
                    SetCurrentPhase(Phase.DraftPhase);
                    break;
                case GameState.Paused:
                    break;
                case GameState.Ended:
                    SetEndScreen();
                    break;
            }

            CurrentState = active_state;
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
                    Current_turn_number = 1;
                    turn_number_text.text = "turn: " + Current_turn_number;

                    DraftUI.SetActive(true);
                    phase_text.text = "Draft Phase";

                    //reset draft text
                    P1_drafted.text = "Drafted: 0/5";
                    P2_drafted.text = "Drafted: 0/5";
                
                    //disable plan ui
                    PlanUI.SetActive(false);

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
                    GridUI.SetActive(true);
                
                    DraftUI.SetActive(false);
                    ResolveUI.SetActive(false);
                    phase_text.text = "Planning Phase";

                    PlanUI.SetActive(true);

                    //reset player actions on new turn
                    P1_actions.text = "Actions: 0/3";
                    P2_actions.text = "";

                    Player.Instance.PlayerOneActionCount = 0;
                    Player.Instance.PlayerTwoActionCount = 0;

                    foreach(GameObject hero in HeroListP1)
                    {
                        HeroView heroView = hero.GetComponent<HeroView>();
                        heroView.SetUI(true);
                    }
                    foreach (GameObject hero in HeroListP2)
                    {
                        HeroView heroView = hero.GetComponent<HeroView>();
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
                    player_turn_text.text = "";

                    //fade in/out animation ui
                    _fadingBannerView.SetAnimationUI(true, Phase.ResolvePhase, CurrentPlayerTurn);

                    //disable plan ui
                    PlanUI.SetActive(false);

                    //enable resolve ui
                    ResolveUI.SetActive(true);

                    //set action ended to true so that animations can play
                    action_ended = true;

                    //hide stats on heroes and green check                
                    foreach(GameObject g in HeroListP2)
                    {
                        HeroView heroView = g.GetComponent<HeroView>();

                        heroView.SetUI(false);
                        heroView.SetAction(false);
                    }

                    phase_text.text = "Resolve Phase";
                    StartCoroutine(Player.Instance.ResolveActions());
                    break;
            }

            //set current phase to active phase
            CurrentPhase = active_phase;
        }

        public IEnumerator ClearKilledHeroes()
        {
            foreach(GameObject hero in HeroListP1)
            {
                if(hero.GetComponent<HeroView>().HeroStatsModel.HealthPoints <= 0)
                {
                    Destroy(hero);
                }
            }

            foreach(GameObject hero in HeroListP2)
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
                Current_turn_number++;
                turn_number_text.text = "turn: " + Current_turn_number;
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
            EndUI.SetActive(true);

            //set winners name
            if(HeroListP2.Count == 0)
            {
                Winner_playername_text.text = "Player 1";
                Winner_playername_text.color = ColorPlayer1;
            }
            else if(HeroListP1.Count == 0)
            {
                Winner_playername_text.text = "Player 2";
                Winner_playername_text.color = ColorPlayer2;
            }
        }

        public void OnDraftReady()
        {
            //check if both players selected the max amount of units to continue to planning phase
            if (HeroListP1.Count == max_amount_units && HeroListP2.Count == max_amount_units)
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
            Current_turn_number++;
            turn_number_text.text = "turn: " + Current_turn_number;

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

    [Serializable]
    public class HeroBase
    {
        public string Hero_name;
        public int HealthPoints;
        public int Damage;
        public int Initiative;
        public MainClass Main_class;
        public SubClass Sub_clas;
        public Sprite Draft_sprite;
        public Sprite Main_sprite;
    }
}