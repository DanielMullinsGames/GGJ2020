using UnityEngine;
using System.Collections;

namespace Kilt.EasyRopes2D.Examples
{
    [RequireComponent(typeof(ParticleSystem))]
    public class AutoDestroyParticle : MonoBehaviour
    {
        #region Private Variables
        [SerializeField]
        private bool m_onlyDeactivate;

        #endregion

        #region Public Properties

        public bool OnlyDeactivate
        {
            get
            {
                return m_onlyDeactivate;
            }
            set
            {
                if (m_onlyDeactivate == value)
                    return;
                m_onlyDeactivate = value;
            }
        }

        #endregion

        #region Unity Functions

        protected virtual void OnEnable()
        {
            StartCoroutine("CheckIfMustDestroy");
        }

        protected virtual void OnDisable()
        {
            StopCoroutine("CheckIfMustDestroy");
        }

        #endregion

        #region Coroutines

        protected virtual IEnumerator CheckIfMustDestroy()
        {
            while (true)
            {
                float start = Time.realtimeSinceStartup;
                float time = 0.5f;
                while (Time.realtimeSinceStartup < start + time)
                    yield return null;

                if (!GetComponent<ParticleSystem>().IsAlive(true) || GetComponent<ParticleSystem>().isStopped)
                {
                    if (OnlyDeactivate)
                    {
#if UNITY_3_5
					this.gameObject.SetActiveRecursively(false);
#else
                        this.gameObject.SetActive(false);
#endif
                    }
                    else
                        GameObject.Destroy(this.gameObject);
                    break;
                }
            }
        }

        #endregion
    }
}
