using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public Material default_material;
    // Start is called before the first frame update
    private void Awake()
    {

    }

    void Start()
    {
        EventManager.Instance.OnScriptLoaded += Load;
    }

    private void Load(ScriptObjectScriptable current)
    {
        GetComponent<UnityEngine.UI.Image>().material = 
            current.background == null ? default_material : current.background;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
