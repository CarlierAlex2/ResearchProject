using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<TGridObject>
{
    // --- VARIABLES ------------------------------------------------------------------
    private int fontSize = 20;
    private int width;
    private int height;
    private float cellSize;
    private TGridObject[,] gridArray;
    private TextMesh[,] textArray;
    public Vector3 originPosition;

    public int Width { get { return width; } }
    public int Height { get { return height; } }
    public float CellSize { get { return cellSize; } }
    public int FontSize { get { return fontSize; } }
    public TextMesh[,] TextArray { get { return textArray; } }

    bool showDebug = false;


    // --- INIT ------------------------------------------------------------------
    public Grid(int width, int height, float cellSize, Vector3 originPosition = default(Vector3), System.Func<Grid<TGridObject>, int, int, TGridObject> createGridObject  =null)
    {
        //public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
        //public class OnGridValueChangedEventArgs : EventArgs
        //{
        //    public int x; 
        //    public int y;
        //}

        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;
        textArray = new TextMesh [width, height];
        
        gridArray = new TGridObject [width, height];
        for(int x = 0; x < gridArray.GetLength(0); x++)
        {
            for(int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        CreateTextGrid();
        if (showDebug)
            ShowDebug();

    }

    private void CreateTextGrid()
    {
        Debug.Log("Grid: " + width + " - "  + height);
        Color color = Color.white;

        for(int x = 0; x < gridArray.GetLength(0); x++)
        {
            for(int y = 0; y < gridArray.GetLength(1); y++)
            {
                Vector3 worldPos = GetWorldPosition(x, y);
                Vector3 textPos = worldPos + new Vector3(cellSize, 0, cellSize) * 0.5f;
                textArray[x,y] = Utils.CreateWorldText(gridArray[x,y].ToString(), null, textPos, fontSize, color, TextAnchor.MiddleCenter);
                textArray[x,y].gameObject.SetActive(false);
            }
        }
    }

    private void ShowDebug()
    {
        Color color = Color.white;

        for(int x = 0; x < gridArray.GetLength(0); x++)
        {
            for(int y = 0; y < gridArray.GetLength(1); y++)
            {
                Debug.Log("[x,y]: " + width + ","  + height);
                Vector3 worldPos = GetWorldPosition(x, y);
                Vector3 textPos = worldPos + new Vector3(cellSize, 0, cellSize) * 0.5f;
                textArray[x,y].text =gridArray[x,y].ToString();
                Debug.DrawLine(worldPos, GetWorldPosition(x, y+1), color, 100f);
                Debug.DrawLine(worldPos, GetWorldPosition(x+1, y), color, 100f);
            }
        }

        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), color, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), color, 100f);
    }

    // --- VALUE ------------------------------------------------------------------
    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, value);
    }

    public void SetGridObject(int x, int y, TGridObject value)
    {
        if ((x >= 0 && x <= width) && (y >= 0 && y <= height))
        {
            gridArray[x, y] = value;
            textArray[x, y].text = value.ToString();
        }
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if ((x >= 0 && x < width) && (y >= 0 && y < height))
        {
            return gridArray[x, y];
        }
        return default(TGridObject);
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }


    // --- EXTRA ------------------------------------------------------------------
    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        Vector3 p = worldPosition - originPosition;
        x = Mathf.FloorToInt(p.x / cellSize);
        y = Mathf.FloorToInt(p.z / cellSize);
    }

    // --- POSITION ------------------------------------------------------------------
    public Vector3 GetWorldPosition(int x, int y, bool isCenter=false)
    {
        Vector3 pos = new Vector3(x, 0, y) * cellSize + originPosition;
        if (isCenter)
            pos += new Vector3(1, 0, 1) * cellSize * 0.5f;
        return pos;
    }
}
