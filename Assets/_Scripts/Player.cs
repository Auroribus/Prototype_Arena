using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour {

    public static Player instance;

    public int PlayerNumber = 1;
    public int ActionsLeft = 3;

    [System.NonSerialized] public GameObject SelectedHero;

    string target_tag = "";
    string own_tag = "";
    List<GameObject> enemy_list;
    GridParent player_grid;
    int melee_range = 2;

    private int max_actions = 3;
    public int p1_actions = 0;
    public int p2_actions = 0;

    public List<Action> list_of_actions = new List<Action>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    // Update is called once per frame
    void Update () {
        MouseControl();
	}
        
    private void MouseControl()
    {       
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                //Debug.Log(hit.collider.tag);

                switch (GameManager.instance.CurrentPhase)
                {
                    case Phase.DraftPhase:
                        DraftPhase(hit);
                        break;
                    case Phase.ActionPhase:
                        SetPlayerTurnActionPhase();
                        ActionPhase(hit);
                        break;
                }
            }
        }
    }
    
    private void DraftPhase(RaycastHit2D hit)
    {
        if(hit.collider.tag == "HeroP1")
        {
            if(!hit.transform.GetComponent<Hero>().isDrafted)
            {
                if (GameManager.instance.HeroList_P1.Count < GameManager.instance.max_amount_units)
                {
                    int amount_of_class = 0;
                    //check if not already 3 of main class
                    foreach(GameObject hero in GameManager.instance.HeroList_P1)
                    {
                        if (hero.GetComponent<Hero>().main_class == hit.transform.GetComponent<Hero>().main_class)
                            amount_of_class++;
                    }

                    if (amount_of_class < 3)
                    {
                        hit.transform.gameObject.GetComponent<Hero>().SetDrafted(true);
                        GameManager.instance.HeroList_P1.Add(hit.transform.gameObject);
                    }
                }
            }
            else
            {
                hit.transform.GetComponent<Hero>().SetDrafted(false);
                GameManager.instance.HeroList_P1.Remove(hit.transform.gameObject);
            }            
        }
        else if(hit.collider.tag == "HeroP2")
        {
            if (!hit.transform.GetComponent<Hero>().isDrafted)
            {
                if (GameManager.instance.HeroList_P2.Count < GameManager.instance.max_amount_units)
                {
                    int amount_of_class = 0;
                    //check if not already 3 of main class
                    foreach (GameObject hero in GameManager.instance.HeroList_P2)
                    {
                        if (hero.GetComponent<Hero>().main_class == hit.transform.GetComponent<Hero>().main_class)
                            amount_of_class++;
                    }

                    if (amount_of_class < 3)
                    {
                        hit.transform.GetComponent<Hero>().SetDrafted(true);
                        GameManager.instance.HeroList_P2.Add(hit.transform.gameObject);
                    }
                }
            }
            else
            {
                hit.transform.GetComponent<Hero>().SetDrafted(false);
                GameManager.instance.HeroList_P2.Remove(hit.transform.gameObject);
            }
        }
    }

    private void ActionPhase(RaycastHit2D hit)
    {
        //Debug.Log("turn: " + GameManager.instance.CurrentTurn);
        //see if hit is a unit belonging to this player
        if (hit.collider.tag == own_tag)
        {
            if (SelectedHero != null)
                SelectedHero.GetComponent<Hero>().SetSelected(false);

            SelectedHero = hit.transform.gameObject;
            SelectedHero.GetComponent<Hero>().SetSelected(true);

            Hero selected_hero = SelectedHero.GetComponent<Hero>();

            GameManager.instance.CleanLists();
            foreach (GameObject hero in enemy_list)
            {
                hero.GetComponent<Hero>().SetTargeted(false);
            }

            MainClass main_class = selected_hero.main_class;

            //set target on all player 2 heros for ranged
            if (main_class == MainClass.Scout)
            {
                //ranged can hit any of the enemies heroes
                foreach (GameObject hero in enemy_list)
                {
                    hero.GetComponent<Hero>().SetTargeted(true);
                }
            }
            else if (main_class == MainClass.Warrior)
            {
                //temp list to get the lanes from the hero's
                List<int> _max_list = new List<int>();

                //put all int values into the max list
                foreach (GameObject hero in enemy_list)
                {
                    _max_list.Add(hero.GetComponent<Hero>().x_position_grid);
                }

                //get the max value, meaning the closest lane that has a unit on it
                int _max = Mathf.Max(_max_list.ToArray());

                //melee can only hit the closest lane of enemies
                foreach (GameObject hero in enemy_list)
                {
                    if (hero.GetComponent<Hero>().x_position_grid == _max )//&& hero.GetComponent<Hero>().y_position_grid == selected_hero.GetComponent<Hero>().y_position_grid)
                        hero.GetComponent<Hero>().SetTargeted(true);
                }
            }
            else if (main_class == MainClass.Mage)
            {
                //hit a whole lane in a straight line, horizontaly
                foreach (GameObject hero in enemy_list)
                {
                    if (hero.GetComponent<Hero>().y_position_grid == selected_hero.y_position_grid)
                        hero.GetComponent<Hero>().SetTargeted(true);
                }
            }

            //set movement ring on tiles in same lane to active
            int x = selected_hero.x_position_grid;
            int y = selected_hero.y_position_grid;
            player_grid.SetMovementRings(x, y);
        }
        //attacking
        else if (SelectedHero != null && hit.collider.tag == target_tag)
        {
            if (hit.transform.GetComponent<Hero>().isTargeted)
            {
                //non magical attacks that only target one hero
                if (SelectedHero.GetComponent<Hero>().main_class != MainClass.Mage)
                {
                    //hit.transform.GetComponent<Hero>().TakeDamage(SelectedHero.GetComponent<Hero>().Damage);
                    //add action to list
                    list_of_actions.Add(new Action(
                            SelectedHero,
                            GameManager.instance.CurrentTurn, 
                            ActionType.attack,
                            hit.transform.gameObject
                        ));

                    //unselect selected hero
                    SelectedHero.GetComponent<Hero>().SetSelected(false);
                    SelectedHero = null;

                    GameManager.instance.CleanLists();
                    foreach (GameObject hero in enemy_list)
                    {
                        hero.GetComponent<Hero>().SetTargeted(false);
                    }
                    player_grid.SetMovementRings(-1,-1);
                }
                //magical attacks that target a whole row of heros
                else if (SelectedHero.GetComponent<Hero>().main_class == MainClass.Mage)
                {
                    GameManager.instance.CleanLists();
                    List<GameObject> target_heroes = new List<GameObject>();
                    foreach (GameObject hero in enemy_list)
                    {
                        if (hit.transform.GetComponent<Hero>().y_position_grid == hero.GetComponent<Hero>().y_position_grid)
                        {
                            //hero.GetComponent<Hero>().TakeDamage(SelectedHero.GetComponent<Hero>().Damage);
                            //add hero to list
                            target_heroes.Add(hero);
                        }
                    }

                    list_of_actions.Add(new Action(
                                    SelectedHero,
                                    GameManager.instance.CurrentTurn,
                                    ActionType.attack,
                                    target_heroes
                                ));

                    //unselect selected hero
                    SelectedHero.GetComponent<Hero>().SetSelected(false);
                    SelectedHero = null;

                    GameManager.instance.CleanLists();
                    foreach (GameObject hero in enemy_list)
                    {
                        hero.GetComponent<Hero>().SetTargeted(false);
                    }
                    player_grid.SetMovementRings(-1, -1);
                }
            }
        }
        //moving
        else if (SelectedHero != null && hit.collider.tag == "Tile")
        {
            if (hit.transform.GetComponent<GridTile>().can_move_here)
            {
                if (!SelectedHero.GetComponent<Hero>().move_hero)
                {
                    //set old occupied tile to no longer occupied
                    player_grid.Grid[SelectedHero.GetComponent<Hero>().x_position_grid, SelectedHero.GetComponent<Hero>().y_position_grid]
                        .GetComponent<GridTile>().isOccupied = false;

                    //move hero
                    SelectedHero.GetComponent<Hero>().target_position = hit.transform.position;
                    SelectedHero.GetComponent<Hero>().move_hero = true;

                    //update heros position on grid
                    SelectedHero.GetComponent<Hero>().x_position_grid = hit.transform.GetComponent<GridTile>().pos_grid_x;
                    SelectedHero.GetComponent<Hero>().y_position_grid = hit.transform.GetComponent<GridTile>().pos_grid_y;

                    //set new tile as occupied
                    hit.transform.GetComponent<GridTile>().isOccupied = true;
                    hit.transform.GetComponent<GridTile>().SetMovementRing(false);
                }
            }
        }

    }

    private void SetPlayerTurnActionPhase()
    {
        switch (GameManager.instance.CurrentTurn)
        {
            case PlayerTurn.Player1:
                PlayerNumber = 1;
                own_tag = "HeroP1";
                target_tag = "HeroP2";
                enemy_list = GameManager.instance.HeroList_P2;
                player_grid = GameManager.instance.Grid_P1;
                break;
            case PlayerTurn.Player2:
                PlayerNumber = 2;
                own_tag = "HeroP2";
                target_tag = "HeroP1";
                enemy_list = GameManager.instance.HeroList_P1;
                player_grid = GameManager.instance.Grid_P2;
                break;
        }
    }

    public IEnumerator ResolveActions()
    {
        //sort actions list by initiative descending
        list_of_actions = list_of_actions.OrderByDescending(action => action.initiative).ToList();

        foreach(Action action in list_of_actions)
        {
            //check if the hero doing the action is still alive
            if(action.selected_hero != null)
            {
                Debug.Log("initiative: " + action.initiative);

                switch (action.action_type)
                {
                    case ActionType.attack:
                        //deal damage to target
                        //amount of damage determined by seleted hero
                        //difference between one target and multiple
                        if (action.single_target != null)
                        {
                            //check if target is alive and hero that is doing the action is alive
                            if (action.single_target != null)
                                action.single_target.GetComponent<Hero>().TakeDamage(action.selected_hero.GetComponent<Hero>().Damage);

                            yield return new WaitForSeconds(1f);
                        }
                        else if (action.targets.Count > 0)
                        {
                            foreach (GameObject target in action.targets)
                            {
                                if (target != null)
                                    target.GetComponent<Hero>().TakeDamage(action.selected_hero.GetComponent<Hero>().Damage);
                            }

                            yield return new WaitForSeconds(1f);
                        }
                        break;
                    case ActionType.ability:
                        break;
                    case ActionType.movement:
                        break;
                }
            }
            else
            {
                //remove action from the list
            }
        }

        //clear list of actions after all actions have been resolved
        list_of_actions.Clear();
    }
}

public enum ActionType
{
    attack,
    ability,
    movement
}

public class Action
{
    //type of action, attack, ability, move
    //action initiative
    //target or targets
    public PlayerTurn player;
    public ActionType action_type;
    public int initiative;
    public GameObject selected_hero;
    public GameObject single_target;
    public List<GameObject> targets = new List<GameObject>();

    public Action(GameObject _selected_hero, PlayerTurn _player, ActionType _action, GameObject _single_target)
    {
        selected_hero = _selected_hero;
        player = _player;
        action_type = _action;
        initiative = selected_hero.GetComponent<Hero>().Initiative;
        single_target = _single_target;
    }

    public Action(GameObject _selected_hero, PlayerTurn _player, ActionType _action, List<GameObject> _targets)
    {
        selected_hero = _selected_hero;
        player = _player;
        action_type = _action;
        initiative = selected_hero.GetComponent<Hero>().Initiative;
        targets = _targets;
    }
}