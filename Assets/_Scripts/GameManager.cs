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

public enum HeroType
{
    Melee,
    Ranged,
    Magic
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
    public GameObject ArcherPrefab;
    public GameObject KnightPrefab;
    public GameObject MagePrefab;

    //Lists of all the hero units per player that are on the board
    [System.NonSerialized] public List<GameObject> HeroList_P1 = new List<GameObject>();
    [System.NonSerialized] public List<GameObject> HeroList_P2 = new List<GameObject>();

    //temp spawn point for randomly spawning heros
    private Vector2 hero_spawnpoint;

    private Text phase_text;
    private Text turn_text;

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

                //hero gets added to the list for P1
                int random = Random.Range(0, 3);
                switch(random)
                {
                    case 0:
                        HeroList_P1.Add(Instantiate(ArcherPrefab, hero_spawnpoint, Quaternion.identity, Grid_P1.transform));
                        break;
                    case 1:
                        HeroList_P1.Add(Instantiate(KnightPrefab, hero_spawnpoint, Quaternion.identity, Grid_P1.transform));
                        break;
                    case 2:
                        HeroList_P1.Add(Instantiate(MagePrefab, hero_spawnpoint, Quaternion.identity, Grid_P1.transform));
                        break;
                }

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
                
                int random = Random.Range(0, 3);
                switch (random)
                {
                    case 0:
                        HeroList_P2.Add(Instantiate(ArcherPrefab, hero_spawnpoint, Quaternion.identity, Grid_P2.transform));
                        break;
                    case 1:
                        HeroList_P2.Add(Instantiate(KnightPrefab, hero_spawnpoint, Quaternion.identity, Grid_P2.transform));
                        break;
                    case 2:
                        HeroList_P2.Add(Instantiate(MagePrefab, hero_spawnpoint, Quaternion.identity, Grid_P2.transform));
                        break;
                }

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
