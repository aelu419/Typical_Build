using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputGate : MonoBehaviour
{

    private static InputGate _instance;

    public static InputGate Instance
    {
        get { return _instance; }
    }

    List<GameObject> alphabet_blockers, backspace_blockers;

    public bool alphabet_typable
    {
        get { return alphabet_blockers.Count == 0 && !EventManager.Instance.Game_Paused; }
    }

    public bool backspace_typable
    {
        get { return backspace_blockers == null || backspace_blockers.Count == 0 && !EventManager.Instance.Game_Paused; }
    }

    private void OnEnable()
    {
        _instance = this;
        alphabet_blockers = new List<GameObject>();
        backspace_blockers = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void RegisterAlphabetBlocker(GameObject go)
    {
        alphabet_blockers.Add(go);
    }

    public void UnregisterAlphabetBlocker(GameObject go)
    {
        if (!alphabet_blockers.Remove(go))
        {
           Debug.LogError("cannot find " + go + " in alphabet blocker list");
        }
    }

    public void RegisterBackspaceBlocker(GameObject go)
    {
        backspace_blockers.Add(go);
    }

    public void UnregisterBackspaceBlocker(GameObject go)
    {
        if (!backspace_blockers.Remove(go))
        {
            Debug.LogError("cannot find " + go + " in backspace blocker list");
        }
    }

    public static bool AnyTypableDown ()
    {
        return Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Escape);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
