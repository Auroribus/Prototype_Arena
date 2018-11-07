using UnityEngine;

public class Hero : MonoBehaviour {

    //hero stats, values set in the inspector
    public int Healthpoints = 1;
    public int Damage = 1;
    public int Defense = 1;
    public int Initiative = 1;

    public MainClass main_class = MainClass.Scout;

    private Transform selection_ring;
    private Transform targeting_ring;

    //used for when the hero can be targeted by an enemy hero
    public bool isTargeted = false;

    public GameObject BloodSplashPrefab;

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
    private Transform UiText;
    private TextMesh health_text;
    private TextMesh damage_text;
    private TextMesh initiative_text;
    
    private void Awake()
    {
        selection_ring = transform.Find("SelectionRing");
        selection_ring.gameObject.SetActive(false);

        targeting_ring = transform.Find("TargetingRing");
        targeting_ring.gameObject.SetActive(false);

        UiText = transform.Find("UI Text");

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

    private void Update()
    {
        if(Healthpoints <= 0)
        {
            Instantiate(BloodSplashPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
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
    }
}
