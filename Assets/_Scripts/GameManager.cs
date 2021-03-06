﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region enums

public enum GameState
{
    Menu,
    Game,
    Paused,
    Ended
}

public enum Phase
{
    IdlePhase,
    DraftPhase,
    PlanPhase,
    ResolvePhase
}

public enum PlayerTurn
{
    Player1,
    Player2
}

public enum MainClass
{
    Warrior,
    Scout,
    Mage
}

public enum SubClass
{
    Scout1,
    Scout2,
    Scout3,
    Warrior1,
    Warrior2,
    Warrior3,
    Mage1,
    Mage2,
    Mage3
}

public enum ProjectileType
{
    Arrow,
    BounceArrow,
    Fireball,
    WindSlash,
    ChainLightning
}

#endregion

public class GameManager : MonoBehaviour {

    #region Variables
    
    //static reference which can be accessed in all other scripts by calling GameManager.instance
    public static GameManager instance;

    //References to enums
    public GameState CurrentState = GameState.Game;
    public Phase CurrentPhase = Phase.DraftPhase;
    public PlayerTurn CurrentTurn = PlayerTurn.Player1;

    //Reference to each player
    [System.NonSerialized] public GameObject Player1, Player2;

    //Reference to the grid parents for each player
    [System.NonSerialized] public GridParent Grid_P1;
    [System.NonSerialized] public GridParent Grid_P2;

    //max units that each player can have
    public int max_amount_units = 5;

    //temp reference to hero prefab
    public GameObject HeroPrefab;

    //Lists of all the hero units per player that are on the board
    [System.NonSerialized] public List<GameObject> HeroList_P1 = new List<GameObject>();
    [System.NonSerialized] public List<GameObject> HeroList_P2 = new List<GameObject>();

    [System.NonSerialized] public List<GameObject> HeroPool_P1 = new List<GameObject>();
    [System.NonSerialized] public List<GameObject> HeroPool_P2 = new List<GameObject>();

    //temp spawn point for randomly spawning heros
    private Vector2 hero_spawnpoint;

    private Text phase_text;
    private Text player_turn_text;
    private Text turn_number_text;
    private Text Winner_playername_text;

    public List<HeroBase> Heroes = new List<HeroBase>();
    
    public int Current_turn_number = 0;

    private Transform DraftP1;
    private Transform DraftP2;

    private GameObject DraftUI;
    [System.NonSerialized] public Text P1_drafted, P2_drafted;
    private Transform hero_info_panel;
    private Text hero_name;
    private Text hero_hp;
    private Text hero_dmg;
    private Text hero_init;
    private Text hero_abil;

    private GameObject PlanUI;
    [System.NonSerialized] public Text P1_actions, P2_actions;

    private GameObject ResolveUI;

    private GameObject EndUI;

    private GameObject GridUI;

    private GameObject MenuUI;

    private GameObject AnimationUI;
    private Animator animator_ui;
    private Text animation_phase_text;
    private Text animation_turn_text;

    //bool to keep track if an action has ended so that resolving can continue
    public bool action_ended = true;

    //player colors
    public Color Player1_color;
    public Color Player2_color;

    #endregion

    private void Awake()
    {
        //check if no instance already exists of game manager
        if (instance == null)
            instance = this;
    }

    // Use this for initialization
    void Start () {
        Player1 = GameObject.Find("Player1");
        Player2 = GameObject.Find("Player2");

        MenuUI = GameObject.Find("Menu UI");

        Grid_P2 = GameObject.Find("GridParent P2").GetComponent<GridParent>();
        //flip grid parent 2, so that the column orientation is the same as grid p1
        Grid_P2.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        Grid_P1 = GameObject.Find("GridParent P1").GetComponent<GridParent>();

        phase_text = GameObject.Find("Phase").GetComponent<Text>();
        player_turn_text = GameObject.Find("Turn").GetComponent<Text>();
        turn_number_text = GameObject.Find("Turn number").GetComponent<Text>();

        DraftP1 = GameObject.Find("DraftP1").transform;
        DraftP2 = GameObject.Find("DraftP2").transform;

        DraftUI = GameObject.Find("Draft UI");
        P1_drafted = DraftUI.transform.Find("P1 drafted").GetComponent<Text>();
        P2_drafted = DraftUI.transform.Find("P2 drafted").GetComponent<Text>();
        hero_info_panel = GameObject.Find("Hero Info Panel").transform;

        hero_name = hero_info_panel.Find("Hero_name").GetComponent<Text>();
        hero_hp = hero_info_panel.Find("HP_value").GetComponent<Text>();
        hero_dmg = hero_info_panel.Find("DMG_value").GetComponent<Text>();
        hero_init = hero_info_panel.Find("INIT_value").GetComponent<Text>();
        hero_abil = hero_info_panel.Find("ABIL_name").GetComponent<Text>();

        PlanUI = GameObject.Find("Plan UI");
        P1_actions = PlanUI.transform.Find("P1_actions").GetComponent<Text>();
        P2_actions = PlanUI.transform.Find("P2_actions").GetComponent<Text>();

        ResolveUI = GameObject.Find("Resolve UI");
        ResolveUI.SetActive(true);

        EndUI = GameObject.Find("End UI");
        Winner_playername_text = EndUI.transform.Find("Winner Playername").GetComponent<Text>();

        GridUI = GameObject.Find("Grid UI");

        AnimationUI = GameObject.Find("Animation UI");
        animator_ui = AnimationUI.GetComponent<Animator>();
        animation_phase_text = AnimationUI.transform.Find("Phase Text").GetComponent<Text>();
        animation_turn_text = AnimationUI.transform.Find("Turn Text").GetComponent<Text>();

        
        GridUI.SetActive(false);
        EndUI.SetActive(false);
        PlanUI.SetActive(false);
        DraftUI.SetActive(false);
        ResolveUI.SetActive(false);
        AnimationUI.SetActive(false);

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

    public void SetAnimationUI(bool fade_in, Phase current_phase, PlayerTurn player)
    {
        if(fade_in)
        {
            AnimationUI.SetActive(true);
            
            switch(current_phase)
            {
                case Phase.DraftPhase:
                    animation_phase_text.text = "Draft Phase";
                    break;
                case Phase.PlanPhase:
                    animation_phase_text.text = "Planning Phase";
                    break;
                case Phase.ResolvePhase:
                    animation_phase_text.text = "Resolve Phase";
                    break;
            }

            if(current_phase != Phase.DraftPhase && current_phase != Phase.ResolvePhase)
            {
                switch (player)
                {
                    case PlayerTurn.Player1:
                        animation_turn_text.text = "Player 1";
                        animation_turn_text.color = Player1_color;
                        break;
                    case PlayerTurn.Player2:
                        animation_turn_text.text = "Player 2";
                        animation_turn_text.color = Player2_color;
                        break;
                }
            }
            else
            {
                animation_turn_text.text = "";
            }
            

            animator_ui.SetTrigger("FadeIn");
        }
    }
    
    //Function for handling the players drafting heroes
    private void DraftHeroUnits()
    {
        //spawn 10 random heroes for player 1
        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < 2; i++)
            {
                hero_spawnpoint.x = i * 1.2f + DraftP1.transform.position.x;
                hero_spawnpoint.y = j * 1.2f + DraftP1.transform.position.y;

                int random = Random.Range(0, Heroes.Count);
                HeroPool_P1.Add(Instantiate(HeroPrefab, hero_spawnpoint, Quaternion.identity, DraftP1));
                Hero hero = HeroPool_P1[HeroPool_P1.Count - 1].GetComponent<Hero>();

                //assisgn hero specifics based on the hero base presets
                hero.Healthpoints = Heroes[random].HealthPoints;
                hero.Damage = Heroes[random].Damage;
                hero.Initiative = Heroes[random].Initiative;
                hero.GetComponentInChildren<SpriteRenderer>().sprite = Heroes[random].Draft_sprite;
                hero.main_class = Heroes[random].Main_class;

                HeroPool_P1[HeroPool_P1.Count - 1].GetComponentInChildren<SpriteRenderer>().color = Player1_color;
                HeroPool_P1[HeroPool_P1.Count - 1].tag = "HeroP1";
            }
        }

        //spawn 10 random heroes for player 2
        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < 2; i++)
            {
                hero_spawnpoint.x = i * 1.2f + DraftP2.transform.position.x;
                hero_spawnpoint.y = j * 1.2f + DraftP2.transform.position.y;

                int random = Random.Range(0, Heroes.Count);
                HeroPool_P2.Add(Instantiate(HeroPrefab, hero_spawnpoint, Quaternion.identity, DraftP2));
                Hero hero = HeroPool_P2[HeroPool_P2.Count - 1].GetComponent<Hero>();

                //assisgn hero specifics based on the hero base presets
                hero.Healthpoints = Heroes[random].HealthPoints;
                hero.Damage = Heroes[random].Damage;
                hero.Initiative = Heroes[random].Initiative;
                hero.GetComponentInChildren<SpriteRenderer>().sprite = Heroes[random].Draft_sprite;
                hero.main_class = Heroes[random].Main_class;

                HeroPool_P2[HeroPool_P2.Count - 1].GetComponentInChildren<SpriteRenderer>().color = Player2_color;
                HeroPool_P2[HeroPool_P2.Count - 1].GetComponentInChildren<SpriteRenderer>().flipX = true;
                HeroPool_P2[HeroPool_P2.Count - 1].tag = "HeroP2";
            }
        }
    }

    public void SetHeroInfo(MainClass main_class, int hp, int damage, int init, string ability_name)
    {
        string _hero_name = "";

        switch(main_class)
        {
            case MainClass.Scout:
                _hero_name = "Scout";
                break;
            case MainClass.Warrior:
                _hero_name = "Warrior";
                break;
            case MainClass.Mage:
                _hero_name = "Mage";
                break;
        }

        hero_name.text = _hero_name;
        hero_hp.text = hp.ToString();
        hero_dmg.text = damage.ToString();
        hero_init.text = init.ToString();
        hero_abil.text = ability_name;
    }

    private void PlaceDraftedUnits()
    {
        int column = 0;

        //instantiate heroes in the hero list in the grid system
        for (int i = 0; i < HeroList_P1.Count; i++)
        {
            Hero _hero = HeroList_P1[i].GetComponent<Hero>();

            //check the class and select the column
            switch (_hero.main_class)
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
            while (_hero.isDrafted)
            {
                //select random y
                int random_y = Random.Range(0, 3);

                GridTile _tile = Grid_P1.Grid[column, random_y].GetComponent<GridTile>();

                if (!_tile.isOccupied)
                {
                    int tile_x = _tile.pos_grid_x;
                    int tile_y = _tile.pos_grid_y;

                    //remove unit from pool list                                                                      
                    HeroPool_P1.Remove(HeroList_P1[i]);

                    //place unit and set spawnpoint
                    hero_spawnpoint = Grid_P1.Grid[tile_x, tile_y].transform.position;
                    HeroList_P1[i].transform.position = hero_spawnpoint;

                    //disable drafted visual
                    _hero.SetDrafted(false);
                    
                    //bool for the grid tile gets set to true so that no other unit can be spawned on top at the same time
                    Grid_P1.Grid[tile_x, tile_y].GetComponent<GridTile>().isOccupied = true;

                    _hero.x_position_grid = tile_x;
                    _hero.y_position_grid = tile_y;

                    //enable ui text and images
                    _hero.SetUI(true);
                }                
            }            
        }

        for (int i = 0; i < HeroList_P2.Count; i++)
        {
            Hero _hero = HeroList_P2[i].GetComponent<Hero>();

            //check the class and select the column
            switch (_hero.main_class)
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
            while (_hero.isDrafted)
            {
                //select random y
                int random_y = Random.Range(0, 3);

                GridTile _tile = Grid_P2.Grid[column, random_y].GetComponent<GridTile>();

                if (!_tile.isOccupied)
                {
                    int tile_x = _tile.pos_grid_x;
                    int tile_y = _tile.pos_grid_y;

                    //remove unit from pool list                                                                      
                    HeroPool_P2.Remove(HeroList_P2[i]);

                    //place unit and set spawnpoint
                    hero_spawnpoint = Grid_P2.Grid[tile_x, tile_y].transform.position;
                    HeroList_P2[i].transform.position = hero_spawnpoint;

                    //disable drafted visual
                    _hero.SetDrafted(false);
                    
                    //bool for the grid tile gets set to true so that no other unit can be spawned on top at the same time
                    Grid_P2.Grid[tile_x, tile_y].GetComponent<GridTile>().isOccupied = true;

                    _hero.x_position_grid = tile_x;
                    _hero.y_position_grid = tile_y;
                    
                    //enable ui text and images
                    _hero.SetUI(true);
                }
            }
        }

        //destroy pool lists
        foreach(GameObject g in HeroPool_P1)
        {
            Destroy(g);
        }
        foreach (GameObject g in HeroPool_P2)
        {
            Destroy(g);
        }

        HeroPool_P1.Clear();
        HeroPool_P2.Clear();
    }
    
    private void ResetGame()
    {
        //destroy all heros
        foreach (GameObject g in HeroList_P1)
        {
            Destroy(g);
        }
        foreach (GameObject g in HeroList_P2)
        {
            Destroy(g);
        }

        //destroy all heroes in pools
        foreach(GameObject g in HeroPool_P1)
        {
            Destroy(g);
        }
        foreach(GameObject g in HeroPool_P2)
        {
            Destroy(g);
        }

        //Reset bools in grid tiles
        foreach (GameObject g in Grid_P1.Grid)
        {
            g.GetComponent<GridTile>().isOccupied = false;
            g.GetComponent<GridTile>().SetMovementRing(false);
        }
        foreach (GameObject g in Grid_P2.Grid)
        {
            g.GetComponent<GridTile>().isOccupied = false;
            g.GetComponent<GridTile>().SetMovementRing(false);
        }

        //clear lists
        HeroList_P1.Clear();
        HeroList_P2.Clear();
        Player.instance.ClearActionIcons();
        Player.instance.ClearActionsList();
                
        //reset enums
        SetCurrentGameState(GameState.Game);
        SetPlayerTurn(PlayerTurn.Player1);
    }

    public void SetPlayerTurn(PlayerTurn active_turn)
    {
        switch(active_turn)
        {
            case PlayerTurn.Player1:
                player_turn_text.text = "Player 1";
                player_turn_text.color = Player1_color;
                break;
            case PlayerTurn.Player2:
                player_turn_text.text = "Player 2";
                player_turn_text.color = Player2_color;
                                
                switch (CurrentPhase)
                {
                    case Phase.PlanPhase:

                        foreach (GameObject g in HeroList_P1)
                        {
                            Hero _hero = g.GetComponent<Hero>();

                            _hero.SetUI(false);
                            _hero.SetAction(false);
                        }

                        P1_actions.text = "";
                        P2_actions.text = "Actions: 0/" + Player.instance.max_actions;
                        break;
                }
                

                //fade animation ui
                SetAnimationUI(true, CurrentPhase, CurrentTurn);

                break;
        }
        
        CurrentTurn = active_turn;
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
                SetAnimationUI(true, Phase.DraftPhase, CurrentTurn);

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

                Player.instance.ClearActionIcons();

                //check if turn is player 1s turn
                SetPlayerTurn(PlayerTurn.Player1);

                //fade in/out animation ui
                SetAnimationUI(true, Phase.PlanPhase, CurrentTurn);
                
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

                Player.instance.p1_actions = 0;
                Player.instance.p2_actions = 0;

                foreach(GameObject hero in HeroList_P1)
                {
                    Hero _hero = hero.GetComponent<Hero>();
                    _hero.SetUI(true);
                }
                foreach (GameObject hero in HeroList_P2)
                {
                    Hero _hero = hero.GetComponent<Hero>();
                    _hero.SetUI(true);
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
                SetAnimationUI(true, Phase.ResolvePhase, CurrentTurn);

                //disable plan ui
                PlanUI.SetActive(false);

                //enable resolve ui
                ResolveUI.SetActive(true);

                //set action ended to true so that animations can play
                action_ended = true;

                //hide stats on heroes and green check                
                foreach(GameObject g in HeroList_P2)
                {
                    Hero _hero = g.GetComponent<Hero>();

                    _hero.SetUI(false);
                    _hero.SetAction(false);
                }

                phase_text.text = "Resolve Phase";
                StartCoroutine(Player.instance.ResolveActions());
                break;
        }

        //set current phase to active phase
        CurrentPhase = active_phase;
    }

    public IEnumerator ClearKilledHeroes()
    {
        foreach(GameObject hero in HeroList_P1)
        {
            if(hero.GetComponent<Hero>().Healthpoints <= 0)
            {
                Destroy(hero);
            }
        }

        foreach(GameObject hero in HeroList_P2)
        {
            if (hero.GetComponent<Hero>().Healthpoints <= 0)
            {
                Destroy(hero);
            }
        }

        yield return new WaitForSeconds(1f);
        //clean up lists, need small waiting buffer to see if heroes have died or not, could also check if still heroes in list, their hp is 0 or less
        CleanLists();

        //check if either player has no more heroes
        if(HeroList_P1.Count == 0 || HeroList_P2.Count == 0)
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
        HeroList_P1.RemoveAll(item => item == null);
        HeroList_P2.RemoveAll(item => item == null);
    }

    private void SetEndScreen()
    {
        //enable end ui
        EndUI.SetActive(true);

        //set winners name
        if(HeroList_P2.Count == 0)
        {
            Winner_playername_text.text = "Player 1";
            Winner_playername_text.color = Player1_color;
        }
        else if(HeroList_P1.Count == 0)
        {
            Winner_playername_text.text = "Player 2";
            Winner_playername_text.color = Player2_color;
        }
    }

    public void OnDraftReady()
    {
        //check if both players selected the max amount of units to continue to planning phase
        if (HeroList_P1.Count == max_amount_units && HeroList_P2.Count == max_amount_units)
        {
            //temp SetCurrentPhase(Phase.PlanPhase);
        }

        SetCurrentPhase(Phase.PlanPhase);
    }

    public void OnPlanReady()
    {
        switch(CurrentTurn)
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
        Player.instance.ClearActionIcons();
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

[System.Serializable]
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