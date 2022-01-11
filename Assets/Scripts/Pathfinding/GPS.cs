using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPS : MonoBehaviour
{
    [SerializeField] private float cellSize = 2f;
    [SerializeField] private Vector3 origin = new Vector3(0, 1, 0);
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 20;
    [SerializeField] private float raycastDuration = 1f;
    [SerializeField] private Transform startPos = null;
    [SerializeField] private Transform endPos = null;
    [SerializeField] private bool doTestGrid = false;
    private Pathfinding pathfinding;
    private bool isGridChecked = false;
    private void Start()
    {
        pathfinding = new Pathfinding(width, height, cellSize, origin);
    }

    public void Update() {
        if(!isGridChecked)
        {
            CheckWalkable();
            if(doTestGrid)
                TestGrid();
            isGridChecked = true;
        }

        pathfinding.ShowDebug();
    }

    private void TestGrid()
    {
        var path = pathfinding.FindPath(startPos.position, endPos.position);
        if (path == null)
            Debug.Log("No path found!");
        else
            Debug.Log("Path length" + path.Count);
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 endPos) {
        return pathfinding.FindPath(startPos, endPos);
    }

    private void CheckWalkable()
    {
        int width = pathfinding.grid.Width;
        int height = pathfinding.grid.Height;
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                PathNode node = pathfinding.grid.GetGridObject(x, y);
                Vector3 worldPos = pathfinding.grid.GetWorldPosition(x, y, true);
                worldPos += Vector3.up * 30f;
                RaycastHit hit;
                node.isWalkable = false;
                if (Physics.Raycast (worldPos, Vector3.down, out hit, 50f))
                {
                    Debug.DrawLine(worldPos, worldPos + Vector3.down * 50f, Color.blue, raycastDuration);
                    if (hit.transform.tag == "Floor")
                        node.isWalkable = true;
                }
            }
        }
    }

}
