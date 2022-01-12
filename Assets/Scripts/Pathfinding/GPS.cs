using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPS : MonoBehaviour
{
    [SerializeField] private bool showDebug = false;
    [SerializeField] private float cellSize = 2f;
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 20;
    [SerializeField] private float raycastDuration = 1f;
    private Pathfinding pathfinding;
    private bool isGridChecked = false;

    private void Start()
    {
        pathfinding = new Pathfinding(width, height, cellSize, transform.position - new Vector3(cellSize/2f, 0, cellSize/2f));
        if(!isGridChecked)
            CheckWalkable();
    }

    public void Update() 
    {
        if(showDebug)
            pathfinding.ShowDebug();
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 endPos) 
    {
        return pathfinding.FindPath(startPos, endPos);
    }

    public void CheckWalkable()
    {
        isGridChecked = true;
        int width = pathfinding.grid.Width;
        int height = pathfinding.grid.Height;
        int layer_mask = LayerMask.GetMask("Floor");
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                PathNode node = pathfinding.grid.GetGridObject(x, y);
                Vector3 worldPos = pathfinding.grid.GetWorldPosition(x, y, true);
                worldPos += Vector3.up * 30f;
                RaycastHit hit;
                node.isWalkable = false;
                if (Physics.Raycast (worldPos, Vector3.down, out hit, 50f, layer_mask))
                {
                    Debug.DrawLine(worldPos, worldPos + Vector3.down * 50f, Color.blue, raycastDuration);
                    //if (hit.transform.tag == "Floor")
                    node.isWalkable = true;
                }
            }
        }
    }

}
