using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartController : MonoBehaviour
{
    [SerializeField] private Transform[] startArray;

    public void ResetStart(Transform agentTransform)
    {
        int index = Random.Range(0, startArray.Length);
        agentTransform.localPosition = startArray[index].localPosition;
        agentTransform.localRotation = startArray[index].localRotation;
    }
}
