using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wobbler : MonoBehaviour
{
    Animator player_anim, anim;
    //[HideInInspector]
    //public float speed;

    public float wobble_magnitude, wobble_speed;

    float time;

    // Start is called before the first frame update
    void Start()
    {
        player_anim = PlayerControl.Instance.GetComponent<Animator>();
        anim = GetComponent<Animator>();
        time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!anim)
        {
            anim = GetComponent<Animator>();
        }
        if (player_anim)
        {
            float speed = player_anim.GetFloat("speed");
            anim.SetFloat("speed", speed);
            anim.SetBool("in_climb", player_anim.GetBool("in_climb"));
        }
        else
        {
            player_anim = PlayerControl.Instance.GetComponent<Animator>();
        }

        time += Time.deltaTime * wobble_speed;

        transform.rotation = Quaternion.Euler(new Vector3(
            transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y,
            wobble_magnitude * Mathf.Sin(time)
            ));
    }
}
