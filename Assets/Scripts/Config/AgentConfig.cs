using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentConfig
{
    //--
    public int INDEX_START { get { return 1; } }


    //--
    public float CHECKPOINT_OFFSET = 1f;


    //--
    public float CHECKPOINT_RANGE = 2.5f;
    public float CHECKPOINT_ANGLE_MAX { get { return 140f; } }


    //--
    public float VELOCITY_MIN { get { return 0.1f; } }
    public float STEERING_ANGLE { get { return 60f; } }
}