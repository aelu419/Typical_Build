using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perlin
{

    protected int w; //the width of square/cube/...
    private int d; //the dimension of the noise map
    private int f; //total flavors, flavors are just additional stuff for customization, nothing to do
                   //with the original perlin noise

    protected VecN center;

    private PerlinNode[] nodes;

    private VecN[] grid_deviations;

    //out puts noise in the range [0, 1]

    /*
     * The noise map is noted by a grid of d-dimensional vectors going from:
     * (0, 0, ..., 0), (1, 0, ..., 0), ..., (w-1, 0, ..., 0)
     * (0, 1, ..., 0), (1, 1, ..., 0), ..., (w-1, 1, ..., 0)
     *                               ...(w-1, w-1, ..., w-1)
     * and each vector corresponds to a measurement of length for a space filling curve
     * which works similar to a w-based number:
     *      digit 0 * 1 + digit 1 * w + ... + digit d-1 * w ^ d-1
     */

    //beware that this method is not very efficient as it uses a rectangular grid instead of triangular

    //initialize with flavor specification
    public Perlin (int w, int d, int f)
    {
        this.w = w;
        this.d = d;
        this.f = f;

        //set the center of this map
        float[] centerC = new float[d];
        for(int i = 0; i < d; i++)
        {
            centerC[i] = (float)w / 2;
        }
        center = new VecN(centerC);

        //initiate the relative positions of adjacent grids to the basis grid
        //use a cursor going from 0 to 2^n-1 to iterate across all the 
        //possible deviations associated with adjacent coordinates
        grid_deviations = new VecN[(int)Mathf.Pow(2, d)];
        for (int i = 0; i < grid_deviations.Length; i++)
        {
            float[] c = new float[d];
            for (int j = 0; j < d; j++)
            {
                c[j] = (i >> j) & 1; //gets the bit value of the j'th digit of i
            }

            grid_deviations[i] = new VecN(c);
        }

        //there are w * ... * w pivots in total, w^d
        nodes = new PerlinNode[(int)Mathf.Pow(w, d)];
        //iterate through the space filling curve by length
        //populate the noise map and configure the adjacency relationships
        for(int i = 0; i < nodes.Length; i++)
        {
            //populate the map and set the gradient vector (see constructor for pn)
            /*VecN gridCoord = Wrap(i);
            for(int j = 0; j < d; j++)
            {
                if(gridCoord.coord[j] == w - 1)
                {
                    //loop back to 
                }
            }*/
            PerlinNode pn = new PerlinNode(Wrap(i));
            nodes[i] = pn;
        }

        for(int i = 0; i < nodes.Length; i++)
        {
            //setup adjacency relationships
            nodes[i].neighbors = FindAdjacency(nodes[i]);
        }

        //Debug.Log(nodes[0].PrintNeighbors());
    }

    //initialize without flavor specification
    public Perlin(int w, int d) : this(w, d, 1)
    {
        //...
    }

    //find the noise in the noisespace corresponding to v
    public float Noise(VecN v)
    {
        if(v.d != d)
        {
            throw new System.ArgumentException(
                "dimension of pixel (" + v.d +
                ") does not match the dimension of this noise map (" + d + ")");
        }

        ArrayList found = FindBasis(v);

        //the basis node
        PerlinNode[] influencers = (found[0] as PerlinNode).neighbors;

        //influences[0] is basically the basis grid node
        //w is how much it deviates from that node, it's values will be
        //for lerping purposes
        VecN w = found[1] as VecN;
        
        float n = RecurLerpTree(influencers, grid_deviations, w, 0, grid_deviations.Length-1, 0);
        return Mathf.Min(Mathf.Max(0, n + 0.5f), 1.0f);
    }

    //recursively lerp the influences of the neighboring grid points to the pixel
    //in the first iteration, the last index is split between 0 and 1
    //  (0, 0, ..., 0) (0, 1, ..., 0) vs. (0, 0, ..., 1) (0, 1, ..., 1)
    //  the weight by which these two groups will be lerped is the faded value
    //  weight = last coordinate of ((the map coordinate of v) - (the basis node))
    //then for the second iteration, the second last index is split between 0 and 1
    //  weight = 2nd last of ...
    //and so on
    private float RecurLerpTree(
            PerlinNode[] influencers, VecN[] deviations, VecN w, 
            int left, int right, int iteration
        )
    {
        if(right - left == 1)
        {
            //only two elements to be processed
            //by def. the iteration number will be d-1 at this point
            //calculate left and right influences with dist dot grad
            float lInf = w.Subtract(deviations[left]).Dot(influencers[left].gradient);
            float rInf = w.Subtract(deviations[right]).Dot(influencers[right].gradient);
            
            //lerp the two influences using the weight of the first digit
            //this is because the 01 difference of the deviations are at the first digit
            float lerped = Lerp(
                lInf,
                rInf,
                Fade(w.coord[d - iteration - 1])
            );
            return lerped;
        }

        //for more than one element, by def the length of influencers should always be even
        //lerp the left and right influences using the last iteration'th value of w\
        return Lerp(
            RecurLerpTree(influencers, deviations, w, left, left + (right - left) / 2, iteration + 1),
            RecurLerpTree(influencers, deviations, w, left + (right - left) / 2 + 1, right, iteration + 1),
            Fade(w.coord[d - iteration - 1])
            );
    }

    //overloaded methods using Unity's own vector classes
    public float Noise(Vector2 v)
    {
        if(d < 2)
        {
            throw new ArgumentException(
                "noisemap's dimension " + d + " is less than the requested vector's dimension" + 2
                );
        }

        else
        {
            //incorporate lower dimensional vectors by adding 0 to the end
            float[] temp = new float[d];
            temp[0] = v.x;
            temp[1] = v.y;

            return Noise(new VecN(temp));
        }
    }

    public float Noise(Vector3 v)
    {
        if (d < 3)
        {
            throw new ArgumentException(
                "noisemap's dimension " + d + " is less than the requested vector's dimension" + 3
                );
        }

        else
        {
            //incorporate lower dimensional vectors by adding 0 to the end
            float[] temp = new float[d];
            temp[0] = v.x;
            temp[1] = v.y;
            temp[2] = v.z;

            return Noise(new VecN(temp));
        }
    }

    public float Noise(Vector4 v)
    {
        if (d < 4)
        {
            throw new ArgumentException(
                "noisemap's dimension " + d + " is less than the requested vector's dimension" + 4
                );
        }

        else
        {
            //incorporate lower dimensional vectors by adding 0 to the end
            float[] temp = new float[d];
            temp[0] = v.x;
            temp[1] = v.y;
            temp[2] = v.z;
            temp[3] = v.w;

            return Noise(new VecN(temp));
        }
    }

    /*
    private float RecurTreeHelper(PerlinNode[] influencers, VecN[] deviations, VecN w, int iteration)
    {
        if (influencers.Length == 2)
        {
            //end condition: there are only two items
            //  at this time, iteration will equal d-1
            //dist dot grad

            //l has no x-deviation
            VecN l = v.Subtract(influencers[0].coord);
            //r has x-deviation of 1, so the subtraction goes the other way
            VecN r = influencers[1].coord.Subtract(v);

            //lerp the two using the last weight

            return Lerp(
                    l.Dot(influencers[0].gradient),
                    r.Dot(influencers[1].gradient),
                    Fade(w.coord[iteration])
                );
        }

        //split the influencers into two halves
        PerlinNode[] left = new PerlinNode[influencers.Length / 2];
        PerlinNode[] right = new PerlinNode[influencers.Length / 2];
        for(int i = 0; i < influencers.Length/2; i++)
        {
            left[i] = influencers[i];
            right[i] = influencers[i + (influencers.Length / 2)];
        }

        return Lerp(
                RecurTreeHelper(left, v, w, iteration + 1),
                RecurTreeHelper(right, v, w, iteration + 1),
                Fade(w.coord[iteration])
            );
    }*/

    private PerlinNode[] FindAdjacency(PerlinNode p)
    {
        //add the deviations to the base coordinate of p, then fetch corresponding gridpoint
        PerlinNode[] result = new PerlinNode[grid_deviations.Length];
        for(int i = 0; i < grid_deviations.Length; i++)
        {
            //length marker of the deviated grid node = length (p's grid position + grid deviation)
            //the modulo deals with positive overflow, the only possibility is
            //being 1 over w-1 in either coordinates. in that case we want to 
            //loop back to 0
            result[i] = nodes[Unwrap(p.coord.Add(grid_deviations[i]).Mod(w))];
        }

        return result;
    }

    //find the base adjacent grid-coordinate for a given d-dimensional vector
    //the first element returned is the basis node
    //the second element is the deviation vector that goes from the pixel to the basis node
    public ArrayList FindBasis(VecN a)
    {
        if(a.d != d)
        {
            throw new System.ArgumentException(
                "dimension of pixel (" + a.d +
                ") does not match the dimension of this noise map (" + d + ")");
        }
        else
        {
            int[] coord = new int[a.d];
            float[] deviation = new float[a.d];
            for(int i = 0; i < a.d; i++)
            {
                //fit the i'th item of the coordinate into the range [0, w]
                float temp = a.coord[i];
                if(temp < 0) //for negative values of a coordinate
                {
                    if(temp % w == 0.0f)
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp += (int)(-1 * temp / w + 1) * w;
                    }
                }
                else if (temp >= w) //for positive overflow (repeat)
                {
                    temp = temp % w;
                }

                deviation[i] = temp % 1.0f;
                //round the coordinate value down to get the basis value
                coord[i] = (int)temp;
            }
            int l = Unwrap(coord);
            if(l < 0 || l >= nodes.Length)
            {
                Debug.Log("requested vector " + a + "which was translated to"
                    + (new VecN(coord)).ToString() + "caused the following issue:");
                throw new ArgumentException("length is out of bounds, " + l);
            }
            ArrayList result = new ArrayList();
            result.Add(nodes[l]);
            result.Add(new VecN(deviation));
            return result;
        }
    }

    //convert a Vec-d instance to the corresponding
    //length coordinate on the space filling curve
    public int Unwrap(VecN a)
    {
        if(a.d != d)
        {
            throw new System.ArgumentException(
                "the coordinate's dimension " + a.d + 
                " does not match with the noise map's dimension " + d);
        }

        int result = 0;
        for (int i = 0; i < d; i++){
            result += (int)a.coord[i] * (int)Mathf.Pow(w, i);
        }
        return result;
    }

    //convert a Vec-d instance to the corresponding
    //length coordinate on the space filling curve
    public int Unwrap(int[] coord)
    {
        if (coord.Length != d)
        {
            throw new System.ArgumentException(
                "the coordinate's dimension " + coord.Length +
                " does not match with the noise map's dimension " + d);
        }

        int result = 0;
        for (int i = 0; i < d; i++)
        {
            result += coord[i] * (int)Mathf.Pow(w, i);
        }
        return result;
    }

    //covert the length coordinate on the space filling curve
    //to its corresponding Vec-d instance
    public VecN Wrap(int l)
    {
        float[] coord = new float[d];
        for(int i = 0; i < d; i++)
        {
            coord[i] = l % (int)Mathf.Pow(w, i + 1);
            coord[i] = (int)coord[i] / (int)Mathf.Pow(w, i);
        }

        return new VecN(coord);
    }

    public static float Lerp(float a, float b, float t)
    {
        if(t > 1 || t < 0)
        {
            throw new System.ArgumentException("t must be within [0, 1]");
        }
        return b * t + a * (1 - t);
    }

    public static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    public override string ToString()
    {
        string result = "Noise map with following coordinates:\n";
        for(int i = 0; i < nodes.Length; i++)
        {
            result += nodes[i].ToString() + "\n";
        }
        return result;
    }

    public class PerlinNode
    {
        public VecN coord;
        public PerlinNode[] neighbors;
        public VecN gradient;
        public int flavor;

        public PerlinNode(VecN coord)
        {
            this.coord = coord;

            //generate random d-dimensional unit vector
            gradient = Gaussian.UnitVecN(coord.d);

        }

        //initialize via cloning
        public PerlinNode(PerlinNode pn)
        {
            this.coord = pn.coord;
            this.gradient = pn.gradient;
        }

        override public string ToString()
        {
            return "node at: " + coord.ToString() + " with gradient " + gradient.ToString();
        }

        public string PrintNeighbors()
        {
            string result = "neighbors:\n";
            for(int i = 0; i < neighbors.Length; i++)
            {
                result += "\t" + neighbors[i].ToString() + "\n";
            }

            return result;
        }
    }
}
