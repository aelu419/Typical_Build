using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

public class Atonal : ContinuousInstrument
{
    public Atonal(
        int index, CustomSong song, string fmod_event_address,
        float noise_velocity, float noise_amplitude, float gain_master) 
        : base(song, fmod_event_address, index, noise_velocity, noise_amplitude, gain_master)
    {
    }

    public IEnumerator Iterate()
    {
        Debug.Log("Atonal instrument active");
        fmod_event = FMODUnity.RuntimeManager.CreateInstance(fmod_event_address);
        fmod_event.start();
        float t = 0;
        while (song.enabled)
        {
            t += Time.deltaTime;
            if (t > 1)
            {
                //update per second
                float noisy = GetNoisyGain(t);
                fmod_event.setVolume(noisy);
                t = 0;
            }
            yield return null;
        }
        Debug.Log("Atonal instrument inactive");
        fmod_event.stop(STOP_MODE.ALLOWFADEOUT);
        yield return null;
    }
}
