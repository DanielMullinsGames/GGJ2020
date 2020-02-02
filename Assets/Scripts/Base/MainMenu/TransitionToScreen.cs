using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TransitionToScreen : MonoBehaviour
{
    public int ScreenIndex;
    public bool TransitionOnAnyInput;
    public bool TransitionAfterDelay;
    public float Delay;

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
        SceneManager.LoadScene(ScreenIndex);
    }
}
