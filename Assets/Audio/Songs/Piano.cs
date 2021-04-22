using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Typical Customs/Songs/Piano")]
public class Piano : PureAmbient
{
    [FMODUnity.EventRef]
    public string piano;

    public override void Initialize(MonoBehaviour mb)
    {
        base.Initialize(mb);
    }

    public override void Start()
    {
        base.Start();
        player.StartCoroutine(PianoRoutine());
    }

    IEnumerator PianoRoutine()
    {
        
        while (enabled)
        {
            if (GameSave.Muted) { yield return new WaitForSeconds(5); }
            FMOD.Studio.EventInstance note = FMODUnity.RuntimeManager.CreateInstance(piano);
            note.setParameterByName("pan", Random.value * 2.0f - 1.0f);
            note.setParameterByName("eqL", Random.value);
            note.setParameterByName("eqM", Random.value);
            note.setParameterByName("eqH", Random.value);
            note.setParameterByName("pitch", Mathf.RoundToInt(Random.value * 12 - 6));
            Debug.Log("new note");
            note.start();
            FMOD.Studio.PLAYBACK_STATE state;
            do
            {
                note.getPlaybackState(out state);
                yield return null;
            } while (state != FMOD.Studio.PLAYBACK_STATE.STOPPING);
            note.release();
            yield return new WaitForSeconds(Random.value * 5f);
        }
        yield return null;
    }
}
