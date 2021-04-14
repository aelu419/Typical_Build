using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gaussian
{
    // generates random number in [0, 1) using marsaglia polar method
    public static float NextNormal() {
        float v1, v2, s;
        do
        {
            v1 = Random.value;
            v2 = Random.value;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1 || s == 0);

        return v1 * Mathf.Sqrt(-2 * Mathf.Log(s) / s);
    }

    public static VecN UnitVecN(int dimension)
    {
        float[] c = new float[dimension];
        for(int i = 0; i < dimension; i++)
        {
            c[i] = 2.0f * NextNormal() - 1.0f;
        }

        return (new VecN(c)).Normalize();
    }
}
