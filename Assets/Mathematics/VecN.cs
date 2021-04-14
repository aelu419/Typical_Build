using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VecN
{
    public int d; //dimension of this vector
    public float[] coord; //coordinate of this vector

    public VecN(params float[] coords)
    {
        if(coords == null || coords.Length == 0)
        {
            throw new System.ArgumentException("Missing Parameters when initializing VecN");
        }

        this.coord = coords;
        d = coords.Length;
    }

    public VecN(params int[] coords)
    {
        if (coords == null || coords.Length == 0)
        {
            throw new System.ArgumentException("Missing Parameters when initializing VecN");
        }

        float[] holder = new float[coords.Length];
        for(int i = 0; i < coords.Length; i++)
        {
            holder[i] = coords[i];
        }
        this.coord = holder;
        this.d = coord.Length;
    }

    //notice that none of the functions below modify the original VecN instance

    //return the dot product of two vectors with the same dimensions
    public static float Dot(VecN a, VecN b)
    {
        if(a.d != b.d)
        {
            throw new System.ArgumentException("dimensions do not agree when calculating dot product: "
                + a.d + " and " + b.d);
        }
        else
        {
            float result = 0.0f;

            //Debug.Log("\t\tdot called with returning:");
            for (int i = 0; i < a.d; i++)
            {
                result += a.coord[i] * b.coord[i];
                //Debug.Log("\t\t\t" + i + "th value is" + (a.coord[i] * b.coord[i]));
            }

            return result;
        }
    }

    //return dot product of the current VecN instance with another vector
    public float Dot(VecN a)
    {
        return Dot(this, a);
    }
    
    //return a new VecN instance representing the sum of two
    //VecN instances with the same dimensions
    public static VecN Add(VecN a, VecN b)
    {
        if (a.d != b.d)
        {
            throw new System.ArgumentException("dimensions do not agree when sum: "
                 + a.d + " and " + b.d);
        }
        else
        {
            float[] c = a.coord.Clone() as float[];
            for (int i = 0; i < a.d; i++)
            {
                c[i] = a.coord[i] + b.coord[i];
            }
            return new VecN(c);
        }
    }

    //return a new VecN instance representing the sum of
    //this VecN instance with another vector
    public VecN Add(VecN a)
    {
        return Add(this, a);
    }

    //return a new VecN instance representing the difference of two
    //VecN instances with the same dimensions (a - b), not (b - a)
    public static VecN Subtract(VecN a, VecN b)
    {
        if (a.d != b.d)
        {
            throw new System.ArgumentException("dimensions do not agree when calculating difference: "
                 + a.d + " and " + b.d);
        }
        else
        {
            float[] c = a.coord.Clone() as float[];
            for (int i = 0; i < a.d; i++)
            {
                c[i] = a.coord[i] - b.coord[i];
            }
            return new VecN(c);
        }
    }

    //return a new VecN instance representing this VecN instance minus
    //another VecN instance
    public VecN Subtract(VecN a)
    {
        return Subtract(this, a);
    }

    //return a new VecN instance (for N = 3) representing the cross product
    //of two 3D vectors
    public static VecN Cross(VecN a, VecN b)
    {
        if(a.d != 3 || b.d != 3)
        {
            throw new System.ArgumentException("cross product only takes 3D vectors, not "
                + a.d + " and " + b.d + "D vectors");
        }
        else
        {
            return new VecN(new float[] {
                a.coord[1] * b.coord[2] - a.coord[2] * b.coord[1],
                a.coord[2] * b.coord[0] - a.coord[0] * b.coord[2],
                a.coord[0] * b.coord[1] - a.coord[1] * b.coord[0],
            });
        }
    }

    //return a new VecN instance (for N = 3) representing the cross product
    //of this VecN instance with another 3D vector
    public VecN Cross(VecN a)
    {
        return Cross(this, a);
    }

    //return a new VecN instance representing the result of a VecN instance multiplied
    //by a scalar value
    public static VecN Scale(VecN a, float b)
    {
        float[] c = new float[a.d];
        for(int i = 0; i < a.d; i++)
        {
            c[i] = a.coord[i] * b;
        }
        return new VecN(c);
    }

    //return a new VecN instance representing the result of this VecN instance multiplied
    //by a scalar value
    public VecN Scale(float b)
    {
        return Scale(this, b);
    }

    //return the length of a VecN instance
    public static float Norm(VecN a)
    {
        return Mathf.Sqrt(Dot(a, a));
    }

    //return the length of this VecN instance
    public float Norm()
    {
        return Mathf.Sqrt(Dot(this, this));
    }

    //return a new VecN instance representing the normalized result of
    //a VecN instance
    public static VecN Normalize(VecN a)
    {
        float l = a.Norm();
        if (l != 0.0f)
        {
            return a.Scale(1 / l);
        }
        else
        {
            return a.Scale(1.0f); //not returning a for the sake of cloning
        }
    }

    //return a new VecN instance representing the normalized result of
    //this VecN instance
    public VecN Normalize()
    {
        float l = this.Norm();
        if (l != 0.0f)
        {
            return this.Scale(1 / l);
        }
        else
        {
            return this.Scale(1.0f); //not returning a for the sake of cloning
        }
    }


    //return the radian angle of a VecN instance
    public static float Angle(VecN a)
    {
        if(a.d != 2)
        {
            throw new System.ArgumentException("angle only applies to 2D vectors, not "
                + a.d +"D");
        }
        return Mathf.Atan2(a.coord[1], a.coord[0]);
    }

    //return the radian angle of this VecN instance
    public float Angle()
    {
        if (this.d != 2)
        {
            throw new System.ArgumentException("angle only applies to 2D vectors, not "
                + this.d + "D");
        }
        return Mathf.Atan2(this.coord[1], this.coord[0]);
    }

    //returns a new VecN instance representing the modulo b version of a VecN instance
    //basically applies mod b to every coordinate of a
    public static VecN Mod(VecN a, float b)
    {
        float[] coord = new float[a.d];
        for(int i = 0; i < a.d; i++)
        {
            coord[i] = a.coord[i] % b;
        }
        return new VecN(coord);
    }

    //returns a new VecN instance representing the modulo b version of this VecN instance
    //basically applies mod b to every coordinate of this vector
    public VecN Mod(float b)
    {
        return Mod(this, b);
    }

    //clones the current VecN instance
    public VecN Clone()
    {
        float[] cloneC = new float[d];
        for(int i = 0; i < d; i++)
        {
            cloneC[i] = coord[i];
        }
        return new VecN(cloneC);
    }

    override public string ToString()
    {
        string result = "Vec" + d.ToString() + " - (";
        for(int i = 0; i < d - 1; i++)
        {
            result += coord[i].ToString()+", ";
        }
        result += coord[d - 1].ToString() + ")";
        return result;
    }

}
