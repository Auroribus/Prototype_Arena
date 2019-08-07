using _Scripts.Refactor.Game;
using _Scripts.Refactor.PlayerScripts;
using UnityEngine;

namespace _Scripts.Refactor.Grid
{
    public class GridCreator : MonoBehaviour
    {
        //Reference to 2 dimensional grid
        public GameObject[,] Grid;

        //Reference to prefab of grid tile
        private GridTile _gridTile;

        //Amount Grid tiles to spawn per row and column
        public int rows = 3;
        public int columns = 3;

        //Distance between grid tiles for x and y
        public float xDistance = 2f;
        public float yDistance = 1f;

        private Vector2 _gridTileSpawnPoint;

        public int playerNumber = 0;

        // Use this for initialization
        void Start()
        {
            _gridTile = GameManager.Instance.GridTilePrefab;
            Grid = new GameObject[rows, columns];
            BuildGrid();
        }

        //Loops over 2 dimensional array to build the grid
        private void BuildGrid()
        {
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    //Set spawn point for the grid tile, compensate for parent by adding the transform position of parent
                    _gridTileSpawnPoint.x = transform.position.x + row * xDistance;
                    _gridTileSpawnPoint.y = transform.position.y + column * yDistance;

                    Grid[row, column] = Instantiate(_gridTile.gameObject, _gridTileSpawnPoint, Quaternion.identity,
                        transform);

                    Grid[row, column].GetComponent<GridTile>().pos_grid_x = row;
                    Grid[row, column].GetComponent<GridTile>().pos_grid_y = column;

                    switch (playerNumber)
                    {
                        case 1:
                            Grid[row, column].GetComponent<GridTile>().player = PlayerTurn.Player1;
                            break;
                        case 2:
                            Grid[row, column].GetComponent<GridTile>().player = PlayerTurn.Player2;
                            break;
                    }
                }
            }
        }

        public void SetMovementRings(int unit_row, int unit_column)
        {
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
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