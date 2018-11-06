using UnityEngine;

public class Hero : MonoBehaviour {

    //hero stats, values set in the inspector
    public int Healthpoints = 1;
    public int Damage = 1;
    public int Defense = 1;
    public int Initiative = 1;

    public HeroType hero_type = HeroType.Ranged;

    private Transform selection_ring;
    private Transform targeting_ring;

    public GameObject BloodSplashPrefab;

    public int x_position_grid = 0;
    public int y_position_grid = 0;

    public bool move_hero = false;
    public Vector2 target_position;

    public float movement_speed = 2f;

    private void Awake()
    {
        selection_ring = transform.Find("SelectionRing");
        selection_ring.gameObject.SetActive(false);

        targeting_ring = transform.Find("TargetingRing");
        targeting_ring.gameObject.SetActive(false);
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
    }

    private void Update()
    {
        if(Healthpoints <= 0)
        {
            Instantiate(BloodSplashPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
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
}
