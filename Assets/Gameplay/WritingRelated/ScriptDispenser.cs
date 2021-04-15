using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Typical Customs/Dispensers/Script Dispenser")]
public class ScriptDispenser : ScriptableObject
{
    public ScriptObjectScriptable[] init_scripts;
    [System.NonSerialized]
    public static ScriptObjectScriptable[] scripts;

    public ScriptObjectScriptable[] main_menus;
    public ScriptObjectScriptable tutorial;

    private static ScriptObjectScriptable _current;

    public const int PASSED = 2, HAS_SAVE = 1;

    //public TextAsset parseable;


    public const string
        MAINMENU = "_mainmenu",
        QUIT = "_quit",
        SAVE = "_save",
        TUTORIAL = "_tutorial";

    public static bool first_load = true; //the first time for a script to be loaded, this is set once per game session

    //true: enter from the front of the script
    //false: enter from the back of the script, and set all words as typed out
    //(see ReadingManager for implementation)
    public bool load_mode;

    public ScriptObjectScriptable CurrentScript {
        get
        {
            if (first_load)
            {
                if (AnalyticsSessionInfo.sessionFirstRun)
                {
                    GameSave.ClearSave();
                    Debug.LogError("cleared save");
                    _current = tutorial;
                }
                else if (GameSave.PassedTutorial)
                {
                    int main_menu_flag =
                        (GameSave.HasSavedScene ? HAS_SAVE : 0)
                        | (GameSave.PassedGame ? PASSED : 0);
                    //fetch main menu
                    _current = main_menus[main_menu_flag];
                }
                else
                {
                    _current = tutorial;
                }
            }

            first_load = false;
            string script_state = _current.name_;
            script_state += "\n\tfirst load:" + AnalyticsSessionInfo.sessionFirstRun;
            script_state += ", \n\tpassed tutorial: " + GameSave.PassedTutorial;
            script_state += ", \n\tcurrent: " + (_current == null ? "null" : _current.name_);
            Debug.Log(script_state);
            return _current;
        }
    }

    public string Previous
    {
        get
        {
            ScriptObjectScriptable p = Fetch(CurrentScript.previous);
            if (p != null)
            {
                return p.name_;
            }
            else
            {
                if (GameSave.PassedTutorial && _current.name_.Equals(TUTORIAL))
                {
                    return MAINMENU;
                }
                else
                {
                    return null;
                }
            }
        }
    }

    public void SetCurrentScript(string destination)
    {
        _current = Fetch(destination);
    }

    public ScriptObjectScriptable LoadSaved()
    {
        string s = GameSave.SavedScene;
        if (s.Equals("")) { return null; }
        for(int i = 0; i < scripts.Length; i++)
        {
            if (scripts[i].name_.Equals(s))
            {
                Debug.Log("found game save on " + s);
                return scripts[i];
            }
        }
        Debug.LogError("saved scene called \"" + s + "\" cannot be found");
        return null;
    }

    public ScriptObjectScriptable Fetch(string name)
    {
        if (name.Equals(MAINMENU))
        {
            int main_menu_flag =
                (GameSave.HasSavedScene ? HAS_SAVE : 0)
                | (GameSave.PassedGame ? PASSED : 0);
            //fetch main menu
            return main_menus[main_menu_flag];
        }
        else if (name.Equals(SAVE))
        {
            return LoadSaved();
        }
        else
        {
            foreach (ScriptObjectScriptable s in scripts)
            {
                if (s.name_.Equals(name))
                {
                    return s;
                }
            }
            return null;
        }
    }

    public static void CheckValidity(ScriptObjectScriptable sos)
    {
        OnLoad();
        Debug.Log("checking validity for " + sos.name);
        List<PortalData> portals = new List<PortalData>();
        if (sos.next == null || sos.next.Length == 0)
        {
            throw new System.Exception("script has no next");
        }
        foreach (string n in sos.next)
        {
            portals.Add(PortalData.Fetch(n));
        }
        if (!sos.previous.Equals(""))
        {
            portals.Add(PortalData.Fetch(sos.previous));
        }

        foreach (PortalData p in portals)
        {
            bool found = false;
            foreach (ScriptObjectScriptable s in scripts)
            {
                if (p.destination.Equals(s.name_) || p.destination.Equals(SAVE) || p.destination.Equals(QUIT))
                {
                    found = true;
                }
            }
            if (!found)
            {
                throw new System.Exception(p.destination + " cannot be found!");
            }
        }
    }

    public static event System.Action OnLoad;
    public static void Load()
    {
        OnLoad?.Invoke();
    }

    private void LoadScripts()
    {
        Debug.Log("Start loading scripts");
        ScriptObjectScriptable[] fwd = Resources.LoadAll<ScriptObjectScriptable>("PlotFwd/");
        ScriptObjectScriptable[] bwd = Resources.LoadAll<ScriptObjectScriptable>("PlotBwd/");
        List<ScriptObjectScriptable> all = new List<ScriptObjectScriptable>();
        all.AddRange(fwd);

        //feed all forward plot to generator
        //screen for raw text
        System.Text.StringBuilder raw = new System.Text.StringBuilder();
        foreach (ScriptObjectScriptable s in all)
        {
            s.Cleanse();
            raw.Append(s.CleansedText);
        }

        all.AddRange(bwd);
        all.AddRange(init_scripts);
        all.AddRange(main_menus);

        scripts = all.ToArray();
        Debug.Log("Scripts loaded: " + scripts.Length);
        //System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //sb.Append("loaded scripts:\n\t");
        
        //Debug.Log("all raw text:\n\t" + raw.ToString());
        //feed raw text into generators
        foreach (ScriptObjectScriptable s in scripts)
        {
            if (s.source == ScriptTextSource.SCRIPT)
            {
                s.text_writer.input = raw.ToString();
                //Debug.Log("sample generated text: " + s.Text);
            }
            //sb.Append(s.name_+", ");
        }
        //Debug.Log(sb);
    }

    private void OnEnable()
    {
        OnLoad = LoadScripts;
        first_load = true;
        load_mode = true;
    }
}
