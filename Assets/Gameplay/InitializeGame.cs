using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializeGame : MonoBehaviour
{
    public AnimationCurve fade;
    [FMODUnity.BankRef]
    public List<string> banks;

    // kickstart game initialization
    public void Initialize()
    {
        GetComponent<UnityEngine.UI.Button>().enabled = false;
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
    }

    private IEnumerator LoadAsync()
    {
        ScriptDispenser.Load();
        foreach (string b in banks)
        {
            FMODUnity.RuntimeManager.LoadBank(b, true);
            Debug.Log("Loaded bank " + b);
        }
        while (!FMODUnity.RuntimeManager.HasBankLoaded("Master"))
        {
            yield return null;
        }
        // -deprecated-: fmod loading now kickstarted via script
        // GetComponent<FMODUnity.StudioListener>().enabled = true;

        //load scripts
        ScriptDispenser.Load();

        AsyncOperation load = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        while (!load.isDone)
        {
            yield return null;
        }
    }
}
