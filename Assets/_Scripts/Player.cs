using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public int PlayerNumber = 1;
    public int ActionsLeft = 3;

    [System.NonSerialized] public GameObject SelectedHero;

    string target_tag = "";
    string own_tag = "";
    List<GameObject> enemy_list;
    GridParent player_grid;
    int melee_range = 2;

    // Update is called once per frame
    void Update () {
        MouseControl();
	}

    public void SetPlayerTurn()
    {
        switch (GameManager.instance.CurrentTurn)
        {
            case PlayerTurn.Player1:
                PlayerNumber = 1;
                own_tag = "HeroP1";
                target_tag = "HeroP2";
                enemy_list = GameManager.instance.HeroList_P2;
                player_grid = GameManager.instance.Grid_P1;
                break;
            case PlayerTurn.Player2:
                PlayerNumber = 2;
                own_tag = "HeroP2";
                target_tag = "HeroP1";
                enemy_list = GameManager.instance.HeroList_P1;
                player_grid = GameManager.instance.Grid_P2;
                break;
        }

    }

    private void MouseControl()
    {       
        if(Input.GetMouseButtonDown(0))
        {
            SetPlayerTurn();

            Debug.Log("turn: " + GameManager.instance.CurrentTurn);

            //send raycast from mouse to world
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log("Collider: " + hit.collider.tag);

                //see if hit is a unit belonging to this player
                if (hit.collider.tag == own_tag)
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

                    HeroType hero_type = selected_hero.hero_type;

                    //set target on all player 2 heros for ranged
                    if (hero_type == HeroType.Ranged)
                    {
                        //ranged can hit any of the enemies heroes
                        foreach (GameObject hero in enemy_list)
                        {
                            hero.GetComponent<Hero>().SetTargeted(true);
                        }
                    }
                    else if (hero_type == HeroType.Melee)
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

                        //melee can only hit the closest lane of enemies
                        foreach (GameObject hero in enemy_list)
                        {
                            if(hero.GetComponent<Hero>().x_position_grid == _max)
                                hero.GetComponent<Hero>().SetTargeted(true);
                        }
                    }
                    else if(hero_type == HeroType.Magic)
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
                else if (SelectedHero != null && hit.collider.tag == target_tag)
                {
                    if (hit.transform.GetComponent<Hero>().isTargeted)
                    {
                        //non magical attacks that only target one hero
                        if (SelectedHero.GetComponent<Hero>().hero_type != HeroType.Magic)
                        {
                            hit.transform.GetComponent<Hero>().Healthpoints -= SelectedHero.GetComponent<Hero>().Damage;
                        }
                        //magical attacks that target a whole row of heros
                        else if (SelectedHero.GetComponent<Hero>().hero_type == HeroType.Magic)
                        {
                            foreach (GameObject hero in enemy_list)
                            {
                                if (hit.transform.GetComponent<Hero>().y_position_grid == hero.GetComponent<Hero>().y_position_grid)
                                {
                                    hero.GetComponent<Hero>().Healthpoints -= SelectedHero.GetComponent<Hero>().Damage;
                                }
                            }
                        }
                    }
                }
                //moving
                else if (SelectedHero != null && hit.collider.tag == "Tile")
                {
                    if (hit.transform.GetComponent<GridTile>().can_move_here)
                    {
                        if (!SelectedHero.GetComponent<Hero>().move_hero)
                        {
                            //set old occupied tile to no longer occupied
                            player_grid.Grid[SelectedHero.GetComponent<Hero>().x_position_grid, SelectedHero.GetComponent<Hero>().y_position_grid]
                                .GetComponent<GridTile>().isOccupied = false;

                            //move hero
                            SelectedHero.GetComponent<Hero>().target_position = hit.transform.position;
                            SelectedHero.GetComponent<Hero>().move_hero = true;

                            //update heros position on grid
                            SelectedHero.GetComponent<Hero>().x_position_grid = hit.transform.GetComponent<GridTile>().pos_grid_x;
                            SelectedHero.GetComponent<Hero>().y_position_grid = hit.transform.GetComponent<GridTile>().pos_grid_y;

                            //set new tile as occupied
                            hit.transform.GetComponent<GridTile>().isOccupied = true;
                        }
                    }
                }
            }
        }

    }
}
