using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : MonoBehaviour
{
    public List<Sprite> sprites;
    private UnityEngine.Experimental.Rendering.Universal.Light2D light_;
    private float initial_outer_radius;
    private float n; //for noise purposes
    [HideInInspector] public float l;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[Mathf.FloorToInt(Random.value * sprites.Count)];
        GetComponent<SpriteRenderer>().flipX = Random.value > 0.5f;

        light_ = GetComponentInChildren<UnityEngine.Experimental.Rendering.Universal.Light2D>();
        initial_outer_radius = light_.pointLightOuterRadius;
        n = Random.value * 10;
    }

    // Update is called once per frame
    void Update()
    {
        l = initial_outer_radius * (1 + 0.8f * Mathf.PerlinNoise(Time.time, n));
        light_.pointLightOuterRadius = initial_outer_radius * (1 + 0.8f * Mathf.PerlinNoise(Time.time, n));
    }
}
