using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torso : MonoBehaviour
{
    public Animator player_anim, anim;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetFloat("speed", player_anim.GetFloat("speed"));
        anim.SetBool("in_climb", player_anim.GetBool("in_climb"));
    }
}
