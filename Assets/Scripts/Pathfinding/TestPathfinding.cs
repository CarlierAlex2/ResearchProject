using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPathfinding : MonoBehaviour
{
    private float cellSize = 6f;
    private Pathfinding pathfinding;
    private void Start()
    {
        pathfinding = new Pathfinding(20, 10, cellSize, Vector3.zero);
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 pos = Utils.GetMouseWorldPosition();
            int x, y;
            pathfinding.grid.GetXY(pos, out x, out y);
            List<PathNode> path = pathfinding.FindPath(0, 0, x, y);

            if (path != null)
            {
                print("Path found!");
                for (int i = 0; i < path.Count - 1; i++)
                {
                    PathNode node = path[i];
                    PathNode next = path[i+1];

                    Vector3 p1 = new Vector3(node.x, node.y) * cellSize + Vector3.one * cellSize * 0.5f;
                    Vector3 p2 = new Vector3(next.x, next.y) * cellSize + Vector3.one * cellSize * 0.5f;
                    Debug.DrawLine(p1, p2, Color.green, 100f);
                }
            }

            print("Mousepos=" + pos);
            //pathfinding.grid.SetValue(pos, 50);
        }
    }
}
