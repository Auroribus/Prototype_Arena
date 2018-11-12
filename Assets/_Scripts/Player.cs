using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour {

    #region variables

    public static Player instance;

    public int PlayerNumber = 1;

    [System.NonSerialized] public GameObject SelectedHero;

    //player specific variables that chance depending on the turn
    private string target_tag = "";
    private string own_tag = "";
    private List<GameObject> enemy_list;
    private GridParent player_grid;
    private int player_actions = 0;

    //action based variables
    public int max_actions = 3;
    public int p1_actions = 0;
    public int p2_actions = 0;

    public List<Action> list_of_actions = new List<Action>();

    private GameObject Plan_list;
    public GameObject action_icon_prefab;
    public List<Sprite> action_icon_sprites = new List<Sprite>();
    private List<GameObject> action_icons_list = new List<GameObject>();

    #endregion

    private void Awake()
    {
        if (instance == null)
            instance = this;

        Plan_list = GameObject.Find("Plan List");
    }
    
    private void Update () {
        MouseControl();
	}
        
    private void MouseControl()
    {       
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                switch (GameManager.instance.CurrentPhase)
                {
                    case Phase.DraftPhase:
                        DraftPhase(hit);
                        break;
                    case Phase.PlanPhase:
                        SetPlayerTurnActionPhase();
                        PlanPhase(hit);
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
                        GameManager.instance.P1_drafted.text = "Drafted: " + GameManager.instance.HeroList_P1.Count + "/" + GameManager.instance.max_amount_units;
                    }
                }
            }
            else
            {
                hit.transform.GetComponent<Hero>().SetDrafted(false);
                GameManager.instance.HeroList_P1.Remove(hit.transform.gameObject);
                GameManager.instance.P1_drafted.text = "Drafted: " + GameManager.instance.HeroList_P1.Count + "/" + GameManager.instance.max_amount_units;
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
                        GameManager.instance.P2_drafted.text = "Drafted: " + GameManager.instance.HeroList_P2.Count + "/" + GameManager.instance.max_amount_units;
                    }
                }
            }
            else
            {
                hit.transform.GetComponent<Hero>().SetDrafted(false);
                GameManager.instance.HeroList_P2.Remove(hit.transform.gameObject);
                GameManager.instance.P2_drafted.text = "Drafted: " + GameManager.instance.HeroList_P2.Count + "/" + GameManager.instance.max_amount_units;
            }
        }
    }

    private void PlanPhase(RaycastHit2D hit)
    {
        //see if hit is a unit belonging to this player
        //check if the unit already performing an action
        if (hit.collider.tag == own_tag && player_actions < max_actions && !hit.transform.GetComponent<Hero>().hasAction)
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

                //melee can only hit the closest enemy in the same lane
                foreach (GameObject hero in enemy_list)
                {
                    if (hero.GetComponent<Hero>().x_position_grid == _max && hero.GetComponent<Hero>().y_position_grid == selected_hero.GetComponent<Hero>().y_position_grid)
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
        else if (SelectedHero != null && hit.collider.tag == target_tag && player_actions < max_actions)
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

                    //increment attack
                    IncrementActions(+1);

                    //unselect selected hero
                    UnselectHero();
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

                    //add action to the list
                    list_of_actions.Add(new Action(
                                    SelectedHero,
                                    GameManager.instance.CurrentTurn,
                                    ActionType.attack,
                                    target_heroes
                                ));

                    //increment attack
                    IncrementActions(+1);

                    //unselect selected hero
                    UnselectHero();
                }
            }
        }
        //moving
        else if (SelectedHero != null && hit.collider.tag == "Tile" && player_actions < max_actions)
        {
            if (hit.transform.GetComponent<GridTile>().can_move_here)
            {
                if (!SelectedHero.GetComponent<Hero>().move_hero)
                {
                    //add movement action to the list
                    list_of_actions.Add(new Action(
                                    SelectedHero,
                                    GameManager.instance.CurrentTurn,
                                    ActionType.movement,
                                    hit.transform.gameObject
                                ));

                    //increment attack
                    IncrementActions(+1);

                    //deselect hero
                    UnselectHero();
                }
            }
        }

    }

    private void SetPlayerTurnActionPhase()
    {
        switch (GameManager.instance.CurrentTurn)
        {
            case PlayerTurn.Player1:
                own_tag = "HeroP1";
                target_tag = "HeroP2";
                enemy_list = GameManager.instance.HeroList_P2;
                player_grid = GameManager.instance.Grid_P1;
                player_actions = p1_actions;
                break;
            case PlayerTurn.Player2:
                own_tag = "HeroP2";
                target_tag = "HeroP1";
                enemy_list = GameManager.instance.HeroList_P1;
                player_grid = GameManager.instance.Grid_P2;
                player_actions = p2_actions;
                break;
        }
    }

    public IEnumerator ResolveActions()
    {
        foreach(Action action in list_of_actions)
        {
            //check if the hero doing the action is still alive
            if(action.selected_hero != null && action.selected_hero.GetComponent<Hero>().Healthpoints > 0)
            {
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
                            {
                                //check if hero is warrior and target not is warrior for double damage
                                if(action.selected_hero.GetComponent<Hero>().main_class == MainClass.Warrior && action.single_target.GetComponent<Hero>().main_class != MainClass.Warrior)
                                {
                                    int damage = action.selected_hero.GetComponent<Hero>().Damage * 2;
                                    action.single_target.GetComponent<Hero>().TakeDamage(damage);
                                }
                                //scout attack
                                else if(action.selected_hero.GetComponent<Hero>().main_class == MainClass.Scout)
                                {
                                    //instance arrow from hero to target
                                    action.selected_hero.GetComponent<Hero>().LaunchProjectile(action.single_target, action.selected_hero.GetComponent<Hero>().Damage, action.player);
                                }
                                //otherwise normal damage applies
                                else
                                {
                                    action.single_target.GetComponent<Hero>().TakeDamage(action.selected_hero.GetComponent<Hero>().Damage);
                                }
                                
                                //set checkmark to green on action prefab                            
                                action_icons_list[list_of_actions.IndexOf(action)].GetComponent<ActionPrefab>().SetCheckmark(true, Color.green);
                            }
                            //else the target is already dead
                            else
                            {
                                //set checkmark to red on action prefab                            
                                action_icons_list[list_of_actions.IndexOf(action)].GetComponent<ActionPrefab>().SetCheckmark(true, Color.red);
                            }

                            yield return new WaitForSeconds(1f);
                        }
                        else if (action.targets.Count > 0)
                        {
                            foreach (GameObject target in action.targets)
                            {
                                //if main class is mage, check if the targeted enemy hasn't moved
                                if (action.selected_hero.GetComponent<Hero>().main_class == MainClass.Mage)
                                {
                                    //check if target is alive and in the same row
                                    if (target != null && target.GetComponent<Hero>().y_position_grid == action.selected_hero.GetComponent<Hero>().y_position_grid)
                                    {
                                        target.GetComponent<Hero>().TakeDamage(action.selected_hero.GetComponent<Hero>().Damage);
                                    }
                                }
                                else
                                {
                                    if (target != null)
                                    {
                                        target.GetComponent<Hero>().TakeDamage(action.selected_hero.GetComponent<Hero>().Damage);
                                    }
                                }
                            }

                            //set checkmark to green on action prefab
                            action_icons_list[list_of_actions.IndexOf(action)].GetComponent<ActionPrefab>().SetCheckmark(true, Color.green);

                            yield return new WaitForSeconds(1f);
                        }
                        else
                        {
                            //set checkmark to red on action prefab
                            action_icons_list[list_of_actions.IndexOf(action)].GetComponent<ActionPrefab>().SetCheckmark(true, Color.red);
                            
                            yield return new WaitForSeconds(1f);
                        }
                        break;
                    case ActionType.ability:
                        break;
                    case ActionType.movement:

                        //check if target tile is not occupied by other movement
                        if (!action.single_target.GetComponent<GridTile>().isOccupied)
                        {
                            //set old occupied tile to no longer occupied
                            switch (action.player)
                            {
                                case PlayerTurn.Player1:
                                    GameManager.instance.Grid_P1.Grid[action.selected_hero.GetComponent<Hero>().x_position_grid,
                                        action.selected_hero.GetComponent<Hero>().y_position_grid]
                                        .GetComponent<GridTile>().isOccupied = false;
                                    break;
                                case PlayerTurn.Player2:
                                    GameManager.instance.Grid_P2.Grid[action.selected_hero.GetComponent<Hero>().x_position_grid,
                                        action.selected_hero.GetComponent<Hero>().y_position_grid]
                                        .GetComponent<GridTile>().isOccupied = false;
                                    break;
                            }

                            //move hero
                            action.selected_hero.GetComponent<Hero>().target_position = action.single_target.transform.position;
                            action.selected_hero.GetComponent<Hero>().move_hero = true;

                            //update heros position on grid
                            action.selected_hero.GetComponent<Hero>().x_position_grid = action.single_target.transform.GetComponent<GridTile>().pos_grid_x;
                            action.selected_hero.GetComponent<Hero>().y_position_grid = action.single_target.transform.GetComponent<GridTile>().pos_grid_y;

                            //set new tile as occupied
                            action.single_target.transform.GetComponent<GridTile>().isOccupied = true;
                            action.single_target.transform.GetComponent<GridTile>().SetMovementRing(false);

                            //set checkmark to green on action prefab
                            action_icons_list[list_of_actions.IndexOf(action)].GetComponent<ActionPrefab>().SetCheckmark(true, Color.green);

                            yield return new WaitForSeconds(1f);
                        }
                        //tile already occupied
                        else
                        {
                            //set checkmark to red on action prefab
                            action_icons_list[list_of_actions.IndexOf(action)].GetComponent<ActionPrefab>().SetCheckmark(true, Color.red);

                            yield return new WaitForSeconds(1f);
                        }
                        break;
                }
            }
            else
            {
                //set checkmark to green on action prefab
                action_icons_list[list_of_actions.IndexOf(action)].GetComponent<ActionPrefab>().SetCheckmark(true, Color.red);

                yield return new WaitForSeconds(1f);
            }
        }

        //clear list of actions after all actions have been resolved
        list_of_actions.Clear();

        GameManager.instance.ClearKilledHeroes();
    }

    private void IncrementActions(int value)
    {
        switch (GameManager.instance.CurrentTurn)
        {
            case PlayerTurn.Player1:
                p1_actions += value;
                GameManager.instance.P1_actions.text = "Actions: " + p1_actions + "/" + max_actions;

                break;

            case PlayerTurn.Player2:
                p2_actions += value;
                GameManager.instance.P2_actions.text = "Actions: " + p2_actions + "/" + max_actions;

                break;
        }

        DisplayActionList();
    }

    private void UnselectHero()
    {
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

    private void DisplayActionList()
    {
        //sort actions list by initiative descending
        list_of_actions = list_of_actions.OrderByDescending(action => action.initiative).ToList();

        ClearActionIcons();

        int position_y = 0;

        foreach(Action action in list_of_actions)
        {
            //set instance position
            Vector2 icon_position = new Vector2();
            icon_position.x = Plan_list.transform.position.x;
            icon_position.y = Plan_list.transform.position.y - position_y;

            //instance action prefab
            action_icons_list.Add(Instantiate(action_icon_prefab, icon_position, Quaternion.identity, Plan_list.transform));
            
            //increment position y
            position_y++;

            SpriteRenderer sRend = action_icons_list[action_icons_list.Count - 1].GetComponent<SpriteRenderer>();

            //set sprite icon
            switch (action.action_type)
            {
                case ActionType.attack:
                    sRend.sprite = action_icon_sprites[0];
                    break;
                case ActionType.ability:
                    sRend.sprite = action_icon_sprites[1];
                    break;
                case ActionType.movement:
                    sRend.sprite = action_icon_sprites[2];
                    break;
            }

            //set sprite icon color
            switch(action.player)
            {
                case PlayerTurn.Player1:
                    sRend.color = Color.blue;
                    break;
                case PlayerTurn.Player2:
                    sRend.color = Color.red;
                    break;
            }
            
        }
    }

    public void ClearActionIcons()
    {
        foreach (GameObject action in action_icons_list)
        {
            Destroy(action);
        }
        action_icons_list.Clear();
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

        //set hero has action to true
        selected_hero.GetComponent<Hero>().SetAction(true);
    }

    public Action(GameObject _selected_hero, PlayerTurn _player, ActionType _action, List<GameObject> _targets)
    {
        selected_hero = _selected_hero;
        player = _player;
        action_type = _action;
        initiative = selected_hero.GetComponent<Hero>().Initiative;
        targets = _targets;

        //set hero has action to true
        selected_hero.GetComponent<Hero>().SetAction(true);
    }
}