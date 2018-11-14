using System.Collections.Generic;
using UnityEngine;

public class HeroAbilities : MonoBehaviour {

    public static HeroAbilities instance;
    
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

    #region abilities

    //heals all allied heroes by x hp
    public void HolyTouch(List<GameObject> allied_heroes, int heal_amount)
    {
        foreach(GameObject hero in allied_heroes)
        {
            hero.GetComponent<Hero>().Healthpoints += heal_amount;
        }
    }

    //prevents 1 damage for each hero in the same row for 1 turn
    public void ShieldWall(List<GameObject> allied_heroes, int row, int turns)
    {
        foreach(GameObject hero in allied_heroes)
        {
            if(hero.GetComponent<Hero>().x_position_grid == row)
            {
                //prevent one damage on each hero
            }
        }
    }

    //gives +x initative for x amount of turns to all allied heroes
    public void HastyBallad(List<GameObject> allied_heroes, int buff_amount, int turns)
    {
        foreach (GameObject hero in allied_heroes)
        {
            hero.GetComponent<Hero>().Initiative += buff_amount;
        }

        //remove buff after two turns, implement once turns is implemented
    }

    //Stun for x turns, 50% chance, seperate roll for each hero, targets all enemies in a row
    public void SleepPowder(List<GameObject> enemy_heroes, int row, int turns, int chance_percentage)
    {
        foreach(GameObject hero in enemy_heroes)
        {
            if(hero.GetComponent<Hero>().x_position_grid == row)
            {
                //50% chance to happen, seperate roll for each hero
                if(Random.Range(0,10) <= chance_percentage)
                {
                    //stun hero
                }
            }
        }
    }

    //Spawn flame on enemy tile, lasts for x turns and deals x damage per turn
    public void ScorchedEarth(GameObject target, int damage_per_turn, int turns)
    {
        //instantiate flame on enemy position
    }

    //non magic users get +x damage, lasts for x turns
    public void BloodAndSteal(List<GameObject> allied_heroes, int damage_buff, int turns)
    {
        foreach(GameObject hero in allied_heroes)
        {
            if(hero.GetComponent<Hero>().main_class != MainClass.Mage)
            {
                //increase damage buff variable
            }
        }

        //reset after x turns
    }
     
    //next attacks deals x multiple damage, stuns self for x turns
    public void RagingStrike(GameObject hero, int damage_multiplier, int turns)
    {
        //variable damage multiplier

        //reset after x turns
    }

    //hits closest enemy on same row, deals x damage, deals x+y damage if hero alone in same row
    public void AloneInTheDark(List<GameObject> allied_heroes, int row, GameObject target, int damage, int extra_damage)
    {
        int Damage = 0;

        foreach(GameObject hero in allied_heroes)
        {
            if(hero.GetComponent<Hero>().x_position_grid == row)
            {
                Damage = damage;
                break;
            }
            else
            {
                Damage = damage + extra_damage;
            }
        }

        target.GetComponent<Hero>().TakeDamage(Damage);
    }

    //deals x damage, deals extra damage if target has less than x health
    public void PrayOnTheWeak(GameObject target, int damage, int extra_damage, int health_treshold)
    {
        int Damage = 0;

        if (target.GetComponent<Hero>().Healthpoints <= health_treshold)
        {
            Damage = damage + extra_damage;
        }
        else if(target.GetComponent<Hero>().Healthpoints > health_treshold)
        {
            Damage = damage;
        }
        
        target.GetComponent<Hero>().TakeDamage(Damage);
    }

    //hits target, then has x% chance to bounce to new target
    public void ChainLighting(GameObject target, int chance_percentage, int damage)
    {
        target.GetComponent<Hero>().TakeDamage(damage);

        if (Random.Range(0, 100) <= chance_percentage)
        {
            //bounce to another target enemy
        }
    }

    //stuns self for x turns, permanently buffs damage by x multiplier
    public void SwordDance(GameObject hero, int damage_multiplier, int stun_turns)
    {
        //buff damage multiplier

        //stun self for x turns
    }

    #endregion

    public void DebugGenerateAbility()
    {
        string seed = GenerateSeed();
        Debug.Log("Seed: " + seed);
        GenerateAbility(seed);
    }

    //stats for ability
    AbilityEffect Ability_effect;
    AbilityTarget Ability_target;
    AbilityAOE Ability_aoe;
    int strength;
    int duration;
    int delay;
    //cost

    //used for generating a random ability
    public void GenerateAbility(string seed)
    {
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

            AbilityBase ability = new AbilityBase(
                Ability_effect,
                Ability_target,
                Ability_aoe,
                strength,
                duration,
                delay
                );

            Debug.Log(
                "Effect: " + ability.Ability_effect + "\n" +
                "Target: " + ability.Ability_target + "\n" +
                "AoE: " + ability.Ability_aoe + "\n" +
                "Strength: " + ability.strength + "\n" +
                "Duration: " + ability.duration + "\n" +
                "Delay: " + ability.delay
                );
        }
    }

    private string GenerateSeed()
    {
        //effect; damage, heal 2 
        //target; any, row, lane, all 4
        //area; single, chain, row, lane, all 5
        //strength; 0 - 9 10
        //duration; 0-9 10
        //delay; 0-9 10
        //cost 0-10 HP / 0-9 Stun

        string random_effect = ConvertIntToAlphabet(Random.Range(1, 3));
        string random_target = ConvertIntToAlphabet(Random.Range(1, 5));
        string random_area = ConvertIntToAlphabet(Random.Range(1, 6));
        string random_strength = ConvertIntToAlphabet(Random.Range(1, 11));
        string random_duration = ConvertIntToAlphabet(Random.Range(0, 10));
        string random_delay = ConvertIntToAlphabet(Random.Range(0, 10));

        //cost hp
        //cost stun

        //combine randoms into full seed

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
    heal = 2
}

public enum AbilityTarget
{
    any = 1,
    row = 2,
    lane = 3,
    all = 4
}

public enum AbilityAOE
{
    single = 1,
    chain = 2,
    row = 3,
    lane = 4,
    all = 5
}

public class AbilityBase
{
    //effect; damage, heal
    //target; any, row, lane, all
    //area; single, chain, row, lane, all
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
