using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour {

    #region variables

    //hero stats, values set in the inspector
    public int Healthpoints = 1;
    public int Damage = 1;
    public int Defense = 1;
    public int Initiative = 1;
    
    public MainClass main_class = MainClass.Scout;

    private Transform selection_ring;
    private Transform targeting_ring;
    private Transform IsDrafted;

    public bool isDrafted = false;

    public bool hasAction = false;

    //used for when the hero can be targeted by an enemy hero
    public bool isTargeted = false;

    public GameObject BloodSplashPrefab;
    public GameObject BloodParticles;

    //for storing the position of the hero in the level grids
    [System.NonSerialized] public int x_position_grid = 0;
    [System.NonSerialized] public int y_position_grid = 0;

    //bool for moving the hero from tile to tile
    [System.NonSerialized] public bool move_hero = false;
    //end position for the movement of the hero
    [System.NonSerialized] public Vector2 target_position;

    //speed the characters move at from tile to tile
    public float movement_speed = 2f;

    //reference to text meshes
    [System.NonSerialized] public Transform UiText;
    [System.NonSerialized] public Transform UiImages;

    private TextMesh health_text;
    private TextMesh damage_text;
    private TextMesh initiative_text;
        

    #endregion

    private void Awake()
    {
        selection_ring = transform.Find("SelectionRing");
        selection_ring.gameObject.SetActive(false);

        targeting_ring = transform.Find("TargetingRing");
        targeting_ring.gameObject.SetActive(false);

        UiText = transform.Find("UI Text");
        UiImages = transform.Find("UI Images");
        SetUI(false);

        IsDrafted = transform.Find("IsDrafted");
        IsDrafted.gameObject.SetActive(false);

        health_text = UiText.Find("HealthText").GetComponent<TextMesh>();
        damage_text = UiText.Find("DamageText").GetComponent<TextMesh>();
        initiative_text = UiText.Find("InitiativeText").GetComponent<TextMesh>();
    }

    public void SetSelected(bool is_selected)
    {
        selection_ring.gameObject.SetActive(is_selected);

        //set movement rings on tiles in same row
        for (int i = 0; i < 3; i++)
        {
            GameManager.instance.Grid_P1.Grid[x_position_grid, i].GetComponent<GridTile>().SetMovementRing(true);
        }
    }

    public void SetTargeted(bool is_targeted)
    {
        targeting_ring.gameObject.SetActive(is_targeted);
        isTargeted = is_targeted;
    }

    public void SetDrafted(bool is_drafted)
    {
        IsDrafted.gameObject.SetActive(is_drafted);
        isDrafted = is_drafted;
    }

    public void SetAction(bool has_action)
    {
        IsDrafted.gameObject.SetActive(has_action);
        hasAction = has_action;
    }

    public void SetUI(bool show)
    {
        UiText.gameObject.SetActive(show);
        UiImages.gameObject.SetActive(show);
    }

    private void Update()
    {
        if(Healthpoints <= 0)
        {
            //Instantiate(BloodSplashPrefab, transform.position, Quaternion.identity);
            //Destroy(gameObject);
        }    
        else
        {
            health_text.text = Healthpoints.ToString();
            damage_text.text = Damage.ToString();
            initiative_text.text = Initiative.ToString();
        }

        if(move_hero)
        {
            transform.position = Vector2.MoveTowards(transform.position, target_position, movement_speed * Time.deltaTime);

            float distance = Vector2.Distance(transform.position, target_position);

            if(distance == 0)
            {
                move_hero = false;
            }
        }
    }

    public void TakeDamage(int damage_value)
    {
        Healthpoints -= damage_value;
        Instantiate(BloodSplashPrefab, transform.position, Quaternion.identity);
        Instantiate(BloodParticles, transform.position, Quaternion.identity);

        if (Healthpoints <= 0)
        {
            //blood/hit effect
            Instantiate(BloodSplashPrefab, transform.position, Quaternion.identity);

            //update gridtile to no longer be occupied
            switch(gameObject.tag)
            {
                case "HeroP1":
                    GameManager.instance.Grid_P1.Grid[x_position_grid, y_position_grid]
                        .GetComponent<GridTile>().isOccupied = false;
                    break;
                case "HeroP2":
                    GameManager.instance.Grid_P2.Grid[x_position_grid, y_position_grid]
                        .GetComponent<GridTile>().isOccupied = false;
                    break;
            }

            //destroy gameobject
            Destroy(gameObject);
        }
    }
}

[System.Serializable]
public class StatBuff
{
    //buff for hp, dmg or initative
    int buff_value;
    int turns;

    //updates each turn
    //if turns == 0, destroy object
}

[System.Serializable]
public class StatMultiplier
{
    float multiplier;
    int turns;
}
