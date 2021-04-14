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
        while (song.enabled)
        {
            //subsequent frames
            float noisy = GetNoisyGain();
            fmod_event.setVolume(noisy);
            fmod_event.set3DAttributes(
                FMODUnity.RuntimeUtils.To3DAttributes(MusicManager.Instance.transform)
            );
            yield return null;
        }
        Debug.Log("Atonal instrument inactive");
        fmod_event.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        yield return null;
    }
}
