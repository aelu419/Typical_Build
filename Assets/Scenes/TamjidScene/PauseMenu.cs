using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool gamePaused = false;
    public GameObject pauseMenuUI;
    public Texture2D hover_cursor, click_cursor;
    public UnityEngine.UI.Button[] buttons;

    private void OnEnable()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        //GetComponent<Canvas>().worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (gamePaused)
            {
                Resume();
            }
            else {
                Pause();
            }
        }
    }
    public void Resume() {
        Cursor.SetCursor(hover_cursor, Vector2.zero, CursorMode.ForceSoftware);
        EventManager.Instance.Game_Paused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gamePaused = false;


    }
    void Pause() {
        Cursor.SetCursor(click_cursor, Vector2.zero, CursorMode.ForceSoftware);
        EventManager.Instance.Game_Paused = true;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gamePaused = true;

        buttons = GetComponentsInChildren<UnityEngine.UI.Button>();

        string scene =
            ScriptableObjectManager.Instance.ScriptManager.CurrentScript.name_;

        bool save_enabled =
            scene.Equals(ScriptDispenser.MAINMENU) || scene.Equals(ScriptDispenser.TUTORIAL);

        foreach (UnityEngine.UI.Button b in buttons)
        {
            switch (b.gameObject.name)
            {
                case "MuteButton":
                    b.gameObject.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text
                    = GameSave.Muted ? "Unmute" : "Mute";
                    break;

                case "SaveButton":
                    b.gameObject.SetActive(!(
                        scene.Equals(ScriptDispenser.MAINMENU) 
                        || scene.Equals(ScriptDispenser.TUTORIAL)
                        ));
                    break;

                case "QuitButton":
                    b.gameObject.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text =
                        scene.Equals(ScriptDispenser.MAINMENU) ? "Quit" : "Quit to Menu";
                    break;

                default:
                    break;
            }
        }
    }
    public void muteGame() {
        Debug.Log("Muting game....");
        MusicManager.Instance.Mute(!GameSave.ToggleMute());

        foreach (UnityEngine.UI.Button b in buttons)
        {
            if (b.gameObject.name.Equals("MuteButton"))
            {
                //Debug.Log("mute button changing -> " + (GameSave.Muted ? "Unmute" : "Mute"));
                b.gameObject.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text
                    = GameSave.Muted ? "Unmute" : "Mute";
            }
        }
    }

    public void saveGame()
    {
        GameSave.SaveProgress();
    }

    public void quitGame()
    {
        if (ScriptableObjectManager.Instance.ScriptManager.CurrentScript.name_.Equals(ScriptDispenser.MAINMENU))
        {
            //Debug.Log("Currently in main menu, quit directly!");
            Application.Quit();
        }
        else
        {
            //Debug.Log("Currently in tutorial, not saving");
            EventManager.Instance.TransitionTo(ScriptDispenser.MAINMENU, false);
            Time.timeScale = 1.0f;
        }
    }
}
