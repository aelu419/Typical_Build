using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Typical Customs/Story Writer/TextGen1")]
public class TextGen1 : Writer
{
    public override string Output()
    {
        Markov m = new Markov(input, 5);
        string str = "";
        for (int i = 0; i < 10; i++)
        {
            try
            {
                str += m.Run(Mathf.CeilToInt(Random.value * 15f) + 5) + ' ';
                
            }catch(System.Exception _)
            {

            }
        }
        //add scoring, selecting, and concatenating
        return str;
    }
}
