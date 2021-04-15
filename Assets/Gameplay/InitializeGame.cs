using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializeGame : MonoBehaviour
{
    public AnimationCurve fade;

    // kickstart game initialization
    public void Initialize()
    {
        GetComponent<UnityEngine.UI.Button>().enabled = false;
        StartCoroutine(LoadAsync());
    }

    private IEnumerator LoadAsync()
    {
        UnityEngine.UI.Image crossfade = transform.parent.GetChild(0)
               .GetComponent<UnityEngine.UI.Image>();
        float t = 0;
        fade.postWrapMode = WrapMode.Clamp;
        AsyncOperation load = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        while (!load.isDone)
        {
            t += Time.deltaTime;
            crossfade.color = new Color(0, 0, 0, fade.Evaluate(t));
            yield return null;
        }
    }
}
