using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Writer : ScriptableObject
{
    public string input;
    public abstract string Output();
}
