using UnityEngine;

namespace _Scripts.Refactor.Hero.Abilities
{
    public class HeroAbilities : MonoBehaviour
    {
        public static HeroAbilities instance;

        //stats for ability
        private AbilityEffect _abilityEffect;
        private AbilityTarget _abilityTarget;
        private AbilityAreaOfEffect _abilityAreaOfEffect;
        private int _strength;
        private int _duration;
        private int _delay;

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                DebugGenerateAbility();
            }
        }

        private void DebugGenerateAbility()
        {
            var seed = GenerateSeed();
            Debug.Log("Seed: " + seed);
            GenerateAbility(seed);
        }

        //used for generating a random ability
        public AbilityBase GenerateAbility(string seed)
        {
            var ability = new AbilityBase();

            seed = seed.ToLower();
            if (seed.Length <= 0)
            {
                return ability;
            }
            
            for (var abilityIndex = 0; abilityIndex < seed.Length; abilityIndex++)
            {
                var integer = ConvertCharacterToInteger(seed[abilityIndex].ToString());

                for (var number = 0; number < 27; number++)
                {
                    if (integer != number)
                    {
                        continue;
                    }
                    
                    switch (abilityIndex)
                    {
                        case 0: //ability effect
                            _abilityEffect = (AbilityEffect) number;
                            break;
                        case 1: //ability target
                            _abilityTarget = (AbilityTarget) number;
                            break;
                        case 2: //ability aoe
                            _abilityAreaOfEffect = (AbilityAreaOfEffect) number;
                            break;
                        case 3: //strength
                            _strength = number;
                            break;
                        case 4: //duration
                            _duration = number;
                            break;
                        case 5: //delay
                            _delay = number;
                            break;
                    }

                    //break out of loop
                    break;
                }
            }

            ability = new AbilityBase(
                _abilityEffect,
                _abilityTarget,
                _abilityAreaOfEffect,
                _strength,
                _duration,
                _delay
            );

            return ability;
        }

        public string GenerateSeed()
        {
            var randomEffect = ConvertIntegerToCharacter(Random.Range(1, 3));
            var randomTarget = ConvertIntegerToCharacter(Random.Range(1, 5));
            var randomAreaOfEffect = ConvertIntegerToCharacter(Random.Range(1, 6));
            var randomStrength = ConvertIntegerToCharacter(Random.Range(1, 11));
            var randomDuration = ConvertIntegerToCharacter(Random.Range(0, 10));
            var randomDelay = ConvertIntegerToCharacter(Random.Range(0, 10));

            return randomEffect
                   + randomTarget
                   + randomAreaOfEffect
                   + randomStrength
                   + randomDuration
                   + randomDelay;
        }

        private string ConvertIntegerToCharacter(int integer)
        {
            switch (integer)
            {
                case 1: return "a";
                case 2: return "b";
                case 3: return "c";
                case 4: return "d";
                case 5: return "e";
                case 6: return "f";
                case 7: return "g";
                case 8: return "h";
                case 9: return "i";
                case 10: return "j";
                case 11: return "k";
                case 12: return "l";
                case 13: return "m";
                case 14: return "n";
                case 15: return "o";
                case 16: return "p";
                case 17: return "q";
                case 18: return "r";
                case 19: return "s";
                case 20: return "t";
                case 21: return "u";
                case 22: return "v";
                case 23: return "w";
                case 24: return "x";
                case 25: return "y";
                case 26: return "z";
            }

            return "0";
        }

        private int ConvertCharacterToInteger(string _string)
        {
            switch (_string)
            {
                case "a": return 1;
                case "b": return 2;
                case "c": return 3;
                case "d": return 4;
                case "e": return 5;
                case "f": return 6;
                case "g": return 7;
                case "h": return 8;
                case "i": return 9;
                case "j": return 10;
                case "k": return 11;
                case "l": return 12;
                case "m": return 13;
                case "n": return 14;
                case "o": return 15;
                case "p": return 16;
                case "q": return 17;
                case "r": return 18;
                case "s": return 19;
                case "t": return 20;
                case "u": return 21;
                case "v": return 22;
                case "w": return 23;
                case "x": return 24;
                case "y": return 25;
                case "z": return 26;
            }

            return 0;
        }
    }
}