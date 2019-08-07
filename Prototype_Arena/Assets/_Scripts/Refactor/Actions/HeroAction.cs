using System.Collections.Generic;
using _Scripts.Refactor.Hero;
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
        public GameObject selected_hero;
        public GameObject single_target;
        public List<GameObject> targets = new List<GameObject>();
        public AbilityBase ability;

        public int casting_delay;
        public int duration_of_effect;

        //single target basic attack
        public HeroAction(GameObject _selected_hero, PlayerTurn _player, ActionType _action, GameObject _single_target)
        {
            selected_hero = _selected_hero;
            player = _player;
            action_type = _action;
            single_target = _single_target;

            //set hero has action to true
            selected_hero.GetComponent<HeroView>().SetAction(true);

            initiative = selected_hero.GetComponent<HeroView>().HeroStatsModel.Initiative;
            //temp fix to coin flip for same initiative
            if (Random.Range(0, 2) == 0)
            {
                initiative++;
            }
        }

        //multiple targets, basic attack
        public HeroAction(GameObject _selected_hero, PlayerTurn _player, ActionType _action, List<GameObject> _targets)
        {
            selected_hero = _selected_hero;
            player = _player;
            action_type = _action;
            targets = _targets;

            //set hero has action to true
            selected_hero.GetComponent<HeroView>().SetAction(true);

            initiative = selected_hero.GetComponent<HeroView>().HeroStatsModel.Initiative;
            //temp fix to coin flip for same initiative
            if (Random.Range(0, 2) == 0)
            {
                initiative++;
            }
        }

        //abilities
        public HeroAction(GameObject _selected_hero, PlayerTurn _player, ActionType _action, List<GameObject> _targets,
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
            selected_hero.GetComponent<HeroView>().SetAction(true);

            initiative = selected_hero.GetComponent<HeroView>().HeroStatsModel.Initiative;
            //temp fix to coin flip for same initiative
            if (Random.Range(0, 2) == 0)
            {
                initiative++;
            }
        }
    }
}