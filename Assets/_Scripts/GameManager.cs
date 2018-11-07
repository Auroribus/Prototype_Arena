using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    SpawnPhase,
    ActionPhase,
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

public class GameManager : MonoBehaviour {

    //static reference which can be accessed in all other scripts by calling GameManager.instance
    public static GameManager instance;

    //References to enums
    public GameState CurrentState = GameState.Game;
    public Phase CurrentPhase = Phase.SpawnPhase;
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

    //temp spawn point for randomly spawning heros
    private Vector2 hero_spawnpoint;

    private Text phase_text;
    private Text turn_text;

    public List<HeroBase> Heroes = new List<HeroBase>();

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

        Grid_P2 = GameObject.Find("GridParent P2").GetComponent<GridParent>();
        //flip grid parent 2, so that the lane orientation is the same as grid p1
        Grid_P2.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        Grid_P1 = GameObject.Find("GridParent P1").GetComponent<GridParent>();

        phase_text = GameObject.Find("Phase").GetComponent<Text>();
        turn_text = GameObject.Find("Turn").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        
        DebugKeyboard();		
	}

    //For testing/debugging only
    private void DebugKeyboard()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
    }

    //Function that randomly spawns 5 units for each player
    private void SpawnHeroUnits()
    {
        //if the hero count is lower than the spawn amount, spawn unit
        while(HeroList_P1.Count < max_amount_units)
        {
            //randomly generate position for unit to spawn at
            int random_x = Random.Range(0, Grid_P1.rows);
            int random_y = Random.Range(0, Grid_P1.columns);

            //check if the grid tile is not already occupied by another unit
            if (!Grid_P1.Grid[random_x, random_y].GetComponent<GridTile>().isOccupied)
            {
                //set the hero spawn point equal to the grid tile
                hero_spawnpoint = Grid_P1.Grid[random_x, random_y].transform.position;

                //base the unit that gets spawned, on which lane has been selected to spawn a unit on

                int warrior = 0;
                int mage = 0;
                int scout = 0;

                //random_x = the lane
                //need to find the index of the hero type for each lane from the Heroes list, this will be changed later when we
                //change to the proper picking of the heroes
                foreach (HeroBase h in Heroes)
                {
                    if (h.Main_class == MainClass.Warrior)
                    {
                        warrior = Heroes.IndexOf(h);
                    }
                    else if (h.Main_class == MainClass.Scout)
                    {
                        scout = Heroes.IndexOf(h);
                    }
                    else if (h.Main_class == MainClass.Mage)
                    {
                        mage = Heroes.IndexOf(h);
                    }
                }

                //lane number is the lane in which the unit gets spawned, which determines which type of unit will be spawned
                int lane_number = 0;

                switch (random_x)
                {
                    case 0: //mage
                        lane_number = mage;
                        break;
                    case 1: //scout
                        lane_number = scout;
                        break;
                    case 2: //warrior
                        lane_number = warrior;
                        break;
                }

                HeroList_P1.Add(Instantiate(HeroPrefab, hero_spawnpoint, Quaternion.identity, Grid_P1.transform));
                Hero hero = HeroList_P1[HeroList_P1.Count - 1].GetComponent<Hero>();

                //assisgn hero specifics based on the hero base presets
                hero.Healthpoints = Heroes[lane_number].HealthPoints;
                hero.Damage = Heroes[lane_number].Damage;
                hero.Initiative = Heroes[lane_number].Initiative;
                hero.GetComponent<SpriteRenderer>().sprite = Heroes[lane_number].Hero_sprite;
                hero.main_class = Heroes[lane_number].Main_class;
                //set sub class once sub classes are implemented
                
                //bool for the grid tile gets set to true so that no other unit can be spawned on top at the same time
                Grid_P1.Grid[random_x, random_y].GetComponent<GridTile>().isOccupied = true;

                HeroList_P1[HeroList_P1.Count - 1].GetComponent<Hero>().x_position_grid = random_x;
                HeroList_P1[HeroList_P1.Count - 1].GetComponent<Hero>().y_position_grid = random_y;

                //Player specific   
                //color each sprite blue for P1
                HeroList_P1[HeroList_P1.Count - 1].GetComponent<SpriteRenderer>().color = Color.blue;
                HeroList_P1[HeroList_P1.Count - 1].tag = "HeroP1";
            }
        }

        while (HeroList_P2.Count < max_amount_units)
        {
            int random_x = Random.Range(0, Grid_P2.rows);
            int random_y = Random.Range(0, Grid_P2.columns);

            if (!Grid_P2.Grid[random_x, random_y].GetComponent<GridTile>().isOccupied)
            {
                hero_spawnpoint = Grid_P2.Grid[random_x, random_y].transform.position;

                //lanes
                //2 = front/warrior
                //1 = middle/scout
                //0 = back/mage

                int warrior = 0;
                int mage = 0;
                int scout = 0;
                
                //random_x = the lane
                //need to find the index of the hero type for each lane
                foreach(HeroBase h in Heroes)
                {
                    if(h.Main_class == MainClass.Warrior)
                    {
                        warrior = Heroes.IndexOf(h);
                    }
                    else if(h.Main_class == MainClass.Scout)
                    {
                        scout = Heroes.IndexOf(h);
                    }
                    else if(h.Main_class == MainClass.Mage)
                    {
                        mage = Heroes.IndexOf(h);
                    }
                }
                
                int lane_number = 0;

                switch (random_x)
                {
                    case 0: //mage
                        lane_number = mage;
                        break;
                    case 1: //scout
                        lane_number = scout;
                        break;
                    case 2: //warrior
                        lane_number = warrior;
                        break;
                }
                
                //instantiate hero
                HeroList_P2.Add(Instantiate(HeroPrefab, hero_spawnpoint, Quaternion.identity, Grid_P2.transform));
                Hero hero = HeroList_P2[HeroList_P2.Count - 1].GetComponent<Hero>();

                //assisgn hero specifics based on the hero base presets
                hero.Healthpoints = Heroes[lane_number].HealthPoints;
                hero.Damage = Heroes[lane_number].Damage;
                hero.Initiative = Heroes[lane_number].Initiative;
                hero.GetComponent<SpriteRenderer>().sprite = Heroes[lane_number].Hero_sprite;
                hero.main_class = Heroes[lane_number].Main_class;
                //set sub class once sub classes are implemented
                
                Grid_P2.Grid[random_x, random_y].GetComponent<GridTile>().isOccupied = true;

                HeroList_P2[HeroList_P2.Count - 1].GetComponent<Hero>().x_position_grid = random_x;
                HeroList_P2[HeroList_P2.Count - 1].GetComponent<Hero>().y_position_grid = random_y;

                //Player specific
                //Flip each sprite and color red for p2
                HeroList_P2[HeroList_P2.Count - 1].GetComponent<SpriteRenderer>().flipX = true;
                HeroList_P2[HeroList_P2.Count - 1].GetComponent<SpriteRenderer>().color = Color.red;
                HeroList_P2[HeroList_P2.Count - 1].tag = "HeroP2";
            }
        }

    }

    private void StartGame()
    {
        if(Random.Range(0,2) == 0)
            SetPlayerTurn(PlayerTurn.Player1);        
        else
            SetPlayerTurn(PlayerTurn.Player2);

        SpawnHeroUnits();
        
        SetCurrentPhase(Phase.ActionPhase);
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
    }

    private void SetPlayerTurn(PlayerTurn active_turn)
    {
        CurrentTurn = active_turn;

        switch(CurrentTurn)
        {
            case PlayerTurn.Player1:
                turn_text.text = "Player 1";
                turn_text.color = Color.blue;
                break;
            case PlayerTurn.Player2:
                turn_text.text = "Player 2";
                turn_text.color = Color.red;
                break;
        }
    }

    private void SetCurrentGameState(GameState active_state)
    {
        CurrentState = active_state;

        switch (CurrentState)
        {
            case GameState.Menu:
                break;
            case GameState.Game:
                break;
            case GameState.Paused:
                break;
            case GameState.Ended:
                break;
        }
    }

    private void SetCurrentPhase(Phase active_phase)
    {
        CurrentPhase = active_phase;

        switch(CurrentPhase)
        {
            case Phase.IdlePhase:
                break;
            case Phase.SpawnPhase:
                phase_text.text = "Spawn Phase";
                break;
            case Phase.ActionPhase:
                phase_text.text = "Action Phase";
                break;
            case Phase.ResolvePhase:
                phase_text.text = "Resolve Phase";
                break;
        }
    }

    public void CleanLists()
    {
        HeroList_P1.RemoveAll(item => item == null);
        HeroList_P2.RemoveAll(item => item == null);
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
    public Sprite Hero_sprite;
}