using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//describes a word (or smallest unit separated by spaces) in the script
public class Word
{
    public Tag[] tags;
    public string content;

    [HideInInspector] public Vector2 L, R; //left and right positions of the cursor associated with this word
    [HideInInspector] public float top;
    [HideInInspector] public float slope;
    [HideInInspector] public int index;
    [HideInInspector] public string cover_type;
    [HideInInspector] public Sprite cover_sprite;
    //private static Sprite default_cover_sprite;

    [HideInInspector] public int typed; //number of typed letters in the word
    [HideInInspector] public WORD_TYPES word_mech; //the mechanism that the word block follows

    [HideInInspector] public TextMeshPro tmp;
    private TMP_CharacterInfo[] char_infos;

    //markers for texual contents of the word block
    [HideInInspector] public bool has_typable; //if the word has any typable letter in it
    [HideInInspector] public int first_typable, last_typable;

    //marks if the word is actually an npc
    [HideInInspector] public bool is_npc;

    public static string
        TYPED_MAT = "AveriaRegular Typed",
        UNTYPED_PLAIN_MAT = "AveriaRegular Untyped Plain",
        UNTYPED_HIDDEN_MAT = "AveriaRegular Untyped Hidden",
        UNTYPED_REFLECTOR_MAT = "AveriaRegular Untyped Reflector";

    public static Material
        TYPED_MAT_,
        UNTYPED_PLAIN_MAT_,
        UNTYPED_HIDDEN_MAT_,
        UNTYPED_REFLECTOR_MAT_;

    public enum WORD_TYPES
    {
        plain,
        hidden,
        reflector
    }

    //load the word materials
    static Word(){
        TYPED_MAT_ = Resources.Load(
            "Fonts & Materials/" + TYPED_MAT) as Material;
        UNTYPED_PLAIN_MAT_ = Resources.Load(
            "Fonts & Materials/" + UNTYPED_PLAIN_MAT) as Material;
        UNTYPED_HIDDEN_MAT_ = Resources.Load(
            "Fonts & Materials/" + UNTYPED_HIDDEN_MAT) as Material;
        UNTYPED_REFLECTOR_MAT_ = Resources.Load(
            "Fonts & Materials/" + UNTYPED_REFLECTOR_MAT) as Material;

        //default_cover_sprite = Resources.Load("DefaultCoverSprite") as Sprite;
    }

    public Word(Tag[] tags, string content, float slope, int index, int typed)
    {
        //the case of EMPTYWORD
        if (tags == null && content == null)
        {
            return;
        }

        this.tags = tags;
        this.content = content;
        this.slope = slope;
        this.index = index;
        this.typed = typed;

        is_npc = false;

        //figure out what the mechanism of the text is, based on the last hanging tag
        //that is mechanism-related
        //also, insert special typables for npc
        for (int i = tags.Length - 1; i >= 0; i--)
        {
            if (tags[i].type.Equals("H"))
            {
                this.word_mech = WORD_TYPES.hidden;
                break;
            }
            else if (tags[i].type.Equals("R"))
            {
                this.word_mech = WORD_TYPES.reflector;
            }
            //tags depleted, use normal style
            else if (i == 0)
            {
                this.word_mech = WORD_TYPES.plain;
            }

            //configure NPC
            if (tags[i].type.Equals("O"))
            {
                //Debug.Log("initiating object type with: " + tags[i]);
                if (tags[i].GetSpecAt(0).Equals("npc"))
                {
                    //Debug.Log("initiating npc");
                    is_npc = true;
                    this.content = "xx";
                }
            }
        }

        has_typable = false;
        first_typable = -1;
        last_typable = -1;
        for(int i = 0; i < this.content.Length; i++)
        {
            if (char.IsLetterOrDigit(this.content[i]))
            {
                //when first encountering typed letter:
                if (!has_typable)
                {
                    has_typable = true;
                    first_typable = i;
                    last_typable = i;
                }
                //for subsequent typed letters
                else
                {
                    last_typable = i;
                }
            }
        }
    }

    public Word(Tag[] tags, string content, float slope, int index)
        : this(tags, content, slope, index, 0) { }

    //instantiate a prefab that holds the current words
    //the parameters are horizontal and vertical beginning of text
    //the actual beginnings are determined after considering slope
    public (Vector2, GameObject) ToPrefab(GameObject pre, Vector2 lCursor)
    {
        GameObject go = MonoBehaviour.Instantiate(pre, lCursor, Quaternion.identity);
        tmp = go.GetComponent<TextMeshPro>();
        if (tmp == null)
            throw new System.Exception("prefab loading error: no TMP component");
        tmp.text = " "+content;
        string style = "";
        for (int i = tags.Length - 1; i >= 0; i--)
        {
            if (tags[i].type.Equals("I"))
            {
                style += "I";
            }
            if (tags[i].type.Equals("B"))
            {
                style += "B";
            }
        }
        tmp.fontStyle =
            (style.IndexOf("I") != -1 ? FontStyles.Italic : FontStyles.Normal)
            |
            (style.IndexOf("B") != -1 ? FontStyles.Bold : FontStyles.Normal);

        SetCharacterMech();

        tmp.ForceMeshUpdate();

        Vector2 rendered_vals = tmp.GetRenderedValues(false);

        go.GetComponent<WordBlockBehavior>().content = this;
        go.tag = "Word Block";

        BoxCollider2D col = go.GetComponent<BoxCollider2D>();
        if (col == null) throw new System.Exception("prefab loading error: no collider");

        //set slope
        float slope_delta = Mathf.Clamp(slope * rendered_vals.x,
            -0.5f * PlayerControl.Instance.charSize,
            0.5f * PlayerControl.Instance.charSize
            );
        /*
        //trim small steps to avoid collision bug
        if (Mathf.Abs(slope_delta) < 0.1f)
        {
            slope_delta = 0;
        }
        */

        //store dimensions of the text block
        L = new Vector2(lCursor.x, lCursor.y);
        top = lCursor.y + rendered_vals.y / 2f;

        //handle objects that cover the word
        cover_type = "";
        GameObject cov = null;
        float cover_w = 0;
        foreach(Tag t in tags)
        {
            if (t.type.Equals("O"))
            {
                cov = FetchCover(t, go);
                cover_w = cov.GetComponent<BoxCollider2D>().size.x;
                break;
            }
        }

        //set collider boundaries
        Vector2 box_size = rendered_vals;
        box_size.x = Mathf.Max(rendered_vals.x, cover_w * (cov == null ? 1 : cov.transform.localScale.x));
        R = new Vector2(lCursor.x + box_size.x, lCursor.y + slope_delta);

        //(deprecated)pad the collider to either sides for a bit to avoid not detecting collision
        //box_size.x += 0.2f;
        col.offset = new Vector2(box_size.x/2, 0);
        col.size = box_size;

        return (new Vector2(R.x, R.y), go);
    }


    //fetch the cover object prefab according to the object tag
    // - see CoverDispenser and CoverObjectScriptable and their respective objects
    private GameObject FetchCover(Tag t, GameObject parent_obj)
    {
        //Debug.Log("fetching cover object for " + t);
        try
        {
            cover_type = t.GetSpecAt(0);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            cover_type = "default";
        }

        //fetch sprite for cover objec
        GameObject cover_child = null;
        foreach (CoverObjectScriptable c in ScriptableObjectManager.Instance.CoverManager.cover_objects)
        {
            if (c.name_.Equals(cover_type))
            {
                //Debug.Log("instantiating type " + cover_type);
                cover_child = GameObject.Instantiate(
                    c.prefab, parent_obj.transform
                    );
                break;
            }
        }

        if (cover_child == null)
        {
            Debug.LogError("cover prefab not found for type: " + cover_type);
        }
        else
        {
            if (cover_child.GetComponent<SpriteRenderer>() != null)
            {
                cover_sprite = cover_child.GetComponent<SpriteRenderer>().sprite;
            }

            cover_child.tag = "Cover Object";

            //fetch image for img tag
            if (t.GetSpecAt(0).Equals("img"))
            {
                string par = t.GetSpecAt(1);
                if (par != null)
                {
                    Sprite sprite_tmp = Resources.Load<Sprite>("Misc/" + par);
                    if (sprite_tmp != null)
                    {
                        cover_sprite = sprite_tmp;
                    }
                    else
                    {
                        //default 
                        cover_sprite = Resources.Load<Sprite>("lamp_1");
                    }
                }
                cover_child.GetComponent<SpriteRenderer>().sprite = cover_sprite;
            }
            else if (t.GetSpecAt(0).Equals("npc"))
            {
                is_npc = true;
                string par = t.GetSpecAt(1);
                cover_child.GetComponent<NPCBehaviour>().Initialize(par == null ? "sample" : par);
            }
            else if (t.GetSpecAt(0).Equals("stump"))
            {
                TextMeshPro txt = cover_child.GetComponent<TextMeshPro>();
                txt.text = Stump.GetInitialText();
                txt.ForceMeshUpdate(true, true);
            }

            BoxCollider2D box = cover_child.GetComponent<BoxCollider2D>();
            //initialize collider
            if (box == null)
            {
                box = cover_child.AddComponent<BoxCollider2D>();
            }
            if (t.GetSpecAt(0) != "stump")
            {
                box.isTrigger = true;
            }

            //Debug.Log(cover_child);
            cover_child.transform.localPosition = new Vector3(
                box.bounds.size.x / 2f,
                (box.bounds.size.y + tmp.GetPreferredValues().y) / 2f,
                0);
        }

        return cover_child;
    }

    public void SetRawText()
    {
        if (tmp == null)
        {
            throw new System.Exception("Word is not attached to TMP yet, do not call set text!");
        }
        else
        {
            tmp.text = " " + content;
            tmp.ForceMeshUpdate();
        }
    }

    public TMP_CharacterInfo[] GetCharacterInfos()
    {
        if (tmp == null)
        {
            throw new System.Exception("Word is not attached to TMP yet, do not call get character info!");
        }
        else
        {
            //reset tmp text to remove tag influence
            SetRawText();
            char_infos = tmp.textInfo.characterInfo;

            //add in tags again
            SetCharacterMech();

            return char_infos;
        }
    }

    public TMP_CharacterInfo GetCharacterInfo(int index)
    {
        //fetch character infos if not already available
        if (char_infos == null) GetCharacterInfos();

        else if (index < 0 || index > content.Length)
        {
            throw new System.Exception("index: " + index + " is out of bounds");
        }

        return char_infos[
            index == 0 ? 0 : index + 1];
    }

    public void SetCharacterMech(int index)
    {
        typed = index;
        SetCharacterMech();
    }

    public void SetCharacterMech()
    {
        //the following skips are to make sure the word does not disappear
        //from the left of the screen before unloading. the exact reason is unknown
        //but i suspect it's caused by the submeshes TMP generates when material
        //tags are used within the TMP box

        //skip if none of of the text is typed out
        if (typed <= 0)
        {
            typed = 0;

            tmp.text = " " + content;
            switch (word_mech)
            {
                case WORD_TYPES.plain:
                    tmp.fontSharedMaterial = UNTYPED_PLAIN_MAT_;
                    break;
                case WORD_TYPES.hidden:
                    tmp.fontSharedMaterial = UNTYPED_HIDDEN_MAT_;
                    break;
                case WORD_TYPES.reflector:
                    tmp.fontSharedMaterial = UNTYPED_REFLECTOR_MAT_;
                    break;
                default:
                    throw new System.Exception("word type not found");
            }
            return;
        }

        //skip if the entire text is typed out
        if (typed >= content.Length)
        {
            typed = content.Length;
            tmp.text = " " + content;
            tmp.fontSharedMaterial = TYPED_MAT_;
            return;
        }

        //the inner material tag inherits the color from the parent, so to set the correct alpha,
        //the parent material's alpha is temporarily overriden
        //when the word is half typed out
        string txt_temp = "<material=\"" + TYPED_MAT + "\"> " //the space is for left spacing between words
            + content.Substring(0, typed) + "</material>";

        switch (word_mech)
        {
            case WORD_TYPES.plain:
                txt_temp += "<material=\"" + UNTYPED_PLAIN_MAT + "\">"
                    + content.Substring(typed) + "</material>";
                break;
            case WORD_TYPES.hidden:
                /*
                txt_temp += "<material=\"" + UNTYPED_HIDDEN_MAT + "\">"
                    + content.Substring(typed) + "</material>";*/
                txt_temp += content.Substring(typed);
                break;
            case WORD_TYPES.reflector:
                txt_temp += "<material=\"" + UNTYPED_REFLECTOR_MAT + "\">"
                    + content.Substring(typed) + "</material>";
                break;
            default:
                throw new System.Exception("word type not found");
        }

        tmp.text = txt_temp;

        /*
        //set style of typed characters
        for (int i = 1; i < typed + 1; i++)
        {
            char_info[i].material = TYPED_MAT;
        }
        //set style of untyped characters
        for (int i = typed; i < content.Length + 1; i++)
        {
            switch (word_mech)
            {
                case WORD_TYPES.plain:
                    char_info[i].material = UNTYPED_PLAIN_MAT;
                    break;
                case WORD_TYPES.hidden:
                    char_info[i].material = UNTYPED_HIDDEN_MAT;
                    break;
                case WORD_TYPES.reflector:
                    char_info[i].material = UNTYPED_REFLECTOR_MAT;
                    break;
                default:
                    throw new System.Exception("word type not found");
            }
        }*/
    }

    public override string ToString()
    {
        string tgs = "";
        for(int i = 0; i < tags.Length; i++)
        {
            tgs += " " + tags[i].ToString();
        }
        return content + ": " + tgs;
    }
}
