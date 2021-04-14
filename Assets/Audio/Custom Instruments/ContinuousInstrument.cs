using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using FMOD.Studio;

public class ContinuousInstrument
{
    protected Vector2 noise_coord;
    protected float noise_velocity;
    protected float noise_amplitude;

    [HideInInspector]
    public int index;
    protected CustomSong song;
    protected string fmod_event_address;
    protected EventInstance fmod_event;

    public float gain_master;

    public ContinuousInstrument(
        CustomSong song, string fmod_event_address, 
        int index, float noise_velocity, float noise_amplitude,
        float gain_master
        )
    {
        this.song = song;
        this.fmod_event_address = fmod_event_address;
        this.index = index;
        this.noise_coord = new Vector2(0, 0);
        this.noise_velocity = noise_velocity;
        this.noise_amplitude = noise_amplitude;

        this.gain_master = gain_master;
    }

    public float GetNoisyGain()
    {
        float theta = UnityEngine.Random.value * Mathf.PI * 2.0f;
        
        noise_coord += noise_velocity * new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));


        //output -1 ~ 0
        float deviation = -1 * Mathf.PerlinNoise(
                noise_coord.x,
                noise_coord.y
            );

        return gain_master * (1 + noise_amplitude * deviation);
    }


}
