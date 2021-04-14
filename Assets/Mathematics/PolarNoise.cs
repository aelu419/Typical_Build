using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarNoise : Perlin {

    private int r;

    //perlin noise, but all the grid nodes align according to polar coordinate
    //instead of rectangular
    public PolarNoise(int r, int f) : base(2 * r, 3, f){
        this.r = r;
    }

    //input is in the format of (u, v, t)
    public float Noise(float u, float v, float t)
    {
        //Debug.Log("uv: " + u + " " + v);
        //convert box uv coordinate to polar
        //convert uv to relative to radius
        u = u - 0.5f; //u from [-0.5f, 0.5f]
        v = v - 0.5f; //v from [-0.5f, 0.5f]
        float theta = (Mathf.Atan2(v, u) / Mathf.PI + 1.0f) / 2.0f * w;
        float r_ = Mathf.Sqrt(u * u + v * v) * w;

        r_ = Mathf.Max(0, Mathf.Min(w, r_));
        theta = Mathf.Max(0, Mathf.Min(w, theta));


        //Debug.Log("\t r theta:" + r_ + " " + theta);
        return base.Noise(new VecN(
            new float[]
            {
                r_, theta, t
            }
            ));
    }
}
