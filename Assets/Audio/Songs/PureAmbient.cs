using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Typical Customs/Songs/Ambient Music")]
public class PureAmbient : CustomSong
{
    [FMODUnity.EventRef]
    public string whisper;
    [Range(0, 1)]
    public float whisper_fluctuation_amplitude;
    [Range(0, 5)]
    public float whisper_flucuation_speed;
    [Range(0, 1)]
    public float whisper_gain_master;

    [FMODUnity.EventRef]
    public string ambient;
    [Range(0, 1)]
    public float ambient_fluctuation_amplitude;
    [Range(0, 5)]
    public float ambient_flucuation_speed;
    [Range(0, 1)]
    public float ambient_gain_master;

    protected Atonal whisper_instrument, ambient_instrument;

    //ID of instruments under the song
    const int WHISPER = 0, AMBIENT = 1;

    public override void Initialize(MonoBehaviour mb)
    {
        player = mb;

        atonals = new Atonal[] { whisper_instrument, ambient_instrument };
        whisper_instrument = new Atonal(WHISPER, this, whisper,
            whisper_flucuation_speed, whisper_fluctuation_amplitude, whisper_gain_master);

        ambient_instrument = new Atonal(AMBIENT, this, ambient,
            ambient_flucuation_speed, ambient_fluctuation_amplitude, ambient_gain_master);

        
    }

    public override void Start()
    {
        player.StartCoroutine(whisper_instrument.Iterate());
        player.StartCoroutine(ambient_instrument.Iterate());
    }
}
