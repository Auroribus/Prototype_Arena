using UnityEngine;

public class GridTile : MonoBehaviour {

    //References to child objects
    private Transform movement_ring;
    public bool can_move_here = false;

    //bool to track if there is a unit on the tile or not
    public bool isOccupied = false;

    public PlayerTurn player;

    public int pos_grid_x, pos_grid_y;

    private BoxCollider2D box_collider;

    private void Awake()
    {
        movement_ring = transform.Find("MovementRing");
        movement_ring.gameObject.SetActive(false);

        box_collider = GetComponent<BoxCollider2D>();
        box_collider.gameObject.SetActive(false);
    }

    private void Update()
    {
    }

    //public function to set the rings attached to the tile
    public void SetMovementRing(bool is_active)
    {
        if (player == GameManager.instance.CurrentTurn)
        {
            movement_ring.gameObject.SetActive(is_active);
            can_move_here = is_active;
            box_collider.gameObject.SetActive(is_active);
        }
    }
    
}
