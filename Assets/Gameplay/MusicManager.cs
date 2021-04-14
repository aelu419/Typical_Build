using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public const float SAMPLING_FREQUENCY = 44100.0f;
    private static MusicManager _instance;
    public static MusicManager Instance
    {
        get
        {
            return _instance;
        }
    }

    public float BPM;
    [HideInInspector]
    public float timer;
    [HideInInspector]
    public float BeatLength; //length of 1 beat in SECONDS
    [HideInInspector]
    public float beat;

    //[Range(0, 1)]
    //public float GLOBAL_VOLUME;

    public CustomSong ambient;
    CustomSong playing;

    private void OnEnable()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        timer = 0.0f;
        beat = 0.0f;
        BeatLength = 1.0f / (BPM / 60.0f);
        EventManager.Instance.OnScriptLoaded += LoadSong;
    }

    private void LoadSong(ScriptObjectScriptable current)
    {
        CustomSong song = current.music;
        if (song == null)
        {
            Debug.LogError("no song loaded");
            playing = ambient;
        }
        else
        {
            //Debug.LogError("implement song playing!");
            playing = song;
        }

        playing.Initialize(this);

        if (!GameSave.Muted)
        {
            PlaySong();
        }
    }

    void PlaySong ()
    {
        masterBus = FMODUnity.RuntimeManager.GetBus(masterBusString);
        masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        playing.enabled = true;
        //kickstart all the atonal instruments
        playing.Start();
    }

    private void OnDisable()
    {
        playing.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        beat = timer / 60.0f * BPM;
        transform.position = CameraController.Instance.transform.position;

    }

    IEnumerator pauseAfterDelay(float t)
    {
        yield return new WaitForSeconds(t);
        Debug.Log("song paused");
        playing.enabled = false;
        //yield return null;
    }


    string masterBusString = "bus:/";
    FMOD.Studio.Bus masterBus;
    public void Mute(bool muted)
    {
        //RuntimeManager.PauseAllEvents(muted);
        if (muted)
        {
            masterBus = FMODUnity.RuntimeManager.GetBus(masterBusString);
            masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            StartCoroutine(pauseAfterDelay(1));
        }
        else
        {
            PlaySong();
        }
    }

    public float BeatToMS(float b)
    {
        return b / BPM * 60.0f * 1000.0f;
    }

    public float MSToBeat(float ms)
    {
        return ms / 1000.0f / 60.0f * BPM;
    }

    public void ResetTimer()
    {
        timer = 0.0f;
        beat = 0.0f;
    }

    public void PlayOneShot(string path)
    {
        PlayOneShot(path, transform.position);
    }

    public void PlayOneShot(string path, Vector3 position)
    {
        //Debug.Log("prompt oneshot: " + GameSave.Muted);
        if (!GameSave.Muted)
        {
            FMODUnity.RuntimeManager.PlayOneShot(path, position);
        }
    }
}
