using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Typical Customs/Create Story Script")]
public class ScriptObjectScriptable : ScriptableObject
{
    public string name_;
    public ScriptTextSource source;
    public Writer text_writer;
    //public TextAsset text_asset;
    [TextArea(20, 20)]
    public string text;
    public string previous;
    public string [] next;

    public CustomSong music;
    public Material background;
    public Vector2 slope_min_max = Vector2.zero;

    [System.NonSerialized]
    private string cleansed_text;
    public string CleansedText { get { return cleansed_text; } }

    public string Text
    {
        get
        {
            if(source == ScriptTextSource.TEXT_ASSET)
            {
                return text;
            }
            else
            {
                return text_writer.Output();
            }
        }
    }

    public void CheckSyntax()
    {
        List<Word> lst;
        if (source == ScriptTextSource.TEXT_ASSET)
        {
            lst = ReadingManager.ParseScript(text);
        }
        else
        {
            lst = ReadingManager.ParseScript(text_writer.Output());
        }
        System.Text.StringBuilder sb = new System.Text.StringBuilder(lst.Count * 10);
        foreach (Word w in lst)
            sb.Append(w.ToString());
        Debug.Log(sb.ToString());

        ScriptDispenser.CheckValidity(this);
    }

    public void Cleanse()
    {
        if (source != ScriptTextSource.TEXT_ASSET)
        {
            cleansed_text = "";
            return;
        }
        //extract raw text
        cleansed_text = Regex.Replace(text, @"<[^>]*>", " ");
        cleansed_text = Regex.Replace(cleansed_text, @"\s+", " ");
    }

    private void OnEnable()
    {
        text = text.Replace("\u2026", "...");
        text = text.Replace('\u2019', '\'');
        text = text.Replace('\u201C', '\"');
        text = text.Replace('\u201D', '\"');
    }
}

public enum ScriptTextSource
{
    TEXT_ASSET,
    SCRIPT
}

