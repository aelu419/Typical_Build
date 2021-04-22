using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Claw : MonoBehaviour
{
    public GameObject player;
    PlayerControl player_ctrl;
    Animator player_anim, claw_anim;

    [HideInInspector] public Vector3 base_pos, regular_pos;
    [HideInInspector] public float extension;

    [FMODUnity.EventRef]
    public string continuous, hit;
    FMOD.Studio.EventInstance _continuous;
    bool hit_playable;
    float time;

    // Start is called before the first frame update
    void Start()
    {
        player_ctrl = player.GetComponent<PlayerControl>();
        player_anim = player.GetComponent<Animator>();
        claw_anim = GetComponent<Animator>();

        _continuous = FMODUnity.RuntimeManager.CreateInstance(continuous);

        hit_playable = false;
        extension = 0.0f;
        time = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        bool in_climb = player_anim.GetBool("in_climb");
        if (in_climb && !claw_anim.GetBool("in_climb"))
        {
            _continuous.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            _continuous.start();
        }
        _continuous.setVolume(GameSave.Muted ? 0 : 1);
        claw_anim.SetBool("in_climb", in_climb);

        if (!in_climb 
            && !player_anim.GetCurrentAnimatorStateInfo(1).IsName("FinishClimb") 
            && extension == 0)
        {
            base_pos = player.transform.position;
            base_pos.y -= player_ctrl.charSize / 2f;
        }
        else if (player_anim.GetCurrentAnimatorStateInfo(1).IsName("FinishClimb"))
        {
            _continuous.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            //_continuous.release();
        }

        //time cap to prevent glitch at spawn
        if (time < 1.0f)
        {
            transform.position = player.transform.position;
        }
        else
        {
            transform.position = Lerp(player.transform.position, base_pos, Mathf.Clamp(extension + 0.5f, 0, 1));
        }

        //time cap is to prevent sfx from playing at spawn
        if (time > 1.0f && extension > 0.85f & hit_playable)
        {
            hit_playable = false;
            MusicManager.Instance.PlayOneShot(hit, transform.position);
        }
        else
        {
            hit_playable = hit_playable || extension < 0.5f;
        }

        time += Time.deltaTime;
    }

    private Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        t = Mathf.Clamp(t, 0, 1.0f);
        return b * t + a * (1.0f - t);
    }
}
