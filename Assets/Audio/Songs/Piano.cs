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
        FMOD.Studio.EventInstance note = FMODUnity.RuntimeManager.CreateInstance(piano);
        while (player.enabled)
        {
            note.setParameterByName("pan", Random.value * 2.0f - 1.0f);
            note.setParameterByName("eqL", Random.value);
            note.setParameterByName("eqM", Random.value);
            note.setParameterByName("eqH", Random.value);
            note.setParameterByName("pitch", Mathf.RoundToInt(Random.value * 12 - 6));

            note.start();
            note.release();

            yield return new WaitForSeconds(12.5f + Random.value * 8);
        }
        yield return null;
    }
}
