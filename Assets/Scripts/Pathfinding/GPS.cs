using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// Component that serves as a GPS, generating the pathfinding grid and routes
/// Link: https://unitycodemonkey.com/video.php?v=alU04hvz6L4 
/// </summary>
public class GPS : MonoBehaviour
{
    // --- Grid ---
    [SerializeField] private float cellSize = 2f;
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 20;
    private Pathfinding pathfinding;
    private bool isObstaclesDone = false;

    // --- Debug ---
    [SerializeField] private bool showDebug = false;
    [SerializeField] private float raycastDuration = 1f;

    //--
    private bool isInit = false;

    /// <summary> 
    ///  Start is called before the first frame update.
    /// </summary>
    private void Start()
    {
        Init();
    }

    /// <summary> 
    ///  Initialize the component.
    /// </summary>
    public void Init()
    {
        if(isInit)
            return;
        isInit = true;
        pathfinding = new Pathfinding(width, height, cellSize, showDebug, transform.position - new Vector3(cellSize/2f, 0, cellSize/2f));
        //SetupObstacles();
    }

    /// <summary> 
    ///  Update is called every frame.
    /// </summary>
    private void Update() 
    {
        if(showDebug)
            pathfinding.ShowDebug();
    }

    /// <summary> 
    /// Is a position walkable/an obstacle?
    /// </summary>
    public bool IsWalkable (Vector3 pos)
    {
        return pathfinding.IsWalkable(pos);
    }

    /// <summary> 
    /// Create a path from a starting point to an endpoint.
    /// </summary>
    public List<Vector3> FindPath(Vector3 startPos, Vector3 endPos) 
    {
        return pathfinding.FindPath(startPos, endPos);
    }

    /// <summary> 
    /// Check if each cells are walkable using raycasts.
    /// </summary>
    public void SetupObstacles()
    {
        if(isObstaclesDone)
            return;

        isObstaclesDone = true;
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
