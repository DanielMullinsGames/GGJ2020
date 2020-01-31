using UnityEngine;
using System.Collections;

namespace Kilt.EasyRopes2D.Examples
{
    public class NextPreviousScene : MonoBehaviour
    {
        public void Retry()
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
#else
            Application.LoadLevel(Application.loadedLevel);
#endif
        }

        public void NextScene()
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
#else
            Application.LoadLevel(Application.loadedLevel + 1);
#endif
        }

        public void PreviousScene()
        {

#if UNITY_5_3_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex - 1);
#else
            Application.LoadLevel(Application.loadedLevel - 1);
#endif
        }
    }
}
