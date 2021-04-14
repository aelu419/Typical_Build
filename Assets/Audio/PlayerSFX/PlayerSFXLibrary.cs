using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Typical Customs/Dispensers/Player SFX Lib")]
public class PlayerSFXLibrary : ScriptableObject
{
    [FMODUnity.EventRef]
    public string helm_open, helm_close, npc_encounter, npc_talk, collision;
}
