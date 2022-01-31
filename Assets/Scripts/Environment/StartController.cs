using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// Component that helps with the environment setup for the agent's startposition.
/// </summary>
public class StartController : MonoBehaviour
{
    // --- External objects (starting position objects) ---
    [SerializeField] private Transform[] startArray;

    // --- Extra ---
    private int index = 0;

    /// <summary> 
    ///  Start is called before the first frame update.
    /// </summary>
    private void Start() 
    {
        index = Random.Range(0, startArray.Length);
    }

    //--- RESET ---------------------------------------------------------------------
    /// <summary> 
    /// This method resets the agent to one of the starting positions.
    /// </summary>
    public void ResetStart(Transform agentTransform)
    {
        index = (index + 1)% startArray.Length;
        agentTransform.localPosition = startArray[index].localPosition;
        agentTransform.localRotation = startArray[index].localRotation;
    }
}
