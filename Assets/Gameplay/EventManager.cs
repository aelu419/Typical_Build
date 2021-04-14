using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private static EventManager _instance;
    public static EventManager Instance => _instance;

    private void Awake()
    {
        //Debug.Log("Event Manager instantiated");
        _instance = this;

        front_portal = false;
        back_portal = false;
    }

    public event Action OnProgression;
    public void RaiseProgression()
    {
        if (OnProgression != null)
        {
            OnProgression();
        }
    }

    public event Action OnRegression;
    public void RaiseRegression()
    {
        if(OnRegression != null)
        {
            OnRegression();
        }
    }

    public bool script_end_reached = false;
    public event Action OnScriptEndReached;
    public void RaiseScriptEndReached()
    {
        if(OnScriptEndReached != null && !script_end_reached)
        {
            Debug.Log("End of script is reached, a portal should be spawn to quit the current story");
            script_end_reached = true;
            OnScriptEndReached();
        }
    }

    public bool back_portal;
    public event Action<Vector2> OnBackPortalOpen;
    public void RaiseBackPortalOpen(Vector2 end)
    {
        if (OnBackPortalOpen != null && !back_portal)
        {
            back_portal = true;
            OnBackPortalOpen(end);
        }
    }

    public event Action OnBackPortalClose;
    public void RaiseBackPortalClose()
    {
        if (OnBackPortalClose != null && back_portal)
        {            
            back_portal = false;
            OnBackPortalClose();
        }
    }

    public bool front_portal;
    public event Action OnFrontPortalEngage;
    public void RaiseFrontPortalEngaged()
    {
        if (OnFrontPortalEngage != null)
        {
            front_portal = true;
            OnFrontPortalEngage();
        }
    }
    
    public event Action OnFrontPortalDisengage;
    public void RaiseFrontPortalDisengaged()
    {
        if (OnFrontPortalDisengage != null && front_portal)
        {
            front_portal = false;
            OnFrontPortalDisengage();
        }
    }

    public event Action<ScriptObjectScriptable> OnScriptLoaded;
    public void RaiseOnScriptLoaded(ScriptObjectScriptable current)
    {
        front_portal = false;
        back_portal = false;
        script_end_reached = false;

        if (OnScriptLoaded != null) { OnScriptLoaded(current); }
    }

    public void TransitionTo(string next, bool from_front)
    {

        ScriptDispenser sManager = ScriptableObjectManager.Instance.ScriptManager;
        if (sManager.CurrentScript.name_.Equals(ScriptDispenser.TUTORIAL)
            && next.Equals(ScriptDispenser.MAINMENU)
            )
        {
            GameSave.SaveTutorial(true);
        }
        else if (sManager.CurrentScript.name_.Equals("_credits"))
        {
            Debug.Log("credits reached, the game is passed.");
            GameSave.SavePassedGame(true);
        }

        Debug.Log("transitioning to " + next + " from " + (from_front ? "front" : "back"));
        sManager.SetCurrentScript(next);
        ScriptableObjectManager.Instance.ScriptManager.load_mode = from_front;
        StartExitingScene();
    }

    public event Action OnStartExitingScene;
    public void StartExitingScene()
    {
        if (OnStartExitingScene != null)
        {
            Debug.Log("exiting scene");
            OnStartExitingScene();
        }
    }

    public event Action OnStartEnteringScene;
    public void StartEnteringScene()
    {
        if (OnStartEnteringScene != null)
        {
            front_portal = false;
            back_portal = false;
            OnStartEnteringScene();
        }
    }

    private bool _game_paused;
    public bool Game_Paused
    {
        get { return _game_paused; }
        set { _game_paused = value; }
    }
}
