using _Scripts.Refactor.Game;
using _Scripts.Refactor.PlayerScripts;
using UnityEngine;

namespace _Scripts.Refactor.Grid
{
    public class GridTile : MonoBehaviour
    {
        //References to child objects
        private Transform movement_ring;
        public bool can_move_here;

        //bool to track if there is a unit on the tile or not
        public bool isOccupied;

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

        //public function to set the rings attached to the tile
        public void SetMovementRing(bool isActive, PlayerTurn currentPlayerTurn)
        {
            if (player == currentPlayerTurn)
            {
                movement_ring.gameObject.SetActive(isActive);
                can_move_here = isActive;
                box_collider.gameObject.SetActive(isActive);
            }
        }
    }
}