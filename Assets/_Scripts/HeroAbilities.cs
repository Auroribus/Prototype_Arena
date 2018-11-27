using UnityEngine;

public class HeroAbilities : MonoBehaviour {

    public static HeroAbilities instance;

    //stats for ability
    AbilityEffect Ability_effect;
    AbilityTarget Ability_target;
    AbilityAOE Ability_aoe;
    int strength;
    int duration;
    int delay;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            DebugGenerateAbility();
        }
    }
        
    public void DebugGenerateAbility()
    {
        string seed = GenerateSeed();
        Debug.Log("Seed: " + seed);
        GenerateAbility(seed);
    }
        
    //cost

    //used for generating a random ability
    public AbilityBase GenerateAbility(string seed)
    {
        AbilityBase ability = new AbilityBase();

        //make sure that it is all uppercase
        seed = seed.ToLower();

        //seeds consist of 7 letters
        //effect; damage, heal
        if (seed.Length > 0)
        {
            for (int ability_number = 0; ability_number < seed.Length; ability_number++)
            {
                //convert letter to int
                int integer = ConvertAlphabetToInteger(seed[ability_number].ToString());

                //loop for going through the alphabet
                for (int number = 0; number < 27; number++)
                {
                    if(integer == number)
                    {
                        switch(ability_number)
                        {
                            case 0: //ability effect
                                Ability_effect = (AbilityEffect)number;
                                break;
                            case 1: //ability target
                                Ability_target = (AbilityTarget)number;
                                break;
                            case 2: //ability aoe
                                Ability_aoe = (AbilityAOE)number;
                                break;
                            case 3: //strength
                                strength = number;
                                break;
                            case 4: //duration
                                duration = number;
                                break;
                            case 5: //delay
                                delay = number;
                                break;
                        }

                        //break out of loop
                        break;
                    }
                }
            }

            ability = new AbilityBase(
                Ability_effect,
                Ability_target,
                Ability_aoe,
                strength,
                duration,
                delay
                );

           /* Debug.Log(
                "Effect: " + ability.Ability_effect + "\n" +
                "Target: " + ability.Ability_target + "\n" +
                "AoE: " + ability.Ability_aoe + "\n" +
                "Strength: " + ability.strength + "\n" +
                "Duration: " + ability.duration + "\n" +
                "Delay: " + ability.delay
                );
                */
        }

        return ability;
    }

    public string GenerateSeed()
    {
        //effect; damage, heal 2 
        //target; any, row, column, all 4
        //area; single, chain, row, column, all 5
        //strength; 0 - 9 10
        //duration; 0-9 10
        //delay; 0-9 10
        //cost 0-10 HP / 0-9 Stun

        //change hard coded numbers
        string random_effect = ConvertIntToAlphabet(Random.Range(1, 3));
        string random_target = ConvertIntToAlphabet(Random.Range(1, 5));
        string random_area = ConvertIntToAlphabet(Random.Range(1, 6));
        string random_strength = ConvertIntToAlphabet(Random.Range(1, 11));
        string random_duration = ConvertIntToAlphabet(Random.Range(0, 10));
        string random_delay = ConvertIntToAlphabet(Random.Range(0, 10));

        //cost hp
        //cost stun

        //combine randoms into full seed

        //limitations on abilities

        return random_effect + random_target + random_area + random_strength + random_duration + random_delay;
    }

    private string ConvertIntToAlphabet(int integer)
    {
        switch(integer)
        {
            case 1:
                return "a";
            case 2:
                return "b";
            case 3:
                return "c";
            case 4:
                return "d";
            case 5:
                return "e";
            case 6:
                return "f";
            case 7:
                return "g";
            case 8:
                return "h";
            case 9:
                return "i";
            case 10:
                return "j";
            case 11:
                return "k";
            case 12:
                return "l";
            case 13:
                return "m";
            case 14:
                return "n";
            case 15:
                return "o";
            case 16:
                return "p";
            case 17:
                return "q";
            case 18:
                return "r";
            case 19:
                return "s";
            case 20:
                return "t";
            case 21:
                return "u";
            case 22:
                return "v";
            case 23:
                return "w";
            case 24:
                return "x";
            case 25:
                return "y";
            case 26:
                return "z";
        }

        return "0";
    }

    private int ConvertAlphabetToInteger(string _string)
    {
        switch (_string)
        {
            case "a":
                return 1;
            case "b":
                return 2;
            case "c":
                return 3;
            case "d":
                return 4;
            case "e":
                return 5;
            case "f":
                return 6;
            case "g":
                return 7;
            case "h":
                return 8;
            case "i":
                return 9;
            case "j":
                return 10;
            case "k":
                return 11;
            case "l":
                return 12;
            case "m":
                return 13;
            case "n":
                return 14;
            case "o":
                return 15;
            case "p":
                return 16;
            case "q":
                return 17;
            case "r":
                return 18;
            case "s":
                return 19;
            case "t":
                return 20;
            case "u":
                return 21;
            case "v":
                return 22;
            case "w":
                return 23;
            case "x":
                return 24;
            case "y":
                return 25;
            case "z":
                return 26;
        }

        return 0;
    }
}

public enum AbilityEffect
{
    damage = 1,
    heal = 2,
    movement = 3
}

public enum AbilityTarget
{
    any = 1,
    row = 2,
    column = 3,
    all = 4
}

public enum AbilityAOE
{
    single = 1,
    chain = 2,
    row = 3,
    column = 4,
    all = 5
}

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
    public AbilityAOE Ability_aoe;
    public int strength;
    public int duration;
    public int delay;
    //cost hp 0-10
    //cost stun 0-9

    //empty constructor
    public AbilityBase() { }

    //constructor with overload
    public AbilityBase(AbilityEffect _abil_effect, AbilityTarget _abil_target, AbilityAOE _abil_aoe, int _str, int _duration, int _delay)
    {
        Ability_effect = _abil_effect;
        Ability_target = _abil_target;
        Ability_aoe = _abil_aoe;
        strength = _str;
        duration = _duration;
        delay = _delay;
    }
}
