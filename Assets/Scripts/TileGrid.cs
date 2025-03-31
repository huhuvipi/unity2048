using UnityEngine;

public class TileGrid : MonoBehaviour
{
       public TileRow[] rows { get; private set; }
    public TileCell[] cells { get; private set; }

    public int size => cells.Length;
    public int height => rows.Length;

    public int width => size / height;

    private void Awake()
    {
        rows = GetComponentsInChildren<TileRow>();
        cells = GetComponentsInChildren<TileCell>();
    }

    private void Start()
    {
        for (int i = 0; i < rows.Length; i ++) {
            for (int j = 0; j < rows[i].cells.Length; j++)
            {
                rows[j].cells[i].coodinates = new Vector2Int(i, j);
            }
        }
    }


    public TileCell GetCell(int i, int j) 
    {
        if (i >= 0 && i < width && j >= 0 && j <  height) {
            return rows[j].cells[i];
        } else {
            return null;
        }
    }

    public TileCell GetCell(Vector2Int coordinates)
    {
        return GetCell(coordinates.x, coordinates.y);
    }

    public TileCell GetAdjacentCell(TileCell cell, Vector2Int direction) 
    {
        Vector2Int coordinate = cell.coodinates;
        coordinate.x += direction.x;
        coordinate.y -= direction.y;
        return GetCell(coordinate);
    }

    public TileCell GetRandomEmptyCell() {
        int index = Random.Range(0, rows.Length);
        int startingIndex =  index;
        while (cells[index].occupied) 
        {
            index ++;
            if (index >= cells.Length)
            {
                index = 0;
            }       

            if (index == startingIndex) {
                return null;
            } 
        }
        return cells[index];
    }
}
