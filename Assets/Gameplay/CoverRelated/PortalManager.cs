using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public GameObject portal_prefab;
    public RuntimeAnimatorController portal_animator;

    private static PortalManager _instance;
    public static PortalManager Instance => _instance;
    public float margin;
    public Vector2 local;

    public List<PortalData> destinations;
    public List<GameObject> active_portals;

    private static KeyCode[] alphabet =
    {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F,
        KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R,
        KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X,
        KeyCode.Y, KeyCode.Z
    };

    private KeyCode[] registeredListening;

    //portal manager is always initialized to the current portal manager in the scene
    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        EventManager.Instance.OnBackPortalOpen += OnBackPortalOpen;
        EventManager.Instance.OnBackPortalClose += OnBackPortalClose;
        EventManager.Instance.OnScriptLoaded += Configure;
    }

    private void Configure(ScriptObjectScriptable current)
    {
        //fetch destinations from current scene data
        destinations = new List<PortalData>();
        if (current.next == null || current.next.Length == 0)
        {
            destinations.Add(PortalData.GetDefault());
        }
        else
        {
            for (int i = 0; i < current.next.Length; i++)
            {
                //Debug.Log("fetching portal destination: " + current.next[i]);
                PortalData pd = PortalData.Fetch(current.next[i]);
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    //webgl does not support application quitting!
                    if (!pd.Equals(ScriptDispenser.QUIT))
                    {
                        destinations.Add(pd);
                    }
                }
                else
                {
                    destinations.Add(PortalData.Fetch(current.next[i]));
                }
            }
        }
    }

    //open portals according to script and location
    //beginning marks the left middle position of the collection of portal blocks
    private void OnBackPortalOpen(Vector2 beginning)
    {
        if (active_portals.Count > 0)
        {
            return;
        }
        /*
        foreach (PortalData pd in destinations)
        {
            Debug.Log(pd.description);
        }*/
        if (destinations == null || destinations.Count == 0)
        {
            Debug.LogError("no destination specified, skipping portal opening procedure");
            return;
        }

        transform.position = new Vector3(beginning.x + local.x, beginning.y + local.y, 0);
        Vector2 s = portal_prefab.GetComponent<SpriteRenderer>().size;
        float block_raw_height = s.y;
        float block_whole_height = s.y + 2 * margin;

        registeredListening = new KeyCode[destinations.Count];
        active_portals = new List<GameObject>();

        destinations.Reverse();

        for (int i = 0; i < destinations.Count ; i++)
        {
            float portional_h = i - destinations.Count / 2f + 0.5f;
            GameObject go = GameObject.Instantiate(
                portal_prefab,
                new Vector3(
                    transform.position.x + 2,// + Random.value * 0.3f - 0.15f,
                    transform.position.y + portional_h * block_whole_height,// + Random.value * 0.2f - 0.1f,
                    transform.position.z
                    ),
                Quaternion.identity,
                transform);

            Portal p_ = go.GetComponent<Portal>();
            p_.SetDisplay(destinations[i], alphabet[destinations.Count - i - 1]);
            registeredListening[i] = alphabet[destinations.Count - i - 1];

            active_portals.Add(go);
        }
    }

    //close all portals opened
    private void OnBackPortalClose()
    {
        //no longer listen to key presses
        registeredListening = new KeyCode[] { };
        if (active_portals != null && active_portals.Count > 0)
        {
            foreach(GameObject go in active_portals)
            {
                Destroy(go);
            }
        }
        active_portals = new List<GameObject>();
    }

    // Update is called once per frame
    // listen for keypresses
    void Update()
    {
        if (registeredListening != null && registeredListening.Length > 0)
        {
            for(int i = 0; i < registeredListening.Length; i++)
            {
                if (InputGate.Instance.alphabet_typable && Input.GetKeyDown(registeredListening[i]))
                {
                    active_portals[i].GetComponent<Portal>().OnPortalOpen();
                    registeredListening = new KeyCode[] { };
                    return;
                }
            }
        }
    }
}
