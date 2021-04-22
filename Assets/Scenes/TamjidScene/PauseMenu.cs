using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool gamePaused = false;
    public GameObject pauseMenuUI;
    [HideInInspector] public UnityEngine.UI.Button[] buttons;

    public Color normal, deplete;
    UnityEngine.UI.Button resume, mute, save, quit;

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
        if (resume != null)
        {
            resume.interactable = false;
        }
        EventManager.Instance.Game_Paused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gamePaused = false;
    }
    void Pause() {
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
            b.interactable = true;
            switch (b.gameObject.name)
            {
                case "MuteButton":
                    mute = b;
                    b.gameObject.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text
                    = GameSave.Muted ? "Unmute" : "Mute";
                    break;

                case "SaveButton":
                    save = b;
                    b.gameObject.SetActive(!(
                        scene.Equals(ScriptDispenser.MAINMENU) 
                        || scene.Equals(ScriptDispenser.TUTORIAL)
                        ));
                    break;

                case "QuitButton":
                    quit = b;
                    b.gameObject.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text =
                        scene.Equals(ScriptDispenser.MAINMENU) ? "Quit" : "Quit to Menu";
                    break;

                default:
                    resume = b;
                    break;
            }

            if (b.IsActive())
            {
                UnityEngine.UI.Text t = b.GetComponentInChildren<UnityEngine.UI.Text>();
                t.color = normal;
            }
        }
    }
    public void muteGame() {
        Debug.Log("Muting game....");
        MusicManager.Instance.Mute(!GameSave.ToggleMute());

        mute.gameObject.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text
                    = GameSave.Muted ? "Unmute" : "Mute";
    }

    public void saveGame()
    {
        GameSave.SaveProgress();
        UnityEngine.UI.Text t = save.gameObject.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>();
        t.text = "Saved";
        t.color = deplete;
        save.interactable = false;
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
        quit.interactable = false;
    }
}
