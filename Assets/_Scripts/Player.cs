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
    private List<GameObject> allies_list;
    private GridParent player_grid;
    private int player_actions = 0;

    //action based variables
    public int max_actions = 3;
    public int p1_actions = 0;
    public int p2_actions = 0;

    public List<Action> list_of_actions = new List<Action>();
    //list of targets for ability
    List<GameObject> ability_targets = new List<GameObject>();

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
               
        if (Input.GetKeyDown(KeyCode.D))
        {
            DeselectHero();
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
                    Hero _hero = hit.transform.gameObject.GetComponent<Hero>();

                    int amount_of_class = 0;
                    //check if not already 3 of main class
                    foreach(GameObject hero in GameManager.instance.HeroList_P1)
                    {
                        if (hero.GetComponent<Hero>().main_class == _hero.main_class)
                            amount_of_class++;
                    }

                    if (amount_of_class < 3)
                    {
                        _hero.SetDrafted(true);
                        GameManager.instance.HeroList_P1.Add(_hero.transform.gameObject);
                        GameManager.instance.P1_drafted.text = "Drafted: " + GameManager.instance.HeroList_P1.Count + "/" + GameManager.instance.max_amount_units;

                        AbilityBase ability = _hero.HeroAbility;

                        string ability_text = 
                            "Effect: " + ability.Ability_effect + "\n" +
                            "Target: " + ability.Ability_target + "\n" +
                            "AoE: " + ability.Ability_aoe + "\n" +
                            "Strength: " + ability.strength + "\n" +
                            "Duration: " + ability.duration + "\n" +
                            "Delay: " + ability.delay;

                        GameManager.instance.SetDraftHeroStats(1, _hero.main_class, _hero.Healthpoints, _hero.Damage, _hero.Initiative, ability_text);
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
                    Hero _hero = hit.transform.gameObject.GetComponent<Hero>();

                    int amount_of_class = 0;
                    //check if not already 3 of main class
                    foreach (GameObject hero in GameManager.instance.HeroList_P2)
                    {
                        if (hero.GetComponent<Hero>().main_class == _hero.main_class)
                            amount_of_class++;
                    }

                    if (amount_of_class < 3)
                    {
                        _hero.SetDrafted(true);
                        GameManager.instance.HeroList_P2.Add(_hero.transform.gameObject);
                        GameManager.instance.P2_drafted.text = "Drafted: " + GameManager.instance.HeroList_P2.Count + "/" + GameManager.instance.max_amount_units;

                        AbilityBase ability = _hero.HeroAbility;

                        string ability_text =
                            "Effect: " + ability.Ability_effect + "\n" +
                            "Target: " + ability.Ability_target + "\n" +
                            "AoE: " + ability.Ability_aoe + "\n" +
                            "Strength: " + ability.strength + "\n" +
                            "Duration: " + ability.duration + "\n" +
                            "Delay: " + ability.delay;

                        GameManager.instance.SetDraftHeroStats(2, _hero.main_class, _hero.Healthpoints, _hero.Damage, _hero.Initiative, ability_text);
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
        //check if hit is ability button
        if(SelectedHero != null && hit.collider.tag == "Ability")
        {
            AbilitySetTargets();
        }
        //see if selected hero not null and if not null, using ability
        else if (SelectedHero != null && SelectedHero.GetComponent<Hero>().isUsingAbility && player_actions < max_actions)
        {
            //local hit object
            Hero hit_hero = hit.transform.GetComponent<Hero>();

            //local var of hero
            Hero _hero = SelectedHero.GetComponent<Hero>();
            
            //check if the hit hero is targeted
            if(hit_hero.isTargeted)
            {
                //take the ability from hero
                AbilityBase ability = _hero.HeroAbility;

                //local list of tarets
                List<GameObject> target_heroes = new List<GameObject>();

                //actual adding of the targets and not just setting them as possible targets as done previously
                switch (ability.Ability_aoe)
                {
                    case AbilityAOE.all:

                        //add all the targeted heroes
                        target_heroes = ability_targets;
                        
                        break;

                    case AbilityAOE.chain:

                        //not sure yet how to implement
                        //add single target, rest of logic implemented in the resolve phase
                        target_heroes.Add(hit_hero.gameObject);

                        break;

                    case AbilityAOE.column:

                        //take the column from the targeted hero, and hit all the heroes in that column
                        foreach(GameObject hero in ability_targets)
                        {
                            if(hero.GetComponent<Hero>().x_position_grid == hit_hero.x_position_grid)
                            {
                                target_heroes.Add(hero);
                            }
                        }
                        break;

                    case AbilityAOE.row:

                        //take the row from the targeted hero, and hit all the heroes in that row
                        foreach (GameObject hero in ability_targets)
                        {
                            if (hero.GetComponent<Hero>().y_position_grid == hit_hero.y_position_grid)
                            {
                                target_heroes.Add(hero);
                            }
                        }
                        break;

                    case AbilityAOE.single:

                        //add the single clicked hero
                        target_heroes.Add(hit_hero.gameObject);
                        
                        break;
                }

                //add action to list
                list_of_actions.Add(new Action(
                        SelectedHero,
                        GameManager.instance.CurrentTurn,
                        ActionType.ability,
                        target_heroes,
                        ability
                    ));

                //increment attack
                IncrementActions(+1);
            }            
        }
        //
        else
        {
            //check if the unit already performing an action, less than max actions and does not already have an action
            if (hit.collider.tag == own_tag && player_actions < max_actions && !hit.transform.GetComponent<Hero>().hasAction)
            {
                //&& !hit.transform.GetComponent<Hero>().isUsingAbility

                //deselect previous hero
                DeselectHero();

                //set new selected hero
                SelectedHero = hit.transform.gameObject;
                SelectedHero.GetComponent<Hero>().SetSelected(true);

                Hero selected_hero = SelectedHero.GetComponent<Hero>();

                //prevents modified list error for foreach after
                GameManager.instance.CleanLists();
                
                //local var for main class
                MainClass main_class = selected_hero.main_class;

                switch (main_class)
                {
                    case MainClass.Scout:

                        //ranged can hit any of the enemies heroes
                        foreach (GameObject hero in enemy_list)
                        {
                            hero.GetComponent<Hero>().SetTargeted(true);
                        }

                        break;

                    case MainClass.Warrior:

                        //temp list to get the lanes from the hero's
                        List<int> _max_list = new List<int>();

                        //put all int values into the max list if the hero is on the same row
                        foreach (GameObject hero in enemy_list)
                        {
                            Hero _hero = hero.GetComponent<Hero>();

                            if (_hero.y_position_grid == selected_hero.y_position_grid)
                                _max_list.Add(hero.GetComponent<Hero>().x_position_grid);
                        }

                        //get the max value, meaning the closest lane that has a unit on it
                        int _max = Mathf.Max(_max_list.ToArray());

                        //melee can only hit the closest enemy in the same lane
                        foreach (GameObject hero in enemy_list)
                        {
                            Hero _hero = hero.GetComponent<Hero>();

                            if (_hero.x_position_grid == _max && _hero.y_position_grid == selected_hero.y_position_grid)
                                _hero.SetTargeted(true);
                        }

                        break;

                    case MainClass.Mage:

                        //hit a whole lane in a straight line, horizontaly
                        foreach (GameObject hero in enemy_list)
                        {
                            Hero _hero = hero.GetComponent<Hero>();

                            if (_hero.y_position_grid == selected_hero.y_position_grid)
                                _hero.SetTargeted(true);
                        }

                        break;
                }

                //set movement ring on tiles in same lane to active
                int x = selected_hero.x_position_grid;
                int y = selected_hero.y_position_grid;
                player_grid.SetMovementRings(x, y);

            }
            //attacking, check if not null, not using ability, not at max actions
            else if (SelectedHero != null && hit.collider.tag == target_tag && player_actions < max_actions && !SelectedHero.GetComponent<Hero>().isUsingAbility)
            {
                if (hit.transform.GetComponent<Hero>().isTargeted)
                {
                    //non magical attacks that only target one hero
                    if (SelectedHero.GetComponent<Hero>().main_class != MainClass.Mage)
                    {
                        //add action to list
                        list_of_actions.Add(new Action(
                                SelectedHero,
                                GameManager.instance.CurrentTurn,
                                ActionType.attack,
                                hit.transform.gameObject
                            ));

                        //increment attack
                        IncrementActions(+1);
                    }
                    //magical attacks that target a whole row of heros
                    else if (SelectedHero.GetComponent<Hero>().main_class == MainClass.Mage)
                    {
                        GameManager.instance.CleanLists();
                        List<GameObject> target_heroes = new List<GameObject>();
                        foreach (GameObject hero in enemy_list)
                        {
                            Hero _hero = hero.GetComponent<Hero>();

                            if (hit.transform.GetComponent<Hero>().y_position_grid == _hero.y_position_grid)
                            {
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
                    }
                }
            }
            //moving, check if not null, hit is tile, less than max actions, not using ability
            else if (SelectedHero != null && hit.collider.tag == "Tile" && player_actions < max_actions && !SelectedHero.GetComponent<Hero>().isUsingAbility)
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
                    }
                }
            }
        }
    }

    private void AbilitySetTargets()
    { 
        //local var hero
        Hero selected_hero = SelectedHero.GetComponent<Hero>();
        
        if (!selected_hero.isUsingAbility)
        {
            //local var ability
            AbilityBase ability = selected_hero.HeroAbility;

            Debug.Log(
                "Effect: " + ability.Ability_effect + "\n" +
                "Target: " + ability.Ability_target + "\n" +
                "AoE: " + ability.Ability_aoe + "\n" +
                "Strength: " + ability.strength + "\n" +
                "Duration: " + ability.duration + "\n" +
                "Delay: " + ability.delay
                );

            //deselect all previously targeted heroes
            foreach (GameObject g in GameManager.instance.HeroList_P1)
            {
                Hero _h = g.GetComponent<Hero>();

                if (_h.isTargeted)
                    _h.SetTargeted(false);
            }
            foreach (GameObject g in GameManager.instance.HeroList_P2)
            {
                Hero _h = g.GetComponent<Hero>();

                if (_h.isTargeted)
                    _h.SetTargeted(false);
            }

            //hide all movement rings unless ability has to do with movement
            if (ability.Ability_effect != AbilityEffect.movement)
            {
                GameManager.instance.Grid_P1.SetMovementRings(-1, -1);
                GameManager.instance.Grid_P2.SetMovementRings(-1, -1);
            }
            
            //set bool to true on using ability
            selected_hero.isUsingAbility = true;

            //based on heal or damage, target enemies or allies
            switch (ability.Ability_effect)
            {
                case AbilityEffect.damage:
                    //target enemies
                    ability_targets = enemy_list;
                    break;
                case AbilityEffect.heal:
                    //target allies
                    ability_targets = allies_list;
                    break;
            }

            //highlight targets based on the heroes ability
            switch (ability.Ability_target)
            {
                case AbilityTarget.all:
                    foreach (GameObject target in ability_targets)
                    {
                        Hero _hero = target.GetComponent<Hero>();

                        _hero.SetTargeted(true);
                    }
                    break;
                case AbilityTarget.row:
                    foreach (GameObject target in ability_targets)
                    {
                        Hero _hero = target.GetComponent<Hero>();

                        if (_hero.y_position_grid == selected_hero.y_position_grid)
                        {
                            _hero.SetTargeted(true);
                        }
                    }
                    break;
                case AbilityTarget.column:
                    foreach (GameObject target in ability_targets)
                    {
                        Hero _hero = target.GetComponent<Hero>();

                        if (_hero.x_position_grid == selected_hero.x_position_grid)
                        {
                            _hero.SetTargeted(true);
                        }
                    }
                    break;
                case AbilityTarget.any:
                    foreach (GameObject target in ability_targets)
                    {
                        Hero _hero = target.GetComponent<Hero>();

                        _hero.SetTargeted(true);
                    }
                    break;
            }
        }
    }

    private void DeselectHero()
    {
        if (SelectedHero != null)
        {
            //set selected hero to false
            SelectedHero.GetComponent<Hero>().SetSelected(false);

            //reset movement rings
            GameManager.instance.Grid_P1.SetMovementRings(-1, -1);
            GameManager.instance.Grid_P2.SetMovementRings(-1, -1);

            //deselect all previously selected heroes
            foreach (GameObject hero in allies_list)
            {
                hero.GetComponent<Hero>().SetTargeted(false);
            }
            foreach (GameObject hero in enemy_list)
            {
                hero.GetComponent<Hero>().SetTargeted(false);
            }

            SelectedHero = null;
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
                allies_list = GameManager.instance.HeroList_P1;
                player_grid = GameManager.instance.Grid_P1;
                player_actions = p1_actions;
                break;
            case PlayerTurn.Player2:
                own_tag = "HeroP2";
                target_tag = "HeroP1";
                enemy_list = GameManager.instance.HeroList_P1;
                allies_list = GameManager.instance.HeroList_P2;
                player_grid = GameManager.instance.Grid_P2;
                player_actions = p2_actions;
                break;
        }
    }

    public IEnumerator ResolveActions()
    {
        //sort actions list by initiative descending
        list_of_actions = list_of_actions.OrderByDescending(action => action.initiative).ToList();

        yield return new WaitForSeconds(1f);

        foreach (Action action in list_of_actions)
        {
            //yield return new WaitForSeconds(1f);
            //wait till action is finished
            yield return new WaitUntil(() => GameManager.instance.action_ended == true);
            //set action ended to false
            GameManager.instance.action_ended = false;

            yield return new WaitForSeconds(.5f);

            //check if the hero doing the action is still alive and the target is still alive
            //single target actions
            if(action.selected_hero != null && action.single_target != null && action.selected_hero.GetComponent<Hero>().Healthpoints > 0)
            {
                Hero _hero = action.selected_hero.GetComponent<Hero>();
                Hero _target = action.single_target.GetComponent<Hero>();

                switch (action.action_type)
                {
                    case ActionType.attack:
                        //deal damage to target
                        //amount of damage determined by seleted hero
                        //difference between one target and multiple
                        //see if target is still alive
                        switch (_hero.main_class)
                        {
                            case MainClass.Scout:

                                //instance arrow from hero to target
                                _hero.RangedAttack(action.single_target, _hero.Damage);

                                //set checkmark to green on action prefab                            
                                action_icons_list[list_of_actions.IndexOf(action)].GetComponent<ActionPrefab>().SetCheckmark(true, Color.green);

                                break;

                            case MainClass.Warrior:

                                //check if the target isnt being protected by a new enemy that moved in front                                    
                                //send a raycast from hero to target
                                //if another hero is in the way, new target

                                var direction = _target.transform.position - _hero.transform.position;
                                RaycastHit2D[] hit = Physics2D.RaycastAll(_hero.transform.position, direction, 10f);

                                if (hit != null)
                                {
                                    foreach (RaycastHit2D _hit in hit)
                                    {
                                        //see if they have the same collider tag
                                        if (_hit.collider.tag == _target.tag)
                                        {
                                            //not target, set new target
                                            _target = _hit.transform.GetComponent<Hero>();
                                            break;
                                        }
                                    }
                                }
                                //check if target is still on same row
                                if (_target.y_position_grid == _hero.y_position_grid)
                                {
                                    int damage;

                                    if (_target.main_class != MainClass.Warrior)
                                        damage = _hero.Damage * 2;
                                    else
                                        damage = _hero.Damage;
                                    //action.single_target.GetComponent<Hero>().TakeDamage(damage);
                                    _hero.MeleeAttack(_target.gameObject, damage);

                                    //set checkmark to green on action prefab                            
                                    action_icons_list[list_of_actions.IndexOf(action)].GetComponent<ActionPrefab>().SetCheckmark(true, Color.green);
                                }
                                else //action fails
                                {
                                    //set checkmark to red on action prefab                            
                                    action_icons_list[list_of_actions.IndexOf(action)].GetComponent<ActionPrefab>().SetCheckmark(true, Color.red);

                                    GameManager.instance.action_ended = true;
                                }

                                break;
                        }  
                        
                        break;

                    case ActionType.ability:

                        //!!single targets

                        //allies

                        //enemies

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
                            
                        }
                        //tile already occupied
                        else
                        {
                            //set checkmark to red on action prefab
                            action_icons_list[list_of_actions.IndexOf(action)].GetComponent<ActionPrefab>().SetCheckmark(true, Color.red);

                            GameManager.instance.action_ended = true;
                        }
                        break;
                }
            }
            //multiple targets actions
            else if(action.selected_hero != null && action.targets.Count > 0 && action.selected_hero.GetComponent<Hero>().Healthpoints > 0)
            {
                Hero _hero = action.selected_hero.GetComponent<Hero>();
                
                switch(action.action_type)
                {
                    case ActionType.attack:
                        
                        switch(_hero.main_class)
                        {
                            case MainClass.Scout:

                                break;

                            case MainClass.Warrior:

                                break;

                            case MainClass.Mage:

                                List<GameObject> enemy_heroes = new List<GameObject>();

                                //default mage attack
                                //check all enemies on same row and hit them with magic
                                //check tag on target
                                if(action.targets[0].tag == "HeroP1")
                                {
                                    enemy_heroes = GameManager.instance.HeroList_P1;
                                }
                                else if(action.targets[0].tag == "HeroP2")
                                {
                                    enemy_heroes = GameManager.instance.HeroList_P2;
                                }

                                foreach(GameObject enemy_hero in enemy_heroes)
                                {
                                    Hero _enemy_hero = enemy_hero.GetComponent<Hero>();

                                    //check if enemies on same row and alive
                                    if(enemy_hero != null)
                                    {
                                        if(_enemy_hero.Healthpoints > 0)
                                        {
                                            if(_enemy_hero.y_position_grid == _hero.y_position_grid)
                                            {
                                                //hit enemy
                                                _hero.RangedAttack(_enemy_hero.gameObject, _hero.Damage);
                                            }
                                        }
                                    }
                                }

                                break;
                        }

                        break;

                    case ActionType.ability:

                        //!!multiple targets

                        //allies

                        //enemies

                        switch(action.ability.Ability_effect)
                        {
                            case AbilityEffect.damage:

                                //hit all the targets in the action target list
                                //based on the ability power

                                //later use delay, duration, cost

                                foreach(GameObject target in action.targets)
                                {
                                    Hero _target = target.GetComponent<Hero>();

                                    //later use with animation
                                    _target.TakeDamage(action.ability.strength);
                                }

                                break;

                            case AbilityEffect.heal:

                                foreach (GameObject target in action.targets)
                                {
                                    Hero _target = target.GetComponent<Hero>();

                                    //later use with animation
                                    //check if target is not dead already
                                    if(_target.Healthpoints > 0)
                                        _target.HealHero(action.ability.strength);
                                }

                                break;

                            case AbilityEffect.movement:

                                break;
                        }

                        break;

                    case ActionType.movement:

                        break;
                }

                //set checkmark to green on action prefab
                action_icons_list[list_of_actions.IndexOf(action)].GetComponent<ActionPrefab>().SetCheckmark(true, Color.green);
                GameManager.instance.action_ended = true;

                //temp
                yield return new WaitForSeconds(1f);
            }
            else
            {
                //set checkmark to red on action prefab
                action_icons_list[list_of_actions.IndexOf(action)].GetComponent<ActionPrefab>().SetCheckmark(true, Color.red);
                GameManager.instance.action_ended = true;
            }
        }

        //clear list of actions after all actions have been resolved
        list_of_actions.Clear();

        //build in wait buffer for heroes hp
        yield return new WaitForSeconds(1f);

        //clear the field
        StartCoroutine(GameManager.instance.ClearKilledHeroes());
                
    }

    private void IncrementActions(int value)
    {
        switch (GameManager.instance.CurrentTurn)
        {
            case PlayerTurn.Player1:
                p1_actions += value;

                DeselectHero();

                GameManager.instance.P1_actions.text = "Actions: " + p1_actions + "/" + max_actions;
                      
                //check if player one has max actions
                if (p1_actions == max_actions)
                {
                    //disable all movement and targeting circles
                    GameManager.instance.Grid_P1.SetMovementRings(-1, -1);
                    GameManager.instance.Grid_P2.SetMovementRings(-1, -1);

                    //change turn to player 2
                    GameManager.instance.SetPlayerTurn(PlayerTurn.Player2);
                }

                break;

            case PlayerTurn.Player2:
                p2_actions += value;

                DeselectHero();

                GameManager.instance.P2_actions.text = "Actions: " + p2_actions + "/" + max_actions;

                //check if player two has max actions
                if (p2_actions == max_actions)
                {
                    //change phase to resolve
                    GameManager.instance.SetCurrentPhase(Phase.ResolvePhase);
                }

                break;
        }

        DisplayActionList();
    }
    
    private void DisplayActionList()
    {
        //sort actions list by initiative descending
        //list_of_actions = list_of_actions.OrderByDescending(action => action.initiative).ToList();

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

            //set player
            action_icons_list[action_icons_list.Count - 1].GetComponent<ActionPrefab>().player = action.player;

            //increment position y
            position_y++;

            SpriteRenderer sRend = action_icons_list[action_icons_list.Count - 1].GetComponent<SpriteRenderer>();

            //set sprite icon
            switch (action.action_type)
            {
                case ActionType.attack:
                    switch(action.selected_hero.GetComponent<Hero>().main_class)
                    {

                    }
                    sRend.sprite = action_icon_sprites[0];
                    break;
                case ActionType.ability:
                    sRend.sprite = action_icon_sprites[1];
                    break;
                case ActionType.movement:
                    sRend.sprite = action_icon_sprites[2];
                    break;
            }

            //set sprite icon color and player
            switch(action.player)
            {
                case PlayerTurn.Player1:
                    sRend.color = GameManager.instance.Player1_color;                    
                    break;
                case PlayerTurn.Player2:
                    sRend.color = GameManager.instance.Player2_color;
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

    public void OnUndoAction()
    {
        //local list, gets set based on player turn
        List<GameObject> hero_list = new List<GameObject>();

        switch (GameManager.instance.CurrentTurn)
        {
            case PlayerTurn.Player1:
                //check if actions lower or equal to 0, if they are then no undo left
                if (p1_actions <= 0)
                    return;

                p1_actions--; //lower actions amount
                GameManager.instance.P1_actions.text = "Actions: " + p1_actions + "/" + max_actions; //set text
                hero_list = GameManager.instance.HeroList_P1; //set local list    


                break;

            case PlayerTurn.Player2:
                //check if actions lower or equal to 0, if they are then no undo left
                if (p2_actions <= 0)
                    return;

                p2_actions--;
                GameManager.instance.P2_actions.text = "Actions: " + p2_actions + "/" + max_actions;
                hero_list = GameManager.instance.HeroList_P2;
                break;
        }
               
        //remove last added action from the list based on whos turn it is
        //check if last action belongs to current player
        if (list_of_actions[list_of_actions.Count - 1].player == GameManager.instance.CurrentTurn)
        {
            //reset checkmark and has action on hero
            list_of_actions[list_of_actions.Count - 1].selected_hero.GetComponent<Hero>().SetAction(false);

            //destroy icon from plan list
            Destroy(action_icons_list[list_of_actions.IndexOf(list_of_actions[list_of_actions.Count - 1])]);
            
            //remove action from list
            list_of_actions.Remove(list_of_actions[list_of_actions.Count - 1]);
        }

        //clean up icons list
        action_icons_list.RemoveAll(item => item == null);

    }

    public void OnUndoAllActions()
    {
        //local list, gets set based on player turn
        List<GameObject> hero_list = new List<GameObject>();

        switch(GameManager.instance.CurrentTurn)
        {
            case PlayerTurn.Player1:
                //check if any actions for player, otherwise it will throw error
                if (p1_actions <= 0)
                    return;

                p1_actions = 0; //reset actions amount
                GameManager.instance.P1_actions.text = "Actions: " + p1_actions + "/" + max_actions; //set text
                hero_list = GameManager.instance.HeroList_P1; //set local list                
                break;

            case PlayerTurn.Player2:

                if (p2_actions <= 0)
                    return;

                p2_actions = 0;
                GameManager.instance.P2_actions.text = "Actions: " + p2_actions + "/" + max_actions;
                hero_list = GameManager.instance.HeroList_P2;
                break;
        }
        
        //remove all actions from the list based on whos turn it is
        list_of_actions.RemoveAll(item => item.player == GameManager.instance.CurrentTurn);

        //remove icons from plan list
        foreach (GameObject action in action_icons_list)
        {
            ActionPrefab _action = action.GetComponent<ActionPrefab>();

            if(_action.player == GameManager.instance.CurrentTurn)
            {
                Destroy(action);
            }
        }

        action_icons_list.RemoveAll(item => item == null);

        //reset checkmark and has action on heroes
        foreach (GameObject hero in hero_list)
        {
            Hero _hero = hero.GetComponent<Hero>();

            if (_hero.hasAction)
            {
                _hero.SetAction(false);
            }
        }        
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
    public AbilityBase ability;
   
    //single target basic attack
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

    //multiple targets, basic attack
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
    
    //abilities
    public Action(GameObject _selected_hero, PlayerTurn _player, ActionType _action, List<GameObject> _targets, AbilityBase _ability)
    {
        selected_hero = _selected_hero;
        player = _player;
        action_type = _action;
        initiative = selected_hero.GetComponent<Hero>().Initiative;
        targets = _targets;
        ability = _ability;

        //set hero has action to true
        selected_hero.GetComponent<Hero>().SetAction(true);
    }
}