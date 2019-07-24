using _Scripts.Refactor.PlayerScripts;
using UnityEngine;

namespace _Scripts.Refactor.Grid
{
    public class GridParent : MonoBehaviour {

        //Reference to 2 dimensional grid
        public GameObject[,] Grid;
        //Reference to prefab of grid tile
        public GameObject GridTile;

        //Amount Grid tiles to spawn per row and column
        public int rows = 3;
        public int columns = 3;

        //Distance between grid tiles for x and y
        public float xDistance = 2f;
        public float yDistance = 1f;

        private Vector2 gridtile_spawnpoint;

        public int player_number = 0;

        // Use this for initialization
        void Start () {
            Grid = new GameObject[rows,columns];
            BuildGrid();
        }
	
        //Loops over 2 dimensional array to build the grid
        private void BuildGrid()
        {
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    //Set spawn point for the grid tile, compensate for parent by adding the transform position of parent
                    gridtile_spawnpoint.x = transform.position.x + row * xDistance;
                    gridtile_spawnpoint.y = transform.position.y + column * yDistance;

                    Grid[row, column] = Instantiate(GridTile, gridtile_spawnpoint, Quaternion.identity, transform);

                    Grid[row, column].GetComponent<GridTile>().pos_grid_x = row;
                    Grid[row, column].GetComponent<GridTile>().pos_grid_y = column;

                    if (player_number == 1)
                        Grid[row, column].GetComponent<GridTile>().player = PlayerTurn.Player1;
                    else if(player_number == 2)
                        Grid[row, column].GetComponent<GridTile>().player = PlayerTurn.Player2;
                }
            }
        }

        public void SetMovementRings(int unit_row, int unit_column)
        {
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    if (!Grid[row, column].GetComponent<GridTile>().isOccupied)
                    {
                        if (unit_row == row && Mathf.Abs(column - unit_column) < 2)
                        {
                            Grid[row, column].GetComponent<GridTile>().SetMovementRing(true);
                        }
                        else
                        {
                            Grid[row, column].GetComponent<GridTile>().SetMovementRing(false);
                        }
                    }
                    else
                    {
                        Grid[row, column].GetComponent<GridTile>().SetMovementRing(false);
                    }
                }
            }
        }
    }
}