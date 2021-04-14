using UnityEngine;

[CreateAssetMenu(menuName = "Typical Customs/Game Save Manager")]
public class GameSave : ScriptableObject
{
    public const string 
        VOLUME = "_volume", 
        SCENE = "_scene", 
        TUTORIAL = "_tutorial",
        PASS = "_pass";

    public string override_scene;

    public static void ClearSave()
    {
        Debug.Log("Clearing saved data");
        //write default state if key non-existant
        PlayerPrefs.SetInt(VOLUME, 1);
        PlayerPrefs.SetString(SCENE, "");
        PlayerPrefs.SetInt(TUTORIAL, 0);
        PlayerPrefs.SetInt(PASS, 0);
        PlayerPrefs.Save();
    }

    public static bool ToggleMute()
    {
        bool old_mute_state = PlayerPrefs.GetInt(VOLUME) == 0;
        PlayerPrefs.SetInt(VOLUME, old_mute_state ? 1 : 0);
        PlayerPrefs.Save();
        return old_mute_state;
    }

    public static void SaveScene(string scene_name)
    {
        PlayerPrefs.SetString(SCENE, scene_name);
        PlayerPrefs.Save();
    }

    public static void SaveProgress()
    {
        PlayerPrefs.SetString(SCENE, ScriptableObjectManager.Instance.ScriptManager.CurrentScript.name_);
        PlayerPrefs.Save();
    }

    public static bool Muted
    {
        get
        {
            if (PlayerPrefs.HasKey(VOLUME)) return PlayerPrefs.GetInt(VOLUME) == 0;
            else
            {
                ClearSave();
                return Muted;
            }
        }
    }

    public static string SavedScene { 
        get {
            if (PlayerPrefs.HasKey(SCENE)) return PlayerPrefs.GetString(SCENE);
            else
            {
                ClearSave();
                return SavedScene;
            }
        } 
    }

    public static bool HasSavedScene
    {
        get
        {
            if (PlayerPrefs.HasKey(SCENE)) return !PlayerPrefs.GetString(SCENE).Equals("");
            else
            {
                ClearSave();
                return false;
            }
        }
    }

    public static bool PassedTutorial
    {
        get
        {
            if (PlayerPrefs.HasKey(TUTORIAL)) return PlayerPrefs.GetInt(TUTORIAL) == 1;
            else
            {
                ClearSave();
                return PassedTutorial;
            }
        }
    }

    public static bool PassedGame
    {
        get
        {
            if (PlayerPrefs.HasKey(PASS)) return PlayerPrefs.GetInt(PASS) == 1;
            else
            {
                ClearSave();
                return PassedGame;
            }
        }
    }

    public static void SaveTutorial(bool passed)
    {
        PlayerPrefs.SetInt(TUTORIAL, passed ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("tutorial state set to: " + PlayerPrefs.GetInt(TUTORIAL));
        return;
    }

    public static void SavePassedGame(bool passed)
    {
        PlayerPrefs.SetInt(PASS, passed ? 1 : 0);
        /*
        if (passed)
        {
            Debug.Log("Game complete! Now deleting old save:)");
            PlayerPrefs.SetString(SCENE, "");
        }*/
        PlayerPrefs.Save();
    }
}
