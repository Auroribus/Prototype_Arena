namespace _Scripts.Refactor.Hero.Abilities
{
    public class AbilityBase
    {
        //effect; damage, heal
        //target; any, row, column, all
        //area; single, chain, row, column, all
        //strength; 0 - 9
        //duration; 0-9
        //delay; 0-9
        //cost 0-10 HP / 0-9 Stun

        public AbilityEffect Ability_effect;
        public AbilityTarget Ability_target;
        public AbilityAreaOfEffect AbilityAreaOfEffect;
        public int strength;
        public int duration;
        public int delay;
        //cost hp 0-10
        //cost stun 0-9

        //empty constructor
        public AbilityBase() { }

        //constructor with overload
        public AbilityBase(AbilityEffect _abil_effect, AbilityTarget _abil_target, AbilityAreaOfEffect abilAreaOfEffect, int _str, int _duration, int _delay)
        {
            Ability_effect = _abil_effect;
            Ability_target = _abil_target;
            AbilityAreaOfEffect = abilAreaOfEffect;
            strength = _str;
            duration = _duration;
            delay = _delay;
        }
    }
}