using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Unity.Collections;
using System.Collections;
using Unity.VisualScripting;

public class TileBoard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameManager gameManager;
    public Tile  tilePrefab;
    public TileState[] tileStates;
    private TileGrid grid;
    private List<Tile> tiles;
    private bool wating;

    private Vector2 startTouchPosition, endTouchPosition;

    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);
    }

    public void ClearBoard() {
        foreach (var cell in grid.cells)
        {
            cell.tile = null;
        }
        foreach (var tile in tiles) {
            Destroy(tile.gameObject);
        }
        tiles.Clear();
    }

    public void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        int randomNumber = (Random.value < 0.9f) ? 2 : 4; // 90% is 2, 10% is 4
        int tileIndex = randomNumber == 2 ? 0 : 1;
        tile.SetState(tileStates[tileIndex],randomNumber);
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }
    private void DetectSwipe()
    {
        Vector2 swipeDirection = endTouchPosition - startTouchPosition;

        if (swipeDirection.magnitude < 50) return; 

        if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
        {
            if (swipeDirection.x > 0)
                MoveTiles(Vector2Int.right, grid.width - 2, -1, 0, 1); 
            else
                MoveTiles(Vector2Int.left, 1, 1, 0, 1); 
        }
        else
        {
            if (swipeDirection.y > 0)
                MoveTiles(Vector2Int.up, 0, 1, 1, 1); // Vuốt lên
            else
                MoveTiles(Vector2Int.down, 0, 1, grid.height - 2, -1); // Vuốt xuống
        }
    }
    private void Update()
    {
        if (!wating) {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
                MoveTiles(Vector2Int.up, 0, 1, 1, 1);
            } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
                MoveTiles(Vector2Int.down, 0, 1, grid.height - 2, -1);
            } else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
                MoveTiles(Vector2Int.left, 1, 1, 0, 1);
            } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
                MoveTiles(Vector2Int.right, grid.width - 2, -1, 0, 1);
            }
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    break;

                case TouchPhase.Ended:
                    endTouchPosition = touch.position;
                    DetectSwipe();
                    break;
            }
        }
        
    }

    private void MoveTiles(Vector2Int direction, int startI, int incementI, int startJ, int incrementJ) 
    {
        bool changed = false;
        for (int i = startI; i >= 0 && i < grid.width; i += incementI)
        {
            for (int j = startJ; j >= 0 && j < grid.height; j += incrementJ) 
            {
                TileCell cell = grid.GetCell(i, j);
                if (cell.occupied) {
                    changed |= MoveTile(cell.tile, direction);
                }
            } 
        }
        if (changed) {
            StartCoroutine(WaitForChanges());
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction) 
    {
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        while (adjacent != null) {
            if (adjacent.occupied) {
                
                if (CanMerge(tile, adjacent.tile)) {
                    Merge(tile, adjacent.tile);
                    return true;
                }
                break;
            }

            newCell = adjacent;
            adjacent = grid.GetAdjacentCell(adjacent, direction);

        }

        if (newCell != null) {
            tile.MoveTo(newCell);
            return true;
        }
        return false;
    }


    private bool CanMerge(Tile a, Tile b) {
        return a.number == b.number && !b.locked;
    } 

    private void Merge(Tile a, Tile b) {
        tiles.Remove(a);
        a.Merge(b.cell);
        int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        int number = b.number * 2;
        b.SetState(tileStates[index], number);
        gameManager.IncreaseScore(number);
    }

    private int IndexOf(TileState state) 
    {
        for (int i = 0; i < tileStates.Length; i ++ ) {
            if (state == tileStates[i]) {
                return i;
            }
        }
        return -1;
    }

    private IEnumerator WaitForChanges() 
    {
        wating = true;
        yield return new WaitForSeconds(0.1f);
        wating = false;

        foreach (var tile in tiles) {
            tile.locked = false;
        }

        if (tiles.Count != grid.size) {
            CreateTile();
        }
        
        if (CheckForGameOver()) {
            gameManager.GameOver();
        }
    }

    private bool CheckForGameOver() 
    {
        if (tiles.Count != grid.size) {
            return false;
        }

        foreach (var tile  in tiles)
        {
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);
            
            if (up != null && CanMerge(tile, up.tile)) {
                return false;
            }
            if (down != null && CanMerge(tile, down.tile)) {
                return false;
            }
            if (left != null && CanMerge(tile, left.tile)) {
                return false;
            }
            if (right != null && CanMerge(tile, right.tile)) {
                return false;
            }
        }
        return true;
    }
}
