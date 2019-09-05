using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Refactor.Actions;
using _Scripts.Refactor.Game;
using _Scripts.Refactor.Grid;
using _Scripts.Refactor.Hero;
using _Scripts.Refactor.Hero.Abilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Refactor.PlayerScripts
{
    public class Player : MonoBehaviour
    {
        public const int MaxNumberOfPlayerActions = 3;
        private int _playerOneActionCount;
        private int _playerTwoActionCount;

        [Header("Configs")] 
        [SerializeField] private ActionConfig _actionConfig;
        
        [Header("UI")]
        [SerializeField] private GameObject _planningList;

        [Header("Prefabs")] 
        [SerializeField] private GameObject _actionIconPrefab;
        
        #region variables

        public static Player Instance;

        private Vector2 _touchOrigin = -Vector2.one;

        private HeroView SelectedHero;

        //player specific variables that chance depending on the turn
        private string _targetTag;
        private string _ownTag;
        private List<HeroView> _listOfEnemies;
        private List<HeroView> _listOfAllies;
        private GridCreator _playerGrid;
        private int _playerActionCount;

        public List<HeroAction> ListOfActions = new List<HeroAction>();

        //list of targets for ability
        private List<HeroView> _listOfAbilityTargets = new List<HeroView>();

        private List<GameObject> _actionIconsList = new List<GameObject>();

        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            //_planningList = GameObject.Find("Plan List");
        }

        private void Update()
        {
            MouseControl();
        }

        public void ResetPlayerActionCounts()
        {
            _playerOneActionCount = 0;
            _playerTwoActionCount = 0;
        }
        
        private void MouseControl()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                if (hit.collider != null)
                {
                    //quick skip animation ui on clicking it
                    if (hit.collider.tag == "AnimationUI")
                    {
                        hit.transform.GetComponent<Animator>().SetTrigger("DisableFade");
                        hit.transform.gameObject.SetActive(false);
                        return;
                    }

                    switch (GameManager.Instance.CurrentPhase)
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
                else
                {
                    if (SelectedHero != null)
                    {
                        DeselectHero();
                    }
                }
            }
            else if (Input.touchCount > 0)
            {
                var myTouch = Input.touches[0];

                if (myTouch.phase == TouchPhase.Began)
                {
                    _touchOrigin = myTouch.position;

                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(_touchOrigin), Vector2.zero);

                    if (hit.collider != null)
                    {
                        //quick skip animation ui on clicking it
                        if (hit.collider.tag == "AnimationUI")
                        {
                            hit.transform.GetComponent<Animator>().SetTrigger("DisableFade");
                            hit.transform.gameObject.SetActive(false);
                            return;
                        }

                        switch (GameManager.Instance.CurrentPhase)
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
                    else
                    {
                        if (SelectedHero != null)
                        {
                            DeselectHero();
                        }
                    }
                }
            }
        }

        private void DraftPhase(RaycastHit2D hit)
        {
            if (hit.collider.tag == "HeroP1")
            {
                var hitHeroView = hit.transform.GetComponent<HeroView>();
                
                if (!hitHeroView.IsHeroDrafted)
                {
                    if (GameManager.Instance.HeroListP1.Count < GameManager.Instance.MaxAmountOfUnits)
                    {
                        var amount_of_class = 0;
                        //check if not already 3 of main class
                        foreach (var heroView in GameManager.Instance.HeroListP1)
                        {
                            if (hitHeroView.MainClass == heroView.MainClass)
                            {
                                amount_of_class++;
                            }
                        }

                        if (amount_of_class < 3)
                        {
                            hitHeroView.SetDrafted(true);
                            GameManager.Instance.HeroListP1.Add(hitHeroView);
                            GameManager.Instance.PlayerOneDraftedText.text =
                                "Drafted: " + GameManager.Instance.HeroListP1.Count + "/" +
                                GameManager.Instance.MaxAmountOfUnits;
                        }
                    }
                }
                else
                {
                    hitHeroView.SetDrafted(false);
                    GameManager.Instance.HeroListP1.Remove(hitHeroView);
                    GameManager.Instance.PlayerOneDraftedText.text =
                        "Drafted: " + GameManager.Instance.HeroListP1.Count + "/" +
                        GameManager.Instance.MaxAmountOfUnits;
                }
            }
            else if (hit.collider.tag == "HeroP2")
            {
                var hitHeroView = hit.transform.gameObject.GetComponent<HeroView>();

                if (!hitHeroView.IsHeroDrafted)
                {
                    if (GameManager.Instance.HeroListP2.Count < GameManager.Instance.MaxAmountOfUnits)
                    {
                        var amount_of_class = 0;
                        //check if not already 3 of main class
                        foreach (var hero in GameManager.Instance.HeroListP2)
                        {
                            if (hero.MainClass == hitHeroView.MainClass)
                            {
                                amount_of_class++;
                            }
                        }

                        if (amount_of_class < 3)
                        {
                            hitHeroView.SetDrafted(true);
                            GameManager.Instance.HeroListP2.Add(hitHeroView);
                            GameManager.Instance.PlayerTwoDraftedText.text =
                                "Drafted: " + GameManager.Instance.HeroListP2.Count + "/" +
                                GameManager.Instance.MaxAmountOfUnits;
                        }
                    }
                }
                else
                {
                    hitHeroView.SetDrafted(false);
                    GameManager.Instance.HeroListP2.Remove(hitHeroView);
                    GameManager.Instance.PlayerTwoDraftedText.text =
                        "Drafted: " + GameManager.Instance.HeroListP2.Count + "/" +
                        GameManager.Instance.MaxAmountOfUnits;
                }
            }
        }

        private void PlanPhase(RaycastHit2D hit)
        {
            //check if hit is ability button
            if (SelectedHero != null && hit.collider.tag == "Ability")
            {
                AbilitySetTargets();
            }
            //deselect hero if clicking the same hero twice, without using an ability
            else if (SelectedHero != null &&
                     !SelectedHero.IsUsingAbility &&
                     SelectedHero.gameObject == hit.transform.gameObject)
            {
                DeselectHero();
            }
            //see if selected hero not null and if not null, using ability
            else if (SelectedHero != null &&
                     SelectedHero.IsUsingAbility &&
                     _playerActionCount < MaxNumberOfPlayerActions)
            {
                //local hit object
                var hit_heroView =
                    hit.transform.GetComponent<HeroView>();

                //check if the hit hero is targeted
                if (hit_heroView.IsTargeted)
                {
                    //take the ability from hero
                    var ability = SelectedHero.HeroAbility;

                    //local list of tarets
                    var target_heroes = new List<HeroView>();

                    //actual adding of the targets and not just setting them as possible targets as done previously
                    switch (ability.AbilityAreaOfEffect)
                    {
                        case AbilityAreaOfEffect.All:

                            //add all the targeted heroes
                            target_heroes = _listOfAbilityTargets;

                            break;

                        case AbilityAreaOfEffect.Chain:

                            //not sure yet how to implement
                            //add single target, rest of logic implemented in the resolve phase
                            target_heroes.Add(hit_heroView);

                            break;

                        case AbilityAreaOfEffect.Column:

                            //take the column from the targeted hero, and hit all the heroes in that column
                            foreach (var hero in _listOfAbilityTargets)
                            {
                                if (hero.XPositionGrid == hit_heroView.XPositionGrid)
                                {
                                    target_heroes.Add(hero);
                                }
                            }

                            break;

                        case AbilityAreaOfEffect.Row:

                            //take the row from the targeted hero, and hit all the heroes in that row
                            foreach (var hero in _listOfAbilityTargets)
                            {
                                if (hero.YPositionGrid == hit_heroView.YPositionGrid)
                                {
                                    target_heroes.Add(hero);
                                }
                            }

                            break;

                        case AbilityAreaOfEffect.Single:

                            //add the single clicked hero
                            target_heroes.Add(hit_heroView);

                            break;
                    }

                    //add action to list
                    ListOfActions.Add(new HeroAction(
                        SelectedHero,
                        GameManager.Instance.CurrentPlayerTurn,
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
                var hit_heroView =
                    hit.transform.GetComponent<HeroView>();
                
                //check if the unit already performing an action, less than max actions and does not already have an action
                if (hit.collider.tag == _ownTag 
                    && _playerActionCount < MaxNumberOfPlayerActions 
                    && !hit_heroView.HasAction)
                {
                    //&& !hit.transform.GetComponent<Hero>().isUsingAbility

                    //deselect previous hero
                    DeselectHero();

                    //set new selected hero
                    SelectedHero = hit_heroView;
                    SelectedHero.SetSelected(true);

                    //prevents modified list error for foreach after
                    GameManager.Instance.CleanLists();

                    //local var for main class
                    var main_class = SelectedHero.MainClass;

                    switch (main_class)
                    {
                        case MainClass.Scout:

                            //ranged can hit any of the enemies heroes
                            foreach (var hero in _listOfEnemies)
                            {
                                hero.SetTargeted(true);
                            }

                            break;

                        case MainClass.Warrior:

                            //temp list to get the lanes from the hero's
                            var _max_list = new List<int>();

                            //put all int values into the max list if the hero is on the same row
                            foreach (var heroView in _listOfEnemies)
                            {
                                if (heroView.YPositionGrid == SelectedHero.YPositionGrid)
                                {
                                    _max_list.Add(heroView.XPositionGrid);
                                }
                            }

                            //get the max value, meaning the closest lane that has a unit on it
                            var _max = Mathf.Max(_max_list.ToArray());

                            //melee can only hit the closest enemy in the same lane
                            foreach (var heroView in _listOfEnemies)
                            {
                                if (heroView.XPositionGrid == _max 
                                    && heroView.YPositionGrid == SelectedHero.YPositionGrid)
                                {
                                    heroView.SetTargeted(true);
                                }
                            }

                            break;

                        case MainClass.Mage:

                            //hit a whole lane in a straight line, horizontaly
                            foreach (var heroView in _listOfEnemies)
                            {
                                if (heroView.YPositionGrid == SelectedHero.YPositionGrid)
                                {
                                    heroView.SetTargeted(true);
                                }
                            }

                            break;
                    }

                    //set movement ring on tiles in same lane to active
                    var x = SelectedHero.XPositionGrid;
                    var y = SelectedHero.YPositionGrid;
                    _playerGrid.SetMovementRings(x, y);
                }
                //attacking, check if not null, not using ability, not at max actions
                else if (SelectedHero != null && hit.collider.tag == _targetTag &&
                         _playerActionCount < MaxNumberOfPlayerActions &&
                         !SelectedHero.IsUsingAbility)
                {
                    var hitHeroView = hit.transform.GetComponent<HeroView>();

                    if (!hitHeroView.IsTargeted)
                    {
                        return;
                    }
                    
                    //non magical attacks that only target one hero
                    if (SelectedHero.MainClass != MainClass.Mage)
                    {
                        //add action to list
                        ListOfActions.Add(new HeroAction(
                            SelectedHero,
                            GameManager.Instance.CurrentPlayerTurn,
                            ActionType.attack,
                            hitHeroView
                        ));

                        //increment attack
                        IncrementActions(+1);
                    }
                    //magical attacks that target a whole row of heros
                    else if (SelectedHero.MainClass == MainClass.Mage)
                    {
                        GameManager.Instance.CleanLists();
                        var target_heroes = new List<HeroView>();
                        foreach (var heroView in _listOfEnemies)
                        {
                            if (hitHeroView.YPositionGrid == heroView.YPositionGrid)
                            {
                                target_heroes.Add(heroView);
                            }
                        }

                        //add action to the list
                        ListOfActions.Add(new HeroAction(
                            SelectedHero,
                            GameManager.Instance.CurrentPlayerTurn,
                            ActionType.attack,
                            target_heroes
                        ));

                        //increment attack
                        IncrementActions(+1);
                    }
                }
                //moving, check if not null, hit is tile, less than max actions, not using ability
                else if (SelectedHero != null 
                         && hit.collider.tag == "Tile" 
                         && _playerActionCount < MaxNumberOfPlayerActions 
                         && !SelectedHero.IsUsingAbility)
                {
                    var hitGridTile = hit.transform.GetComponent<GridTile>();
                    
                    if (hitGridTile.can_move_here)
                    {
                        if (!SelectedHero.MoveHero)
                        {
                            //add movement action to the list
                            ListOfActions.Add(new HeroAction(
                                SelectedHero,
                                GameManager.Instance.CurrentPlayerTurn,
                                ActionType.movement,
                                hitGridTile
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
            if (!SelectedHero.IsUsingAbility)
            {
                var ability = SelectedHero.HeroAbility;

                //deselect all previously targeted heroes
                foreach (var heroView in GameManager.Instance.HeroListP1)
                {
                    if (heroView.IsTargeted)
                    {
                        heroView.SetTargeted(false);
                    }
                }

                foreach (var heroView in GameManager.Instance.HeroListP2)
                {
                    if (heroView.IsTargeted)
                    {
                        heroView.SetTargeted(false);
                    }
                }

                //hide all movement rings unless ability has to do with movement
                if (ability.Ability_effect != AbilityEffect.movement)
                {
                    GameManager.Instance.GridPlayerOne.SetMovementRings(-1, -1);
                    GameManager.Instance.GridPlayerTwo.SetMovementRings(-1, -1);
                }

                //set bool to true on using ability
                SelectedHero.IsUsingAbility = true;

                //based on heal or damage, target enemies or allies
                switch (ability.Ability_effect)
                {
                    case AbilityEffect.damage:
                        //target enemies
                        _listOfAbilityTargets = _listOfEnemies;
                        break;
                    case AbilityEffect.heal:
                        //target allies
                        _listOfAbilityTargets = _listOfAllies;
                        break;
                }

                //highlight targets based on the heroes ability
                switch (ability.Ability_target)
                {
                    case AbilityTarget.All:
                        foreach (var heroView in _listOfAbilityTargets)
                        {
                            heroView.SetTargeted(true);
                        }

                        break;
                    case AbilityTarget.Row:
                        foreach (var target in _listOfAbilityTargets)
                        {
                            if (target.YPositionGrid == SelectedHero.YPositionGrid)
                            {
                                target.SetTargeted(true);
                            }
                        }

                        break;
                    case AbilityTarget.Column:
                        foreach (var target in _listOfAbilityTargets)
                        {
                            if (target.XPositionGrid == SelectedHero.XPositionGrid)
                            {
                                target.SetTargeted(true);
                            }
                        }

                        break;
                    case AbilityTarget.Any:
                        foreach (var target in _listOfAbilityTargets)
                        {
                            target.SetTargeted(true);
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
                SelectedHero.SetSelected(false);

                //reset movement rings
                GameManager.Instance.GridPlayerOne.SetMovementRings(-1, -1);
                GameManager.Instance.GridPlayerTwo.SetMovementRings(-1, -1);

                //deselect all previously selected heroes
                foreach (var hero in _listOfAllies)
                {
                    hero.SetTargeted(false);
                }

                foreach (var hero in _listOfEnemies)
                {
                    hero.SetTargeted(false);
                }

                SelectedHero = null;
            }
        }

        private void SetPlayerTurnActionPhase()
        {
            switch (GameManager.Instance.CurrentPlayerTurn)
            {
                case PlayerTurn.Player1:
                    _ownTag = "HeroP1";
                    _targetTag = "HeroP2";
                    _listOfEnemies = GameManager.Instance.HeroListP2;
                    _listOfAllies = GameManager.Instance.HeroListP1;
                    _playerGrid = GameManager.Instance.GridPlayerOne;
                    _playerActionCount = _playerOneActionCount;
                    break;
                case PlayerTurn.Player2:
                    _ownTag = "HeroP2";
                    _targetTag = "HeroP1";
                    _listOfEnemies = GameManager.Instance.HeroListP1;
                    _listOfAllies = GameManager.Instance.HeroListP2;
                    _playerGrid = GameManager.Instance.GridPlayerTwo;
                    _playerActionCount = _playerTwoActionCount;
                    break;
            }
        }

        public IEnumerator ResolveActions()
        {
            //sort actions list by initiative descending
            ListOfActions = ListOfActions.OrderByDescending(action => action.initiative).ToList();
            DisplayActionList();

            yield return new WaitForSeconds(1f);

            foreach (var action in ListOfActions)
            {
                //wait till action is finished
                yield return new WaitUntil(() => GameManager.Instance.HasActionEnded);
                //set action ended to false
                GameManager.Instance.HasActionEnded = false;

                yield return new WaitForSeconds(.5f);

                //check if the hero doing the action is still alive and the target is still alive
                //single target actions
                if (action.selected_hero != null
                    && action.single_target != null
                    && action.selected_hero.HeroStatsModel.HealthPoints > 0)
                {
                    var heroView = action.selected_hero;
                    var _target = action.single_target;

                    switch (action.action_type)
                    {
                        case ActionType.attack:
                            //deal damage to target
                            //amount of damage determined by selected hero
                            //difference between one target and multiple
                            //see if target is still alive
                            switch (heroView.MainClass)
                            {
                                case MainClass.Scout:

                                    //instance arrow from hero to target
                                    heroView.RangedAttack(action.single_target, heroView.HeroStatsModel.AttackDamage);

                                    //set checkmark to green on action prefab                            
                                    _actionIconsList[ListOfActions.IndexOf(action)].GetComponent<ActionPrefab>()
                                        .SetCheckmark(true, Color.green);

                                    break;

                                case MainClass.Warrior:

                                    //check if the target isnt being protected by a new enemy that moved in front                                    
                                    //send a raycast from hero to target
                                    //if another hero is in the way, new target

                                    var direction = _target.transform.position - heroView.transform.position;
                                    var hit =
                                        Physics2D.RaycastAll(heroView.transform.position, direction, 10f);

                                    if (hit != null)
                                    {
                                        foreach (var _hit in hit)
                                        {
                                            //see if they have the same collider tag
                                            if (_hit.collider.tag == _target.tag)
                                            {
                                                //not target, set new target
                                                _target = _hit.transform.GetComponent<HeroView>();
                                                break;
                                            }
                                        }
                                    }

                                    //check if target is still on same row
                                    if (_target.YPositionGrid == heroView.YPositionGrid)
                                    {
                                        int damage;

                                        if (_target.MainClass != MainClass.Warrior)
                                        {
                                            damage = heroView.HeroStatsModel.AttackDamage * 2;
                                        }
                                        else
                                        {
                                            damage = heroView.HeroStatsModel.AttackDamage;
                                        }

                                        heroView.MeleeAttack(_target, damage);

                                        //set checkmark to green on action prefab                            
                                        _actionIconsList[ListOfActions.IndexOf(action)].GetComponent<ActionPrefab>()
                                            .SetCheckmark(true, Color.green);
                                    }
                                    else //action fails
                                    {
                                        //set checkmark to red on action prefab                            
                                        _actionIconsList[ListOfActions.IndexOf(action)].GetComponent<ActionPrefab>()
                                            .SetCheckmark(true, Color.red);

                                        GameManager.Instance.HasActionEnded = true;
                                    }

                                    break;
                            }

                            break;

                        case ActionType.movement:

                            //check if target tile is not occupied by other movement
                            if (!action.GridTile.isOccupied)
                            {
                                //set old occupied tile to no longer occupied
                                switch (action.player)
                                {
                                    case PlayerTurn.Player1:
                                        GameManager.Instance.GridPlayerOne.Grid[
                                                action.selected_hero.XPositionGrid,
                                                action.selected_hero.YPositionGrid]
                                            .GetComponent<GridTile>().isOccupied = false;
                                        break;
                                    case PlayerTurn.Player2:
                                        GameManager.Instance.GridPlayerTwo.Grid[
                                                action.selected_hero.XPositionGrid,
                                                action.selected_hero.YPositionGrid]
                                            .GetComponent<GridTile>().isOccupied = false;
                                        break;
                                }

                                //move hero
                                action.selected_hero.TargetPosition = action.single_target.transform.position;
                                action.selected_hero.MoveHero = true;

                                //update heros position on grid
                                action.selected_hero.XPositionGrid = action.single_target.transform.GetComponent<GridTile>()
                                    .pos_grid_x;
                                action.selected_hero.YPositionGrid = action.single_target.transform.GetComponent<GridTile>()
                                    .pos_grid_y;

                                //set new tile as occupied
                                action.GridTile.isOccupied = true;
                                action.GridTile.SetMovementRing(false);

                                //set checkmark to green on action prefab
                                _actionIconsList[ListOfActions.IndexOf(action)].GetComponent<ActionPrefab>()
                                    .SetCheckmark(true, Color.green);
                            }
                            //tile already occupied
                            else
                            {
                                //set checkmark to red on action prefab
                                _actionIconsList[ListOfActions.IndexOf(action)].GetComponent<ActionPrefab>()
                                    .SetCheckmark(true, Color.red);

                                GameManager.Instance.HasActionEnded = true;
                            }

                            break;
                    }
                }
                //multiple targets actions
                else if (action.selected_hero != null 
                         && action.targets.Count > 0 
                         && action.selected_hero.HeroStatsModel.HealthPoints > 0)
                {
                    var heroView = action.selected_hero;

                    switch (action.action_type)
                    {
                        case ActionType.attack:

                            switch (heroView.MainClass)
                            {
                                case MainClass.Scout:

                                    break;

                                case MainClass.Warrior:

                                    break;

                                case MainClass.Mage:

                                    var target_tag = "";
                                    var direction = 0;

                                    switch (action.player)
                                    {
                                        case PlayerTurn.Player1:
                                            direction = 1;
                                            target_tag = "HeroP2";
                                            break;

                                        case PlayerTurn.Player2:
                                            direction = -1;
                                            target_tag = "HeroP1";
                                            break;
                                    }

                                    heroView.MagicAttack(heroView.HeroStatsModel.AttackDamage, direction, target_tag);

                                    break;
                            }

                            break;

                        case ActionType.ability:

                            //!!multiple targets
                            switch (action.ability.Ability_effect)
                            {
                                case AbilityEffect.damage:

                                    //hit all the targets in the action target list
                                    //based on the ability power
                                    foreach (var target in action.targets)
                                    {
                                        //later use with animation
                                        //_target.TakeDamage(action.ability.strength);

                                        switch (heroView.MainClass)
                                        {
                                            case MainClass.Warrior:
                                                if (action.ability.AbilityAreaOfEffect == AbilityAreaOfEffect.Chain)
                                                {
                                                    //melee chain ability
                                                    StartCoroutine(heroView.RangedAttack_Chain(target,
                                                        action.ability.strength));
                                                    yield return new WaitForSeconds(.2f);
                                                }
                                                else
                                                {
                                                    //melee AoE ability
                                                    heroView.MeleeAbilityAttack(target, action.ability.strength);
                                                    yield return new WaitForSeconds(.2f);
                                                }

                                                break;
                                            case MainClass.Scout:
                                                if (action.ability.AbilityAreaOfEffect == AbilityAreaOfEffect.Chain)
                                                {
                                                    //ranged chain ability
                                                    StartCoroutine(heroView.RangedAttack_Chain(target,
                                                        action.ability.strength));
                                                    yield return new WaitForSeconds(.2f);
                                                }
                                                else
                                                {
                                                    //ranged AoE ability
                                                    StartCoroutine(heroView.RangedAbilityAttack(target,
                                                        action.ability.strength));
                                                }

                                                break;
                                            case MainClass.Mage:
                                                if (action.ability.AbilityAreaOfEffect == AbilityAreaOfEffect.Chain)
                                                {
                                                    //magic chain ability
                                                    StartCoroutine(heroView.MagicAttack_Chain(target,
                                                        action.ability.strength));
                                                    yield return new WaitForSeconds(.2f);
                                                }
                                                else
                                                {
                                                    //magic AoE ability
                                                    StartCoroutine(heroView.MageAbilityAttack(target,
                                                        action.ability.strength));
                                                }

                                                break;
                                        }
                                    }

                                    if (action.ability.AbilityAreaOfEffect == AbilityAreaOfEffect.Chain)
                                        yield return new WaitUntil(() => heroView.HasChainEnded);
                                    else
                                        yield return new WaitForSeconds(1f);

                                    break;

                                case AbilityEffect.heal:

                                    foreach (var target in action.targets)
                                    {
                                        if (target.HeroStatsModel.HealthPoints > 0)
                                        {
                                            target.HeroStatsController.HealHero(action.ability.strength);
                                        }
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
                    _actionIconsList[ListOfActions.IndexOf(action)].GetComponent<ActionPrefab>()
                        .SetCheckmark(true, Color.green);
                    GameManager.Instance.HasActionEnded = true;

                    //temp
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    //set checkmark to red on action prefab
                    _actionIconsList[ListOfActions.IndexOf(action)].GetComponent<ActionPrefab>()
                        .SetCheckmark(true, Color.red);
                    GameManager.Instance.HasActionEnded = true;
                }
            }

            //clear list of actions after all actions have been resolved
            ClearActionsList();

            //build in wait buffer for heroes hp
            yield return new WaitForSeconds(1f);

            //clear the field
            StartCoroutine(GameManager.Instance.ClearKilledHeroes());
        }

        private void IncrementActions(int value)
        {
            switch (GameManager.Instance.CurrentPlayerTurn)
            {
                case PlayerTurn.Player1:
                    _playerOneActionCount += value;

                    DeselectHero();

                    GameManager.Instance.PlayerOneActionsText.text =
                        "Actions: " + _playerOneActionCount + "/" + MaxNumberOfPlayerActions;

                    //check if player one has max actions
                    if (_playerOneActionCount == MaxNumberOfPlayerActions)
                    {
                        //disable all movement and targeting circles
                        GameManager.Instance.GridPlayerOne.SetMovementRings(-1, -1);
                        GameManager.Instance.GridPlayerTwo.SetMovementRings(-1, -1);

                        //change turn to player 2
                        GameManager.Instance.SetPlayerTurn(PlayerTurn.Player2);
                    }

                    break;

                case PlayerTurn.Player2:
                    _playerTwoActionCount += value;

                    DeselectHero();

                    GameManager.Instance.PlayerTwoActionsText.text =
                        "Actions: " + _playerTwoActionCount + "/" + MaxNumberOfPlayerActions;

                    //check if player two has max actions
                    if (_playerTwoActionCount == MaxNumberOfPlayerActions)
                    {
                        //change phase to resolve
                        GameManager.Instance.SetCurrentPhase(Phase.ResolvePhase);
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

            var position_y = 0;

            foreach (var action in ListOfActions)
            {
                //set instance position
                var icon_position = new Vector2
                {
                    x = _planningList.transform.position.x,
                    y = _planningList.transform.position.y - position_y
                };

                //instance action prefab
                _actionIconsList.Add(Instantiate(_actionIconPrefab, icon_position, Quaternion.identity,
                    _planningList.transform));

                //set player
                _actionIconsList[_actionIconsList.Count - 1].GetComponent<ActionPrefab>().player = action.player;

                //increment position y
                position_y++;

                var actionIconsListSpriteRenderer = _actionIconsList[_actionIconsList.Count - 1].GetComponent<SpriteRenderer>();

                //set sprite icon
                switch (action.action_type)
                {
                    case ActionType.attack:
                        switch (action.selected_hero.MainClass)
                        {
                            case MainClass.Warrior:
                                break;
                            case MainClass.Scout:
                                break;
                            case MainClass.Mage:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    case ActionType.ability:
                        break;
                    case ActionType.movement:
                        break;
                }

                actionIconsListSpriteRenderer.sprite = _actionConfig.GetActionSprite(action.action_type);
                
                //set sprite icon color and player
                switch (action.player)
                {
                    case PlayerTurn.Player1:
                        actionIconsListSpriteRenderer.color = GameManager.Instance.ColorPlayer1;
                        break;
                    case PlayerTurn.Player2:
                        actionIconsListSpriteRenderer.color = GameManager.Instance.ColorPlayer2;
                        break;
                }
            }
        }

        public void ClearActionIcons()
        {
            foreach (var action in _actionIconsList)
            {
                Destroy(action);
            }

            _actionIconsList.Clear();
        }

        public void ClearActionsList()
        {
            ListOfActions.Clear();
        }

        public void OnUndoAction()
        {
            //local list, gets set based on player turn
            var listOfHeroes = new List<HeroView>();

            switch (GameManager.Instance.CurrentPlayerTurn)
            {
                case PlayerTurn.Player1:
                    //check if actions lower or equal to 0, if they are then no undo left
                    if (_playerOneActionCount <= 0)
                        return;

                    _playerOneActionCount--; //lower actions amount
                    GameManager.Instance.PlayerOneActionsText.text =
                        "Actions: " + _playerOneActionCount + "/" + MaxNumberOfPlayerActions; //set text
                    listOfHeroes = GameManager.Instance.HeroListP1; //set local list    


                    break;

                case PlayerTurn.Player2:
                    //check if actions lower or equal to 0, if they are then no undo left
                    if (_playerTwoActionCount <= 0)
                        return;

                    _playerTwoActionCount--;
                    GameManager.Instance.PlayerTwoActionsText.text =
                        "Actions: " + _playerTwoActionCount + "/" + MaxNumberOfPlayerActions;
                    listOfHeroes = GameManager.Instance.HeroListP2;
                    break;
            }

            //remove last added action from the list based on whos turn it is
            //check if last action belongs to current player
            if (ListOfActions[ListOfActions.Count - 1].player == GameManager.Instance.CurrentPlayerTurn)
            {
                //reset checkmark and has action on hero
                ListOfActions[ListOfActions.Count - 1].selected_hero.SetAction(false);

                //destroy icon from plan list
                Destroy(_actionIconsList[ListOfActions.IndexOf(ListOfActions[ListOfActions.Count - 1])]);

                //remove action from list
                ListOfActions.Remove(ListOfActions[ListOfActions.Count - 1]);
            }

            //clean up icons list
            _actionIconsList.RemoveAll(item => item == null);
        }

        public void OnUndoAllActions()
        {
            //local list, gets set based on player turn
            var listOfHeroes = new List<HeroView>();

            switch (GameManager.Instance.CurrentPlayerTurn)
            {
                case PlayerTurn.Player1:
                    //check if any actions for player, otherwise it will throw error
                    if (_playerOneActionCount <= 0)
                    {
                        return;
                    }

                    _playerOneActionCount = 0; //reset actions amount
                    GameManager.Instance.PlayerOneActionsText.text =
                        "Actions: " + _playerOneActionCount + "/" + MaxNumberOfPlayerActions; //set text
                    listOfHeroes = GameManager.Instance.HeroListP1; //set local list                
                    break;

                case PlayerTurn.Player2:

                    if (_playerTwoActionCount <= 0)
                    {
                        return;
                    }

                    _playerTwoActionCount = 0;
                    GameManager.Instance.PlayerTwoActionsText.text =
                        "Actions: " + _playerTwoActionCount + "/" + MaxNumberOfPlayerActions;
                    listOfHeroes = GameManager.Instance.HeroListP2;
                    break;
            }

            //remove all actions from the list based on whos turn it is
            ListOfActions.RemoveAll(item => item.player == GameManager.Instance.CurrentPlayerTurn);

            //remove icons from plan list
            foreach (var action in _actionIconsList)
            {
                var _action = action.GetComponent<ActionPrefab>();

                if (_action.player == GameManager.Instance.CurrentPlayerTurn)
                {
                    Destroy(action);
                }
            }

            _actionIconsList.RemoveAll(item => item == null);

            //reset checkmark and has action on heroes
            foreach (var heroView in listOfHeroes)
            {
                if (heroView.HasAction)
                {
                    heroView.SetAction(false);
                }
            }
        }
    }
}