using UnityEngine;
using System.Collections;

public class TimeOutController : MonoBehaviour
{
    public VignetteController VigController;
    public AnimationCurve IntensityCurve;

    public void CancelTimeOut()
    {
        VigController.SetIntensity(0f);
    }

    public void SetCurrentTimeOut(EpisodeChoice choice, float timeLeft)
    {
        if (choice != null)
        {
            VigController.SetIntensity(IntensityCurve.Evaluate(timeLeft));
        }
    }

    public void SetIntensity(float value)
    {
        VigController.SetIntensity(value);
    }
}
