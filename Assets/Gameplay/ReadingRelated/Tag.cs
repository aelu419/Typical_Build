using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tag
{
    public string type;
    private string[] specs;
    public TagAppearanceType appearance;

    public string[] Specs
    {
        get
        {
            return specs;
        }
    }

    public enum TagAppearanceType
    {
        open,
        close,
        self_closing
    }

    public Tag(string[] inside, TagAppearanceType appearance)
    {
        this.appearance = appearance;

        if(inside == null || inside.Length == 0) {
            throw new System.Exception("Tag cannot be empty");
        }

        else
        {
            this.type = inside[0];
            if(inside.Length > 1)
            {
                this.specs = new string[inside.Length - 1];
                for(int i = 0; i < this.specs.Length; i++)
                {
                    this.specs[i] = inside[i + 1];
                }
            }
            else
            {
                this.specs = new string[0];
            }
        }
    }

    public string GetSpecAt(int index)
    {
        if(specs.Length <= index)
        {
            return null;
        }
        else
        {
            return specs[index];
        }
    }

    public override string ToString()
    {
        string param = "";
        for(int i = 0; i < specs.Length; i++)
        {
            param += specs[i] + " ";
        }
        return type + " with specs: "+param;
    }
}
