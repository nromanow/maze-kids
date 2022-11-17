using System.Collections.Generic;
using Core.Maze.Cell;
using UnityEngine;

namespace Core.Maze
{
    public class MainMaze : MonoBehaviour
    {
        #region Variables:

        // ------------------------------------------------------
        // User defined variables - set in editor:
        // ------------------------------------------------------
        [Header("Maze generation values:")]
        [Tooltip("How many cells tall is the maze. MUST be an even number. " +
                 "If number is odd, it will be reduced by 1.\n\n" +
                 "Minimum value of 4.")]
        public int mazeRows;

        [Tooltip("How many cells wide is the maze. Must be an even number. " +
                 "If number is odd, it will be reduced by 1.\n\n" +
                 "Minimum value of 4.")]
        public int mazeColumns;

        [Header("Maze object variables:")] [Tooltip("Cell prefab object.")] [SerializeField]
        private GameObject cellPrefab;

        [Tooltip(
            "If you want to disable the main sprite so the cell has no background, set to TRUE. This will create a maze with only walls.")]
        public bool disableCellSprite;

        // ------------------------------------------------------
        // System defined variables - You don't need to touch these:
        // ------------------------------------------------------

        // Variable to store size of centre room. Hard coded to be 2.
        private int centreSize = 1;

        // Dictionary to hold and locate all cells in maze.
        private readonly Dictionary<Vector2, Cell> allCells = new();

        // List to hold unvisited cells.
        private readonly List<Cell> unvisited = new();

        // List to store 'stack' cells, cells being checked during generation.
        private readonly List<Cell> stack = new();

        // Array will hold 4 centre room cells, from 0 -> 3 these are:
        // Top left (0), top right (1), bottom left (2), bottom right (3).
        private readonly Cell[] centreCells = new Cell[4];

        // Cell variables to hold current and checking Cells.
        private Cell currentCell;
        private Cell checkCell;

        // Array of all possible neighbour positions.
        private readonly Vector2[] neighbourPositions = { new(-1, 0), new(1, 0), new(0, 1), new(0, -1) };

        // Size of the cells, used to determine how far apart to place cells during generation.
        private float cellSize;

        private GameObject mazeParent;

        #endregion

        /* This Start run is an example, you can delete this when 
     * you want to start calling the maze generator manually. 
     * To generate a maze is really easy, just call the GenerateMaze() function
     * pass a rows value and columns value as parameters and the generator will
     * do the rest for you. Enjoy!
     */
        private void Start()
        {
            GenerateMaze(mazeRows, mazeColumns);
        }

        private void GenerateMaze(int rows, int columns)
        {
            if (mazeParent != null) DeleteMaze();

            mazeRows = rows;
            mazeColumns = columns;
            CreateLayout();
        }

        // Creates the grid of cells.
        private void CreateLayout()
        {
            InitValues();

            // Set starting point, set spawn point to start.
            var startPos = new Vector2(-(cellSize * (mazeColumns / 2)) + cellSize / 2,
                -(cellSize * (mazeRows / 2)) + cellSize / 2);
            var spawnPos = startPos;

            for (var x = 1; x <= mazeColumns; x++)
            {
                for (var y = 1; y <= mazeRows; y++)
                {
                    GenerateCell(spawnPos, new Vector2(x, y));

                    // Increase spawnPos y.
                    spawnPos.y += cellSize;
                }

                // Reset spawnPos y and increase spawnPos x.
                spawnPos.y = startPos.y;
                spawnPos.x += cellSize;
            }

            CreateCentre();
            RunAlgorithm();
            MakeExit();
        }

        // This is where the fun stuff happens.
        private void RunAlgorithm()
        {
            // Get start cell, make it visited (i.e. remove from unvisited list).
            unvisited.Remove(currentCell);

            // While we have unvisited cells.
            while (unvisited.Count > 0)
            {
                var unvisitedNeighbours = GetUnvisitedNeighbours(currentCell);
                if (unvisitedNeighbours.Count > 0)
                {
                    // Get a random unvisited neighbour.
                    checkCell = unvisitedNeighbours[Random.Range(0, unvisitedNeighbours.Count)];
                    // Add current cell to stack.
                    stack.Add(currentCell);
                    // Compare and remove walls.
                    CompareWalls(currentCell, checkCell);
                    // Make currentCell the neighbour cell.
                    currentCell = checkCell;
                    // Mark new current cell as visited.
                    unvisited.Remove(currentCell);
                }
                else if (stack.Count > 0)
                {
                    // Make current cell the most recently added Cell from the stack.
                    currentCell = stack[stack.Count - 1];
                    // Remove it from stack.
                    stack.Remove(currentCell);
                }
            }
        }

        private void MakeExit()
        {
            allCells.TryGetValue(new Vector2(6, 4), out var newCell);

            // Remove appropriate wall for chosen edge cell.
            if (newCell.gridPos.x == 0) RemoveWall(newCell.mCell, 1);
            else if (newCell.gridPos.x == mazeColumns) RemoveWall(newCell.mCell, 2);
            else if (newCell.gridPos.y == mazeRows) RemoveWall(newCell.mCell, 3);
            else RemoveWall(newCell.mCell, 4);

            Debug.Log("Maze generation finished.");
        }

        private List<Cell> GetUnvisitedNeighbours(Cell curCell)
        {
            // Create a list to return.
            var neighbours = new List<Cell>();
            // Create a Cell object.
            var nCell = curCell;
            // Store current cell grid pos.
            var cPos = curCell.gridPos;

            foreach (var p in neighbourPositions)
            {
                // Find position of neighbour on grid, relative to current.
                var nPos = cPos + p;
                // If cell exists.
                if (allCells.ContainsKey(nPos)) nCell = allCells[nPos];
                // If cell is unvisited.
                if (unvisited.Contains(nCell)) neighbours.Add(nCell);
            }

            return neighbours;
        }

        // Compare neighbour with current and remove appropriate walls.
        private void CompareWalls(Cell cCell, Cell nCell)
        {
            // If neighbour is left of current.
            if (nCell.gridPos.x < cCell.gridPos.x)
            {
                RemoveWall(nCell.mCell, 2);
                RemoveWall(cCell.mCell, 1);
            }
            // Else if neighbour is right of current.
            else if (nCell.gridPos.x > cCell.gridPos.x)
            {
                RemoveWall(nCell.mCell, 1);
                RemoveWall(cCell.mCell, 2);
            }
            // Else if neighbour is above current.
            else if (nCell.gridPos.y > cCell.gridPos.y)
            {
                RemoveWall(nCell.mCell, 4);
                RemoveWall(cCell.mCell, 3);
            }
            // Else if neighbour is below current.
            else if (nCell.gridPos.y < cCell.gridPos.y)
            {
                RemoveWall(nCell.mCell, 3);
                RemoveWall(cCell.mCell, 4);
            }
        }

        // Function disables wall of your choosing, pass it the script attached to the desired cell
        // and an 'ID', where the ID = the wall. 1 = left, 2 = right, 3 = up, 4 = down.
        private void RemoveWall(MazeCell mazeCell, int wallID)
        {
            if (wallID == 1) mazeCell.wallL.SetActive(false);
            else if (wallID == 2) mazeCell.wallR.SetActive(false);
            else if (wallID == 3) mazeCell.wallU.SetActive(false);
            else if (wallID == 4) mazeCell.wallD.SetActive(false);
        }

        private void CreateCentre()
        {
            // Get the 4 centre cells using the rows and columns variables.
            // Remove the required walls for each.
            centreCells[0] = allCells[new Vector2(1, 1)];
            RemoveWall(centreCells[0].mCell, 4);
            RemoveWall(centreCells[0].mCell, 2);

            // Create a List of ints, using this, select one at random and remove it.
            // We then use the remaining 3 ints to remove 3 of the centre cells from the 'unvisited' list.
            // This ensures that one of the centre cells will connect to the maze but the other three won't.
            // This way, the centre room will only have 1 entry / exit point.
            var rndList = new List<int> { 0 };
            var startCell = rndList[Random.Range(0, rndList.Count)];
            rndList.Remove(startCell);
            currentCell = centreCells[startCell];
            foreach (var c in rndList) unvisited.Remove(centreCells[c]);
        }

        private void GenerateCell(Vector2 pos, Vector2 keyPos)
        {
            // Create new Cell object.
            var newCell = new Cell();

            // Store reference to position in grid.
            newCell.gridPos = keyPos;
            // Set and instantiate cell GameObject.
            newCell.cellObject = Instantiate(cellPrefab, pos, cellPrefab.transform.rotation);
            // Child new cell to parent.
            if (mazeParent != null) newCell.cellObject.transform.parent = mazeParent.transform;
            // Set name of cellObject.
            newCell.cellObject.name = "Cell - X:" + keyPos.x + " Y:" + keyPos.y;
            // Get reference to attached MazeCell.
            newCell.mCell = newCell.cellObject.GetComponent<MazeCell>();
            // Disable Cell sprite, if applicable.
            if (disableCellSprite) newCell.cellObject.GetComponent<SpriteRenderer>().enabled = false;

            // Add to Lists.
            allCells[keyPos] = newCell;
            unvisited.Add(newCell);
        }

        private void DeleteMaze()
        {
            if (mazeParent != null) Destroy(mazeParent);
        }

        private void InitValues()
        {
            // Check generation values to prevent generation failing.
            if (IsOdd(mazeRows)) mazeRows--;
            if (IsOdd(mazeColumns)) mazeColumns--;

            if (mazeRows <= 3) mazeRows = 4;
            if (mazeColumns <= 3) mazeColumns = 4;

            // Determine size of cell using localScale.
            cellSize = 2;

            // Create an empty parent object to hold the maze in the scene.
            mazeParent = new GameObject();
            mazeParent.transform.position = Vector2.zero;
            mazeParent.name = "Maze";
        }

        private bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

        public Cell GetCellByPosition(Vector2 gridPosition)
        {
            allCells.TryGetValue(gridPosition, out var cell);
            return cell;
        }
        
        public class Cell
        {
            public Vector2 gridPos;
            public GameObject cellObject;
            public MazeCell mCell;
        }
    }
}