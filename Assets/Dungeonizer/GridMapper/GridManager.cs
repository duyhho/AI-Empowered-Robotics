using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject agent;
    public Vector3 gridOrigin = Vector3.zero;  // Origin of the grid in world space
    public float cellSize = 1.0f;  // Size of each grid cell
    public Vector2 environmentSize = new Vector2(200, 200); // example 20x20 meters environment
    public float resizeBuffer = 10f; // The buffer zone near the edges of the grid
    public float gridExpansionAmount = 50f; // The amount to expand the grid by
    public class GridCell
    {
        public bool isVisited = false;
        public bool hasWall = false;
        public bool hasDoor = false;
        public bool hasGround = false;
        public bool hasFire = false;
    }
    GridCell[,] grid;

    void Start()
    {
        Vector3 agentInitialPos = agent.transform.position;
        gridOrigin = new Vector3(agentInitialPos.x - (environmentSize.x / 2), agentInitialPos.y, agentInitialPos.z - (environmentSize.y / 2));
        grid = new GridCell[Mathf.FloorToInt(environmentSize.x / cellSize), Mathf.FloorToInt(environmentSize.y / cellSize)];

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = new GridCell();
            }
        }
    }
    void Update()
    {
        Vector2Int agentGridPos = WorldToGrid(agent.transform.position);

        int xMax = grid.GetLength(0) - 1;
        int yMax = grid.GetLength(1) - 1;

        // Check if the agent is nearing the edges of the grid
        if (agentGridPos.x < resizeBuffer || agentGridPos.y < resizeBuffer ||
            agentGridPos.x > xMax - resizeBuffer || agentGridPos.y > yMax - resizeBuffer)
        {
            // Resize the grid
            environmentSize += new Vector2(gridExpansionAmount, gridExpansionAmount);
            ResizeGrid(environmentSize);
        }
    }

    void ResizeGrid(Vector2 newSize)
    {
        // Save the old grid data
        GridCell[,] oldGrid = grid;

        // Create and initialize a new grid with the new dimensions
        grid = new GridCell[Mathf.FloorToInt(newSize.x / cellSize), Mathf.FloorToInt(newSize.y / cellSize)];

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = new GridCell();

                // If the old grid has data at this index, copy it to the new grid
                if (i < oldGrid.GetLength(0) && j < oldGrid.GetLength(1))
                {
                    grid[i, j] = oldGrid[i, j];
                }
            }
        }
    }


    // Convert a world position to grid coordinates
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - gridOrigin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.z - gridOrigin.z) / cellSize);  // Use z because Unity is Y-up

        x = Mathf.Clamp(x, 0, grid.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, grid.GetLength(1) - 1);

        return new Vector2Int(x, y);
    }

    public void SetVisited(Vector3 worldPosition, bool state = true)
    {
        Vector2Int gridPosition = WorldToGrid(worldPosition);
        grid[gridPosition.x, gridPosition.y].isVisited = state;
    }


    public void SetWall(Vector3 worldPosition, bool state = true)
    {
        Vector2Int gridPosition = WorldToGrid(worldPosition);
        grid[gridPosition.x, gridPosition.y].hasWall = state;
    }

    public void SetDoor(Vector3 worldPosition, bool state = true)
    {
        Vector2Int gridPosition = WorldToGrid(worldPosition);
        grid[gridPosition.x, gridPosition.y].hasDoor = state;
    }

    public void SetGround(Vector3 worldPosition, bool state = true)
    {
        Vector2Int gridPosition = WorldToGrid(worldPosition);
        grid[gridPosition.x, gridPosition.y].hasGround = state;
    }

    public void SetFire(Vector3 worldPosition, bool state = true)
    {
        Vector2Int gridPosition = WorldToGrid(worldPosition);
        grid[gridPosition.x, gridPosition.y].hasFire = state;
    }


    public void ResetGrid()
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y].isVisited = false;
                grid[x, y].hasWall = false;
                grid[x, y].hasDoor = false;
                grid[x, y].hasGround = false;
                grid[x, y].hasFire = false;
            }
        }
    }
    void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                Vector3 cellWorldPosition = new Vector3(gridOrigin.x + x * cellSize - 150f, gridOrigin.y - 1f, gridOrigin.z + y * cellSize);

                Gizmos.color = Color.gray;

                // Set default color to ground if it has ground
                if (grid[x, y].hasGround)
                    Gizmos.color = Color.blue;
                if (grid[x, y].isVisited)
                    Gizmos.color = Color.green;
                // Override with other states if present
                if (grid[x, y].hasDoor)
                    Gizmos.color = Color.yellow;
                if (grid[x, y].hasWall)
                    Gizmos.color = Color.red;
                if (grid[x, y].hasFire)
                    Gizmos.color = Color.magenta;




                Gizmos.DrawCube(cellWorldPosition, new Vector3(cellSize, 0.01f, cellSize));
            }
        }
    }


}
