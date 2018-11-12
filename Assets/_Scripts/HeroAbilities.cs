using System.Collections.Generic;
using UnityEngine;

public class HeroAbilities : MonoBehaviour {

    public static HeroAbilities instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

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
}
