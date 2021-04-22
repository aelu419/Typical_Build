using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class InitializeGame : MonoBehaviour
{
    public AnimationCurve fade;
    [FMODUnity.BankRef]
    public List<string> banks;
    public ScriptDispenser text_loader;

    public string default_warning;
    public string windows_warning;
    public string mac_warning;
    public string webgl_warning;

    private void Start()
    {
        //initialize warning text specific to platform
        TextMeshProUGUI warning = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        switch(Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                warning.text = windows_warning == null || windows_warning.Length == 0 ? default_warning : windows_warning;
                break;
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                warning.text = mac_warning == null || mac_warning.Length == 0 ? default_warning : mac_warning;
                break;
            case RuntimePlatform.WebGLPlayer:
                warning.text = webgl_warning == null || webgl_warning.Length == 0 ? default_warning : webgl_warning;
                break;
            default:
                warning.text = default_warning;
                break;
        }
    }

    // kickstart game initialization
    public void Initialize()
    {
        GetComponent<UnityEngine.UI.Button>().interactable = false;
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        UnityEngine.UI.Image crossfade = transform.parent.GetChild(0)
            .GetComponent<UnityEngine.UI.Image>();
        float t = 0;
        fade.postWrapMode = WrapMode.Clamp;
        while (t < 1)
        {
            t += Time.deltaTime;
            crossfade.color = new Color(0, 0, 0, fade.Evaluate(t));
            yield return null;
        }
        StartCoroutine(LoadAsync());

        //load scripts
        text_loader.LoadScripts();
    }

    private IEnumerator LoadAsync()
    {
        ScriptDispenser.Load();
        foreach (string b in banks)
        {
            FMODUnity.RuntimeManager.LoadBank(b);
            while (!FMODUnity.RuntimeManager.HasBankLoaded(b))
            {
                yield return null;
            }
            Debug.Log("Loaded bank " + b);
        }
        // -deprecated-: fmod loading now kickstarted via script
        // GetComponent<FMODUnity.StudioListener>().enabled = true;


        AsyncOperation load = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        while (!load.isDone)
        {
            yield return null;
        }
    }
}
