using System.Collections.Generic;
using _Scripts.Refactor.Grid;
using _Scripts.Refactor.Hero;
using _Scripts.Refactor.Hero.Abilities;
using _Scripts.Refactor.PlayerScripts;
using UnityEngine;

namespace _Scripts.Refactor.Actions
{
     public class HeroAction
    {
        //type of action, attack, ability, move
        //action initiative
        //target or targets
        public PlayerTurn player;
        public ActionType action_type;
        public int initiative;
        public HeroView selected_hero;
        public HeroView single_target;
        public List<HeroView> targets = new List<HeroView>();
        public AbilityBase ability;
        public GridTile GridTile;

        public int casting_delay;
        public int duration_of_effect;

        //single target basic attack
        public HeroAction(HeroView _selected_hero, PlayerTurn _player, ActionType _action, HeroView _single_target)
        {
            selected_hero = _selected_hero;
            player = _player;
            action_type = _action;
            single_target = _single_target;

            //set hero has action to true
            selected_hero.SetAction(true);

            initiative = selected_hero.HeroStatsModel.Initiative;
            //temp fix to coin flip for same initiative
            if (Random.Range(0, 2) == 0)
            {
                initiative++;
            }
        }
        
        public HeroAction(HeroView _selected_hero, PlayerTurn _player, ActionType _action, GridTile gridTileTarget)
        {
            selected_hero = _selected_hero;
            player = _player;
            action_type = _action;
            GridTile = gridTileTarget;

            //set hero has action to true
            selected_hero.SetAction(true);

            initiative = selected_hero.HeroStatsModel.Initiative;
            //temp fix to coin flip for same initiative
            if (Random.Range(0, 2) == 0)
            {
                initiative++;
            }
        }

        //multiple targets, basic attack
        public HeroAction(HeroView _selected_hero, PlayerTurn _player, ActionType _action, List<HeroView> _targets)
        {
            selected_hero = _selected_hero;
            player = _player;
            action_type = _action;
            targets = _targets;

            //set hero has action to true
            selected_hero.SetAction(true);

            initiative = selected_hero.HeroStatsModel.Initiative;
            //temp fix to coin flip for same initiative
            if (Random.Range(0, 2) == 0)
            {
                initiative++;
            }
        }

        //abilities
        public HeroAction(
            HeroView _selected_hero, 
            PlayerTurn _player, 
            ActionType _action, 
            List<HeroView> _targets,
            AbilityBase _ability)
        {
            selected_hero = _selected_hero;
            player = _player;
            action_type = _action;
            targets = _targets;
            ability = _ability;

            casting_delay = _ability.delay;
            duration_of_effect = _ability.duration;

            //set hero has action to true
            selected_hero.SetAction(true);

            initiative = selected_hero.HeroStatsModel.Initiative;
            //temp fix to coin flip for same initiative
            if (Random.Range(0, 2) == 0)
            {
                initiative++;
            }
        }
    }
}