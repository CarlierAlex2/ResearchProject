using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigAgent
{
    public ConfigAgent()
    {
        this.INDEX_START = 1;
        this.CHECKPOINT_OFFSET = 1f;
        this.CHECKPOINT_RANGE = 2.5f;
        this.CHECKPOINT_ANGLE_MAX = 140f;
        this.VELOCITY_MIN = 0.1f;
        this.STEERING_ANGLE = 60f;
    }

    //--
    public int INDEX_START { get; internal set; }
    //--
    public float CHECKPOINT_OFFSET { get; internal set; }
    //--
    public float CHECKPOINT_RANGE { get; internal set; }
    public float CHECKPOINT_ANGLE_MAX { get; internal set; }
    //--
    public float VELOCITY_MIN { get; internal set; }
    public float STEERING_ANGLE { get; internal set; }
}
