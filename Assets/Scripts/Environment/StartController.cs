using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartController : MonoBehaviour
{
    [SerializeField] private Transform[] startArray;
    private int index = 0;

    private void Start() 
    {
        index = Random.Range(0, startArray.Length);
    }

    public void ResetStart(Transform agentTransform)
    {
        index = (index + 1)% startArray.Length;
        agentTransform.localPosition = startArray[index].localPosition;
        agentTransform.localRotation = startArray[index].localRotation;
    }
}
