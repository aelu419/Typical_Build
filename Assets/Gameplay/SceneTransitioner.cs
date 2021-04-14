using System.Collections;
using System.Collections.Generic;

using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneTransitioner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Animator>().SetBool("exitTrigger", false);
        EventManager.Instance.OnStartExitingScene += OnStartExitingScene;
        EventManager.Instance.OnStartEnteringScene += OnStartEnteringScene;
    }

    private void OnStartExitingScene()
    {
        Debug.Log("Exit scene");
        GetComponent<Animator>().SetBool("exitTrigger", true);
        StartCoroutine(BroadCastExitFinished());
    }

    IEnumerator BroadCastExitFinished()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("scene exit complete");
        EventManager.Instance.StartEnteringScene();
    }

    private void OnStartEnteringScene()
    {
        //load scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        EventManager.Instance.OnStartEnteringScene -= OnStartEnteringScene;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
