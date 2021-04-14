using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Typical Customs/Songs/Confrontation")]
public class Confrontation : PureAmbient
{
    [FMODUnity.EventRef]
    public string[] drums;

    [FMODUnity.EventRef]
    public string[] decoratives;

    [Range(0, 1)]
    public float drum_gain;

    public override void Start()
    {
        base.Start();
        player.StartCoroutine(Drums());
    }

    int headgap = 2; //the initial silence when loaded, just to avoid two components lagging differently

    private IEnumerator Drums()
    {
        int thresh = Mathf.CeilToInt(MusicManager.Instance.beat) + headgap;
        while (MusicManager.Instance.beat <= thresh)
        {
            yield return null;
        }

        //the beat at which the note in the pattern occur
        float[][] patterns = { 
            new float[]{ 0, 2, 2.5f, 4, 5, 6, 6.5f },
            new float[] { 0, 2, 2.5f, 4, 5.5f, 7f },
            new float[] { 0, 2, 2.5f, 4.5f, 5.5f, 6.5f, 7f },
            new float[] { 0, 2, 2.5f, 4, 5, 6, 6.5f, 7.5f}
        };

        float duration = 8;
        float coupling_gap = 1.0f / 8.0f;
        while (enabled)
        {
            //each streak of notes follow the same variation and pan
            int pattern_id = Mathf.FloorToInt(Random.value * patterns.Length);
            float[] pattern = patterns[pattern_id];
            int drum_id = Mathf.FloorToInt(Random.value * drums.Length);
            float pan = Random.value * 2.0f - 1.0f;
            float master_gain = drum_gain * (Random.value * 0.2f + 0.8f);
            float coupled = -1;
            float deco_handle = Random.value; //the spawn intensity of decorative notes, shared between ALL spawns

            for (float mark0 = MusicManager.Instance.beat, note_index = 0; MusicManager.Instance.beat - mark0 < duration; )
            {
                //after each note, there is a small chance of a coupled note played shortly after
                

                if (note_index < pattern.Length)
                {
                    //Debug.Log((MusicManager.Instance.beat - mark0) + " for " + pattern[(int)note_index] + " at " + note_index);
                    if ((MusicManager.Instance.beat - mark0) >= pattern[(int)note_index])
                    {
                        //fire instrument
                        FMOD.Studio.EventInstance note = FMODUnity.RuntimeManager.CreateInstance(drums[drum_id]);
                        note.setParameterByName("pan", pan);
                        note.setVolume(master_gain - Random.value * 0.1f);
                        note.start();
                        note.release();

                        if (Random.value < 0.5f)
                        {
                            //Debug.Log(note_index + " has coupled note! t=" + MusicManager.Instance.beat);
                            coupled = mark0 + pattern[(int)note_index] + coupling_gap;
                        }

                        //at pattern head, there is a possibility to spawn a decorative note
                        if (note_index == 0 && Random.value < 0.5f)
                        {
                            FMOD.Studio.EventInstance dec = FMODUnity.RuntimeManager.CreateInstance(
                                decoratives[Mathf.FloorToInt(Random.value * decoratives.Length)]
                                );
                            note.setParameterByName("pan", Random.value * 2.0f - 1.0f);
                            dec.setVolume(master_gain - Random.value * 0.1f);
                            dec.start();
                            dec.release();
                        }

                        note_index++;
                    }
                }

                if (coupled > 0 && MusicManager.Instance.beat >= coupled)
                {
                    //Debug.Log("fire coupled note! t= " + MusicManager.Instance.beat);
                    coupled = -1;

                    //fire coupled instrument
                    FMOD.Studio.EventInstance note = FMODUnity.RuntimeManager.CreateInstance(drums[drum_id]);
                    note.setParameterByName("pan", pan);
                    note.setVolume(master_gain - Random.value * 0.1f);
                    note.start();
                    note.release();
                }

                yield return null;
            }
        }
    }
}
