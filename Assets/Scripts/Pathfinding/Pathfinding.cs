using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 15;

    public Grid<PathNode> grid;
    public List<PathNode> openList;
    public List<PathNode> closedList;


    public Pathfinding(int width, int height, float cellSize, bool gridDebugActive, Vector3 origin)
    {
        grid = new Grid<PathNode>(width, height, cellSize, gridDebugActive, origin, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y));
        Debug.Log("Grid created: " + grid.Width + " - "  + grid.Height);
    }

    public void ShowDebug()
    {
        Color color = Color.white;
        int width = grid.Width;
        int height = grid.Height;
        float cellSize = grid.CellSize;
        int fontSize = grid.FontSize;

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                PathNode node = grid.GetGridObject(x, y);
                Vector3 worldPos = grid.GetWorldPosition(x, y);
                Vector3 textPos = worldPos + new Vector3(cellSize, 0, cellSize) * 0.5f;
                string text = grid.GetGridObject(x,y).ToString();

                NodeTag t = (NodeTag)(node.nodeTag);
                switch (t) 
                {
                    case NodeTag.NOT_WALKABLE:
                        color = Color.black;
                        break;
                    case NodeTag.PATH:
                        color = Color.yellow;
                        break;
                    case NodeTag.CLOSED:
                        color = Color.red;
                        break;
                    case NodeTag.OPEN:
                        color = Color.green;
                        break;
                    case NodeTag.NONE:
                        color = Color.white;
                        break;
                }

                grid.TextArray[x,y].text = text;
                grid.TextArray[x,y].color = color;

                Debug.DrawLine(worldPos, grid.GetWorldPosition(x, y+1), color);
                Debug.DrawLine(worldPos, grid.GetWorldPosition(x+1, y), color);
            }
        }

        Debug.DrawLine(grid.GetWorldPosition(0, height), grid.GetWorldPosition(width, height), color);
        Debug.DrawLine(grid.GetWorldPosition(width, 0), grid.GetWorldPosition(width, height), color);
    }

    public List<Vector3> FindPath (Vector3 startPos, Vector3 endPos)
    {
        grid.GetXY(startPos, out int startX, out int startY);
        grid.GetXY(endPos, out int endX, out int endY);
        //Debug.Log("Path from " + startX + ", " + startY + " to " + endX + ", " + endY);
        List<PathNode> path = FindPath(startX, startY, endX, endY);
        if (path == null)
            return null;

        List<Vector3> vectorPath = new List<Vector3>();
        foreach (PathNode n in path)
        {
            Vector3 p = new Vector3(n.x, 0, n.y) * grid.CellSize + new Vector3(1, 0, 1) * grid.CellSize * 0.5f + grid.originPosition;
            vectorPath.Add(p);
        }
        return vectorPath;
    }

    public List<PathNode> FindPath (int startX, int startY, int endX, int endY){
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);
        openList = new List<PathNode> {startNode};
        closedList = new List<PathNode> ();

        for(int x = 0; x < grid.Width; x++)
        {
            for(int y = 0; y < grid.Height; y++)
            {
                PathNode node = grid.GetGridObject(x, y);
                node.InitNode();
            }
        }

        startNode.nodeTag = NodeTag.OPEN;
        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode) //Final
                return CalculatePath(endNode);

            currentNode.nodeTag = NodeTag.CLOSED;
            openList.Remove(currentNode);
            closedList.Add(currentNode);
            

            foreach(PathNode neighbourNode in GetNeighbours(currentNode))
            {
                if (closedList.Contains(neighbourNode)) //skip
                    continue;
                if (!neighbourNode.isWalkable)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int gCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (gCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        neighbourNode.nodeTag = NodeTag.OPEN;
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        return null; //no path found
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int straight = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * straight;
    }

    private PathNode GetLowestFCostNode(List<PathNode> nodeList)
    {
        PathNode lowestFCostNode = nodeList[0];
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].fCost < lowestFCostNode.fCost)
                lowestFCostNode = nodeList[i];
        }
        return lowestFCostNode;
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode n = endNode;
        n.nodeTag = NodeTag.PATH;

        while (n.cameFromNode != null)
        {
            path.Add(n.cameFromNode);
            n = n.cameFromNode;
            n.nodeTag = NodeTag.PATH;
        }
        path.Reverse();
        return path;
    }

    private PathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    private List<PathNode> GetNeighbours(PathNode node)
    {
        return GetNeighboursStraight(node);
    }

    private List<PathNode> GetNeighboursDiagonal(PathNode node)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        for(int x = node.x - 1; x <= node.x + 1; x++)
        {
            for(int y = node.y - 1; y <= node.y + 1; y++)
            {
                PathNode n = GetNode(x, y);
                if (n != null)
                    neighbourList.Add(n);
            }
        }

        return neighbourList;
    }

    private List<PathNode> GetNeighboursStraight(PathNode node)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        for(int x = node.x - 1; x <= node.x + 1; x++)
        {
            PathNode n = GetNode(x, node.y);
            if (n != null)
                neighbourList.Add(n);
        }
        for(int y = node.y - 1; y <= node.y + 1; y++)
        {
            PathNode n = GetNode(node.x, y);
            if (n != null)
                neighbourList.Add(n);
        }

        return neighbourList;
    }
}
