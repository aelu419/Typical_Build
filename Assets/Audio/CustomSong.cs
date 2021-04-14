using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomSong : ScriptableObject
{
    [HideInInspector]
    public Atonal[] atonals;
    [HideInInspector]
    public MonoBehaviour player;

    public abstract void Initialize(MonoBehaviour mb);
    public abstract void Start();

    public bool enabled;
    private void OnEnable()
    {
        enabled = false;
    }
}
