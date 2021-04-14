using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FrontPortal : MonoBehaviour
{
    TextMeshPro tmp;

    void Start()
    {
        tmp = transform.GetChild(0).GetComponent<TextMeshPro>();
        EventManager.Instance.OnFrontPortalEngage += OnFrontPortalEngage;
        EventManager.Instance.OnFrontPortalDisengage += OnFrontPortalDisengage;
        tmp.enabled = false;
    }

    private void OnFrontPortalEngage()
    {
        if (!tmp.enabled)
        {
            tmp.enabled = true;
        }
        else
        {
            //force update player direction to face left (false)
            PlayerControl.Instance.direction = false;

            EventManager.Instance.TransitionTo(
                ScriptableObjectManager.Instance.ScriptManager.Previous,
                false
                );
        }
    }

    private void OnFrontPortalDisengage()
    {
        tmp.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
