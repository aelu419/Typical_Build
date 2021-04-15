using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Portal : MonoBehaviour
{
    [HideInInspector] public PortalData data;
    public bool is_from_cover_prefab;
    [HideInInspector] public TextMeshPro word_block;
    [HideInInspector] public Animator portal_animator; //the important parameter is 'open' (bool)

    public string descriptor;
    [FMODUnity.EventRef]
    public string sound;

    public Sprite[] initial_sprites;

    private event System.Action on_enter_camera;

    //obj is instantiated externally
    private void Start()
    {
        //on_enter_camera += () => Debug.Log("portal enter camera");
        //portal gameobject contains child object ONLY when it is spawn
        //to the right of the script
        //itself, meaning that the child obj is the textmesh
        if(transform.childCount != 0)
        {
            word_block = gameObject.
                transform.GetChild(0).gameObject.GetComponent<TextMeshPro>();
            if (descriptor != null && descriptor != "")
            {
                word_block.text = descriptor;
            }
            else
            {
                word_block.text = data.description;
            }
        }

        if (initial_sprites != null && initial_sprites.Length > 1)
        {
            on_enter_camera += () =>
            {
                GetComponent<SpriteRenderer>().sprite = initial_sprites[
                    Mathf.FloorToInt(Random.value * initial_sprites.Length)
                    ];
            };
        }

        portal_animator = gameObject.GetComponent<Animator>();
        portal_animator.SetBool("open", false);

        //DIFFERENTIATE FRONT BACK PORTAL, ASSIGN BY READING MANAGER, NOT HERE!
        if (is_from_cover_prefab)
        {
            on_enter_camera += () =>
            {
                MusicManager.Instance.PlayOneShot(sound, gameObject.transform.position);
                portal_animator.SetBool("open", true);
            };
        }
    }

    //float amp = 0.01f;
    void Update()
    {
        if (on_enter_camera != null
            && CameraController.Instance.CAM.xMax > transform.position.x
            && transform.position.x > CameraController.Instance.CAM.xMin)
        {
            on_enter_camera();
            on_enter_camera = null;
        }
    }

    public void SetDisplay(PortalData pd, KeyCode k)
    {
        //Debug.Log(pd + ", " + k);
        data = pd;
        data.control = k;
        descriptor = "[" + k.ToString() + "] " + data.description;
    }

    //transition forward to the next scene indicated by this portal's portal data
    public void OnPortalOpen()
    {
        MusicManager.Instance.PlayOneShot(sound, gameObject.transform.position);

        portal_animator.SetBool("open", true);
        //force update player direction to face right (true)
        PlayerControl.Instance.direction = true;

        if (data.IsQuit)
        {
            Debug.Log("quitting, please implement progress saving mechanism!");
            Application.Quit();
            return;
        }
        else
        {
            //transition to specific scene
            //set dispenser to display with next script loaded
            EventManager.Instance.TransitionTo(data.destination, true);
        }
    }

}
