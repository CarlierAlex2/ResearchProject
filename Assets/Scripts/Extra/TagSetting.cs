using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagSetting : MonoBehaviour
{
    void Start()
    {
        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            child.gameObject.tag = this.gameObject.tag;
        }
    }
}
