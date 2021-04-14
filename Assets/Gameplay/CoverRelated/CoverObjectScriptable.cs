using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Typical Customs/Create Cover Object")]
public class CoverObjectScriptable : ScriptableObject
{
    public string name_;
    public GameObject prefab;
}
