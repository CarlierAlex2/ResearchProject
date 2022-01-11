using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeTag : int
{
    NONE = 0,
    OPEN = 1,
    CLOSED = 2,
    NOT_WALKABLE = 3,
    PATH = 4
}

public class PathNode
{
    private Grid<PathNode> grid;
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;
    public bool isWalkable = true;
    public NodeTag nodeTag = 0;
    public PathNode cameFromNode;

    public PathNode(Grid<PathNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void InitNode()
    {
        gCost = int.MaxValue;
        CalculateFCost();
        cameFromNode = null;
        if(isWalkable)
        {
            nodeTag = NodeTag.NONE;
        }
        else
        {
            nodeTag = NodeTag.NOT_WALKABLE;
        }
    }

    public override string ToString()
    {
        if(!isWalkable)
            return "-";
        return fCost + "\n" + gCost + " + " + hCost; //x + "," + y + "=" + 
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
}
