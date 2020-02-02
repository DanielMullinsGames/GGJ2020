using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TransitionToScreen : MonoBehaviour
{
    public int ScreenIndex;
    public bool TransitionOnAnyInput;
    public bool TransitionAfterDelay;
    public float Delay;

    [SerializeField]
    private GameObject fadeObject;

    private void Update()
    {
        if (TransitionAfterDelay && Delay > 0)
        {
            Delay -= Time.deltaTime;
            return;
        }


        if (TransitionOnAnyInput && Input.anyKey)
            TriggerTransition();
        else if (!TransitionOnAnyInput && TransitionAfterDelay)
            TriggerTransition();
    }

    public void TriggerTransition()
    {
        if (fadeObject != null)
        {
            fadeObject.SetActive(true);
            CustomCoroutine.WaitThenExecute(0.25f, LoadScene);
        }
        else
        {
            LoadScene();   
        }
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(ScreenIndex);
    }
}
