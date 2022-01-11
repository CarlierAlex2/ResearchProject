using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    private Grid<int> grid;
    private void Start()
    {
        grid = new Grid<int>(4, 2, 10f, new Vector3(40, -10, 0));
        grid.SetGridObject(0, 1, 50);

        grid = new Grid<int>(2, 2, 20f, new Vector3(20, 20, 0));

        grid = new Grid<int>(8, 8, 5f, new Vector3(0, -20, 0));
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 pos = Utils.GetMouseWorldPosition();
            print("Mousepos=" + pos);
            grid.SetGridObject(pos, 50);
        }

        if (Input.GetMouseButtonDown(1)) {
            Vector3 pos = Utils.GetMouseWorldPosition();
            var v = grid.GetGridObject(pos);
            Debug.Log("Grid value= " + v);
        }
    }
}
